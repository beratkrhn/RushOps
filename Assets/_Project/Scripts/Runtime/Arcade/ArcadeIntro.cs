using RushOps.Characters;
using RushOps.Save;
using RushOps.Skins;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RushOps.Arcade
{
    /// <summary>
    /// Transition from the menu to the cabinet (F-ARC-07, AR5, issue #324): the menu shrinks away while an arcade hall
    /// with the cabinet fades in, the figure in the current skin walks up to it, then the view dives into the screen
    /// and the run starts. A tap skips it (a full-screen button on the overlay calls <see cref="Skip"/>). Coming back from an arcade run, the menu fades in briefly from black.
    /// The timing lives in <see cref="ArcadeIntroTimeline"/>, the values in <see cref="ArcadeConfig"/>.
    /// </summary>
    public class ArcadeIntro : MonoBehaviour
    {
        [SerializeField] private ArcadeConfig _config;
        [SerializeField] private SaveGameService _save;
        [SerializeField] private SkinCatalog _catalog;
        [Tooltip("Content of the menu that shrinks away.")]
        [SerializeField] private RectTransform _menu;
        [Tooltip("Root of the overlay; inactive while no transition plays.")]
        [SerializeField] private GameObject _overlay;
        [Tooltip("Fades the arcade hall in.")]
        [SerializeField] private CanvasGroup _hall;
        [Tooltip("The small cabinet; its pivot is the centre of its screen.")]
        [SerializeField] private RectTransform _cabinet;
        [Tooltip("The figure, pivoted at its feet.")]
        [SerializeField] private Image _figure;
        [Tooltip("Black over everything: the end of the dive and the fade back into the menu.")]
        [SerializeField] private Image _black;

        private ArcadeIntroTimeline _timeline;
        private DirectionalAnimation _walk;
        private string _scene;
        private float _time;
        private float _fadeIn = -1f;
        private Vector2 _cabinetRest;
        private float _cabinetRestScale;

        /// <summary>Values of the transition.</summary>
        public ArcadeConfig Config => _config;

        /// <summary>True while the transition plays.</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>Seconds since the transition started.</summary>
        public float Elapsed => _time;

        /// <summary>Phase now.</summary>
        public ArcadeIntroPhase Phase => _timeline.PhaseAt(_time);

        /// <summary>Length of the whole transition in seconds.</summary>
        public float Duration => _timeline.Duration;

        /// <summary>Plays the transition and loads <paramref name="scene"/> at its end.</summary>
        public void Play(string scene)
        {
            if (IsPlaying)
            {
                return;
            }

            _scene = scene;
            _time = 0f;
            _timeline = new ArcadeIntroTimeline(_config.IntroZoomOut, _config.IntroWalk, _config.IntroZoomIn);
            _walk = WalkAnimation();
            IsPlaying = true;
            if (_overlay != null)
            {
                _overlay.SetActive(true);
            }

            Apply();
        }

        /// <summary>Ends the transition at once and starts the run.</summary>
        public void Skip()
        {
            if (IsPlaying)
            {
                Finish();
            }
        }

        private void Awake()
        {
            _timeline = new ArcadeIntroTimeline(0f, 0f, 0f);
            if (_cabinet != null)
            {
                _cabinetRest = _cabinet.anchoredPosition;
                _cabinetRestScale = _cabinet.localScale.x;
            }

            if (_overlay != null)
            {
                _overlay.SetActive(false);
            }

            if (ArcadeTransition.FadeInMenu)
            {
                ArcadeTransition.FadeInMenu = false;
                _fadeIn = 0f;
                ShowBlack(1f);
            }
            else
            {
                ShowBlack(0f);
            }
        }

        private void Update()
        {
            float delta = UnityEngine.Time.unscaledDeltaTime;
            if (_fadeIn >= 0f && _config != null)
            {
                _fadeIn += delta;
                float length = _config.MenuFadeIn;
                ShowBlack(length > 0f ? 1f - Mathf.Clamp01(_fadeIn / length) : 0f);
                if (_fadeIn >= length)
                {
                    _fadeIn = -1f;
                }
            }

            if (!IsPlaying)
            {
                return;
            }

            _time += delta;
            if (_timeline.IsDone(_time))
            {
                Finish();
                return;
            }

            Apply();
        }

        private void Apply()
        {
            float zoomOut = ArcadeIntroTimeline.Ease(_timeline.Progress(ArcadeIntroPhase.ZoomOut, _time));
            float walk = _timeline.Progress(ArcadeIntroPhase.Walk, _time);
            float zoomIn = ArcadeIntroTimeline.Ease(_timeline.Progress(ArcadeIntroPhase.ZoomIn, _time));

            if (_menu != null)
            {
                _menu.localScale = Vector3.one * Mathf.Lerp(1f, _config.IntroMenuScale, zoomOut);
            }

            if (_hall != null)
            {
                _hall.alpha = zoomOut;
            }

            if (_figure != null)
            {
                Vector2 start = _config.IntroFigureStart;
                Vector2 end = _config.IntroFigureEnd;
                ((RectTransform)_figure.transform).anchoredPosition = Vector2.Lerp(start, end, walk);
                if (_walk != null && _walk.FrameCount(Direction8.N) > 0)
                {
                    bool walking = walk > 0f && walk < 1f;
                    int frame = walking ? _walk.FrameIndexAt(_time, Direction8.N) : 0;
                    _figure.sprite = _walk.GetFrame(Direction8.N, frame);
                }
            }

            if (_cabinet != null)
            {
                _cabinet.anchoredPosition = Vector2.Lerp(_cabinetRest, Vector2.zero, zoomIn);
                _cabinet.localScale = Vector3.one * Mathf.Lerp(_cabinetRestScale, _config.IntroDiveScale, zoomIn);
            }

            // The last part of the dive goes black, so the run starts from black.
            ShowBlack(Mathf.Clamp01((zoomIn - 0.6f) / 0.4f));
        }

        private void Finish()
        {
            IsPlaying = false;
            _time = _timeline.Duration;
            ShowBlack(1f);
            if (!string.IsNullOrEmpty(_scene))
            {
                UnityEngine.Time.timeScale = 1f;
                SceneManager.LoadScene(_scene);
            }
        }

        private void ShowBlack(float alpha)
        {
            if (_black == null)
            {
                return;
            }

            Color color = _black.color;
            color.a = alpha;
            _black.color = color;
            _black.enabled = alpha > 0f;
        }

        private DirectionalAnimation WalkAnimation()
        {
            if (_catalog == null || _save == null)
            {
                return null;
            }

            SkinDefinition skin = _catalog.Current(_save.Data);
            return skin != null ? skin.Walk : null;
        }
    }
}
