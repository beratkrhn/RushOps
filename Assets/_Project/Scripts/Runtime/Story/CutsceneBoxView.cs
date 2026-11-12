using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Story
{
    /// <summary>
    /// Shows one <see cref="CutsceneBox"/> (issue #331): an outlined box in the pixel font that sizes itself to its
    /// text and fades and slides in. Lives inside the panel root, whose units are panel pixels. Laid out once per panel
    /// by <see cref="Show"/>; <see cref="SetReveal"/> runs every frame without allocations.
    /// </summary>
    public class CutsceneBoxView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Image _outline;
        [SerializeField] private Image _fill;
        [SerializeField] private Text _speaker;
        [SerializeField] private Text _body;

        private float _baseY;

        /// <summary>Text of the box body, for tests.</summary>
        public string BodyText => _body != null ? _body.text : "";

        /// <summary>Text of the speaker line, for tests.</summary>
        public string SpeakerText => _speaker != null && _speaker.gameObject.activeSelf ? _speaker.text : "";

        /// <summary>Current opacity.</summary>
        public float Alpha => _group != null ? _group.alpha : 0f;

        /// <summary>Lays the box out for <paramref name="box"/> and hides it until <see cref="SetReveal"/>.</summary>
        public void Show(CutsceneBox box, CutsceneConfig config)
        {
            gameObject.SetActive(true);
            int padding = config.BoxPadding;
            float inner = Mathf.Max(1f, box.Width - 2f * padding);

            bool hasSpeaker = box.HasSpeaker;
            _speaker.gameObject.SetActive(hasSpeaker);
            float top = padding;
            if (hasSpeaker)
            {
                _speaker.text = box.Speaker;
                _speaker.color = box.SpeakerColor;
                Place(_speaker.rectTransform, padding, top, inner, config.SpeakerLineHeight);
                top += config.SpeakerLineHeight;
            }

            _body.text = box.Text;
            _body.color = config.Text;
            Place(_body.rectTransform, padding, top, inner, 0f);
            float bodyHeight = Mathf.Ceil(_body.preferredHeight);
            Place(_body.rectTransform, padding, top, inner, bodyHeight);

            _outline.color = config.Outline;
            _fill.color = box.Kind == CutsceneBoxKind.Narration ? config.NarrationFill : config.SpeechFill;

            _baseY = -box.Position.y;
            _rect.sizeDelta = new Vector2(box.Width, top + bodyHeight + padding);
            _rect.anchoredPosition = new Vector2(box.Position.x, _baseY);
            SetReveal(0f, 0f);
        }

        /// <summary>Fades the box: 0 hidden and <paramref name="slide"/> pixels low, 1 fully in place.</summary>
        public void SetReveal(float alpha, float slide)
        {
            _group.alpha = alpha;
            Vector2 position = _rect.anchoredPosition;
            position.y = _baseY - (1f - alpha) * slide;
            _rect.anchoredPosition = position;
        }

        /// <summary>Hides the box.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Builds a box under <paramref name="parent"/> (units are panel pixels). Used by the scene builder and by tests,
        /// so both get the same hierarchy.
        /// </summary>
        public static CutsceneBoxView Create(RectTransform parent, Font font, int fontSize, string name = "Box")
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            var view = rect.gameObject.AddComponent<CutsceneBoxView>();
            view._rect = rect;
            view._group = rect.gameObject.AddComponent<CanvasGroup>();
            view._group.blocksRaycasts = false;
            view._group.interactable = false;
            view._outline = rect.gameObject.AddComponent<Image>();
            view._outline.raycastTarget = false;

            RectTransform fill = CreateRect("Fill", rect);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.one;
            fill.offsetMax = -Vector2.one;
            view._fill = fill.gameObject.AddComponent<Image>();
            view._fill.raycastTarget = false;

            view._speaker = CreateText("Speaker", rect, font, fontSize);
            view._body = CreateText("Body", rect, font, fontSize);
            return view;
        }

        private static Text CreateText(string name, RectTransform parent, Font font, int fontSize)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        // Top left corner at (x, y) below the top left of the box.
        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
