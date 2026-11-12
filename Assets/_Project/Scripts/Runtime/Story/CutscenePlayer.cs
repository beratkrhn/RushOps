using System;
using RushOps.Combat;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RushOps.Story
{
    /// <summary>
    /// Plays a <see cref="CutsceneDefinition"/> on a uGUI canvas (issue #331, F-STO-02): the panel picture at a whole
    /// pixel scale in the middle of the screen, its text boxes fading and sliding in one after another, a fade through
    /// black between panels, a blinking „Weiter“ once the panel is complete and a button „Überspringen“. A tap anywhere
    /// shows the next box or moves on. Thin adapter; the rules live in <see cref="CutscenePlayback"/>.
    /// </summary>
    public class CutscenePlayer : MonoBehaviour
    {
        /// <summary>Text of the hint once a panel is complete.</summary>
        public const string HintText = "Weiter";

        /// <summary>Text of the skip button.</summary>
        public const string SkipText = "Überspringen";

        // Panel pixels between the last box and the hint, and the strip at the bottom kept for „Überspringen“.
        private const float HintGap = 2f;
        private const float HintBottomMargin = 18f;

        [SerializeField] private CutsceneDefinition _cutscene;
        [SerializeField] private CutsceneConfig _config;
        [Tooltip("Starts the cutscene on Start; otherwise call Play.")]
        [SerializeField] private bool _playOnStart = true;

        [Header("Views")]
        [SerializeField] private Font _font;
        [SerializeField] private int _fontSize = 10;
        [SerializeField] private RectTransform _panelRoot;
        [SerializeField] private Image _panelImage;
        [SerializeField] private RectTransform _boxRoot;
        [SerializeField] private Graphic _cover;
        [SerializeField] private Graphic _hint;
        [SerializeField] private Button _tapButton;
        [SerializeField] private Button _skipButton;

        [Tooltip("Called once when the cutscene ends, played through or skipped.")]
        [SerializeField] private UnityEvent _finished = new UnityEvent();

        private CutscenePlayback _playback;
        private CutsceneBoxView[] _boxes = Array.Empty<CutsceneBoxView>();
        private float[] _boxAlpha = Array.Empty<float>();
        private int _shownPanel = -1;
        private float _coverAlpha = -1f;
        private float _hintTime;

        /// <summary>Raised once when the cutscene ends, played through or skipped.</summary>
        public event Action Finished;

        /// <summary>The running playback; null before <see cref="Play"/>.</summary>
        public CutscenePlayback Playback => _playback;

        /// <summary>True while the cutscene runs.</summary>
        public bool IsPlaying => _playback != null && !_playback.IsFinished;

        /// <summary>True while the „Weiter“ hint shows.</summary>
        public bool HintVisible => _hint != null && _hint.enabled;

        /// <summary>The view of box <paramref name="index"/> of the current panel, for tests.</summary>
        public CutsceneBoxView GetBoxView(int index)
        {
            return _boxes[index];
        }

        /// <summary>Starts <paramref name="cutscene"/> from its first panel.</summary>
        public void Play(CutsceneDefinition cutscene)
        {
            if (cutscene == null || _config == null)
            {
                return;
            }

            if (_playback != null)
            {
                _playback.Finished -= OnFinished;
            }

            _cutscene = cutscene;
            EnsureBoxes(cutscene.MaxBoxes);
            if (_panelRoot != null)
            {
                _panelRoot.sizeDelta = cutscene.PanelSize;
            }

            _shownPanel = -1;
            _coverAlpha = -1f;
            _hintTime = 0f;
            _playback = new CutscenePlayback(cutscene.BoxCounts(), _config.Timing);
            _playback.Finished += OnFinished;
            if (_playback.IsFinished)
            {
                OnFinished();
                return;
            }

            Refresh();
        }

        /// <summary>A tap on the screen: next box or next panel.</summary>
        public void Tap()
        {
            if (IsPlaying)
            {
                _playback.Tap();
                Refresh();
            }
        }

        /// <summary>Ends the cutscene at once.</summary>
        public void Skip()
        {
            if (IsPlaying)
            {
                _playback.Skip();
            }
        }

        // Wired in Start, not Awake, so a player made by Create at runtime is complete before it listens.
        private void Start()
        {
            if (_tapButton != null)
            {
                _tapButton.onClick.AddListener(Tap);
            }

            if (_skipButton != null)
            {
                _skipButton.onClick.AddListener(Skip);
            }

            SetHint(false);
            if (_playOnStart && _cutscene != null && _playback == null)
            {
                Play(_cutscene);
            }
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            _playback.Tick(Time.unscaledDeltaTime);
            if (IsPlaying)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            int panel = _playback.PanelIndex;
            if (panel != _shownPanel)
            {
                ShowPanel(panel);
            }

            SetCover(_playback.CoverAlpha);
            int count = _playback.BoxCount;
            for (int i = 0; i < count; i++)
            {
                float alpha = _playback.BoxAlpha(i);
                if (!Mathf.Approximately(alpha, _boxAlpha[i]))
                {
                    _boxAlpha[i] = alpha;
                    _boxes[i].SetReveal(alpha, _config.BoxSlide);
                }
            }

            bool waiting = _playback.WaitsForTap;
            _hintTime = waiting ? _hintTime + Time.unscaledDeltaTime : 0f;
            SetHint(waiting && !BlinkMath.IsVisible(_hintTime, _config.HintBlink));
        }

        private void ShowPanel(int index)
        {
            _shownPanel = index;
            CutscenePanel panel = _cutscene.GetPanel(index);
            if (_panelImage != null)
            {
                _panelImage.sprite = panel.Image;
                _panelImage.enabled = panel.Image != null;
            }

            for (int i = 0; i < _boxes.Length; i++)
            {
                if (i < panel.BoxCount)
                {
                    _boxes[i].Show(panel.GetBox(i), _config);
                    _boxAlpha[i] = 0f;
                }
                else
                {
                    _boxes[i].Hide();
                }
            }

            PlaceHint(panel.BoxCount > 0 ? _boxes[panel.BoxCount - 1].BottomRight : (Vector2)_cutscene.PanelSize * new Vector2(1f, -1f));
        }

        // The hint sits right under the last box of the panel, so it never covers text; without boxes in the corner.
        private void PlaceHint(Vector2 bottomRight)
        {
            if (_hint == null)
            {
                return;
            }

            RectTransform rect = _hint.rectTransform;
            float lowest = -_cutscene.PanelSize.y + HintBottomMargin + rect.sizeDelta.y;
            rect.anchoredPosition = new Vector2(bottomRight.x, Mathf.Max(bottomRight.y - HintGap, lowest));
        }

        private void EnsureBoxes(int count)
        {
            if (_boxes.Length >= count)
            {
                return;
            }

            var boxes = new CutsceneBoxView[count];
            Array.Copy(_boxes, boxes, _boxes.Length);
            for (int i = _boxes.Length; i < count; i++)
            {
                boxes[i] = CutsceneBoxView.Create(_boxRoot, _font, _fontSize, "Box" + i);
                boxes[i].Hide();
            }

            _boxes = boxes;
            _boxAlpha = new float[count];
        }

        private void SetCover(float alpha)
        {
            if (_cover == null || Mathf.Approximately(alpha, _coverAlpha))
            {
                return;
            }

            _coverAlpha = alpha;
            Color color = _cover.color;
            color.a = alpha;
            _cover.color = color;
        }

        private void SetHint(bool visible)
        {
            if (_hint != null && _hint.enabled != visible)
            {
                _hint.enabled = visible;
            }
        }

        private void OnFinished()
        {
            SetCover(1f);
            SetHint(false);
            _finished?.Invoke();
            Finished?.Invoke();
        }

        /// <summary>
        /// Builds a cutscene player under <paramref name="canvasRoot"/> (a full-screen rect of a reference-sized canvas):
        /// a black tap area over the whole screen, the panel in the middle scaled by <paramref name="panelScale"/>, the
        /// black cover, the hint and the skip button. Used by the scene builder and by tests.
        /// </summary>
        public static CutscenePlayer Create(RectTransform canvasRoot, Font font, int fontSize, CutsceneConfig config,
            int panelScale, Color background)
        {
            RectTransform root = CreateRect("Cutscene", canvasRoot, Vector2.zero, Vector2.one);
            var player = root.gameObject.AddComponent<CutscenePlayer>();
            player._config = config;
            player._font = font;
            player._fontSize = fontSize;

            var tapImage = root.gameObject.AddComponent<Image>();
            tapImage.color = background;
            player._tapButton = root.gameObject.AddComponent<Button>();
            player._tapButton.targetGraphic = tapImage;
            player._tapButton.transition = Selectable.Transition.None;

            var center = new Vector2(0.5f, 0.5f);
            RectTransform panel = CreateRect("Panel", root, center, center);
            panel.sizeDelta = new Vector2(180f, 320f);
            panel.localScale = new Vector3(panelScale, panelScale, 1f);
            player._panelRoot = panel;

            RectTransform image = CreateRect("Image", panel, Vector2.zero, Vector2.one);
            player._panelImage = image.gameObject.AddComponent<Image>();
            player._panelImage.raycastTarget = false;

            player._boxRoot = CreateRect("Boxes", panel, Vector2.zero, Vector2.one);

            RectTransform cover = CreateRect("Cover", panel, Vector2.zero, Vector2.one);
            var coverImage = cover.gameObject.AddComponent<Image>();
            coverImage.color = background;
            coverImage.raycastTarget = false;
            player._cover = coverImage;

            Text hint = CreateLabel("Hint", panel, font, fontSize, HintText, new Vector2(0f, 1f), new Vector2(176f, -300f));
            hint.rectTransform.pivot = new Vector2(1f, 1f);
            hint.alignment = TextAnchor.UpperRight;
            player._hint = hint;

            Text skip = CreateLabel("Skip", panel, font, fontSize, SkipText, new Vector2(0f, 0f), new Vector2(4f, 4f));
            skip.alignment = TextAnchor.LowerLeft;
            skip.raycastTarget = true;
            player._skipButton = skip.gameObject.AddComponent<Button>();
            player._skipButton.targetGraphic = skip;
            player._skipButton.transition = Selectable.Transition.None;
            return player;
        }

        private static Text CreateLabel(string name, RectTransform parent, Font font, int fontSize, string content,
            Vector2 corner, Vector2 offset)
        {
            RectTransform rect = CreateRect(name, parent, corner, corner);
            rect.pivot = corner;
            rect.sizeDelta = new Vector2(80f, 14f);
            rect.anchoredPosition = offset;
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.text = content;
            text.color = new Color32(244, 241, 232, 255);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color32(13, 11, 20, 255);
            outline.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }
    }
}
