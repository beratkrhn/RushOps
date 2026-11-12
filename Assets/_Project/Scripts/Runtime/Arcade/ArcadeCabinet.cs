using System;
using RushOps.Endless;
using RushOps.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace RushOps.Arcade
{
    /// <summary>
    /// Shows the endless run inside the arcade cabinet when <see cref="EndlessStart.Arcade"/> is set (F-ARC-01 to
    /// F-ARC-03, AR1, AR2). A pure presentation layer: the game camera renders into a texture of
    /// <see cref="ArcadeConfig.ScreenResolution"/> (pixel perfect, about 10x10 tiles), a second camera draws the
    /// cabinet on the display, and the HUD with pause, upgrade choice and result screen moves into the screen and is
    /// scaled down to it. The rules of the run stay untouched. Without the arcade flag nothing changes.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ArcadeCabinet : MonoBehaviour
    {
        private const string ScreenRootName = "ArcadeScreen";

        [SerializeField] private ArcadeConfig _config;
        [SerializeField] private Camera _gameCamera;
        [Tooltip("Draws the cabinet to the display; inactive outside the arcade mode.")]
        [SerializeField] private Camera _displayCamera;
        [Tooltip("Canvas with the cabinet; inactive outside the arcade mode.")]
        [SerializeField] private Canvas _cabinetCanvas;
        [Tooltip("Root of the cabinet parts, laid out in reference pixels (origin top left).")]
        [SerializeField] private RectTransform _cabinet;
        [SerializeField] private Image _background;
        [Tooltip("Shows the game texture; draws it with the CRT effect (issue #323).")]
        [SerializeField] private RawImage _screen;
        [Tooltip("Reflection on the glass of the screen.")]
        [SerializeField] private Image _gloss;
        [Tooltip("Canvas of the HUD; its content moves into the screen.")]
        [SerializeField] private Canvas _hud;
        [Tooltip("The usual touch sticks; off in the cabinet, where the drawn joysticks take over (issue #322).")]
        [SerializeField] private GameObject _touchControls;

        private static readonly int CurvatureId = Shader.PropertyToID("_Curvature");
        private static readonly int ScanlinesId = Shader.PropertyToID("_Scanlines");
        private static readonly int LinesId = Shader.PropertyToID("_Lines");
        private static readonly int VignetteId = Shader.PropertyToID("_Vignette");

        private RenderTexture _texture;
        private Material _crt;
        private RectTransform _screenRoot;
        private Vector2 _appliedDisplay;
        private Rect _appliedSafeArea;
        private Vector2 _appliedHudCanvas;
        private Vector2 _appliedCabinetCanvas;

        /// <summary>Raised after the cabinet was laid out anew, e.g. when the display or the safe area changed.</summary>
        public event Action LayoutChanged;

        /// <summary>True while the run shows in the cabinet.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Values of the cabinet.</summary>
        public ArcadeConfig Config => _config;

        /// <summary>Texture the game camera renders into while the cabinet is active.</summary>
        public RenderTexture ScreenTexture => _texture;

        /// <summary>Material the screen draws the game with; a copy of the CRT material with the config values.</summary>
        public Material ScreenMaterial => _crt;

        /// <summary>Camera that draws the cabinet to the display.</summary>
        public Camera DisplayCamera => _displayCamera;

        /// <summary>Canvas with the cabinet parts.</summary>
        public Canvas CabinetCanvas => _cabinetCanvas;

        /// <summary>Root of the cabinet parts in reference pixels; children are anchored top left.</summary>
        public RectTransform Cabinet => _cabinet;

        /// <summary>Current layout on the display.</summary>
        public ArcadeLayout Layout { get; private set; }

        private void Awake()
        {
            if (!EndlessStart.Arcade || _config == null || _gameCamera == null)
            {
                SetCabinetVisible(false);
                return;
            }

            Activate();
        }

        private void LateUpdate()
        {
            if (IsActive && NeedsLayout())
            {
                ApplyLayout();
            }
        }

        private void OnDestroy()
        {
            if (_gameCamera != null && _gameCamera.targetTexture == _texture)
            {
                _gameCamera.targetTexture = null;
            }

            if (_texture != null)
            {
                _texture.Release();
                Destroy(_texture);
                _texture = null;
            }

            if (_crt != null)
            {
                Destroy(_crt);
                _crt = null;
            }
        }

        /// <summary>Applies the CRT strengths of the config to the screen, e.g. after they changed.</summary>
        public void ApplyCrt()
        {
            if (_screen == null)
            {
                return;
            }

            if (_crt == null && _config.CrtMaterial != null)
            {
                _crt = new Material(_config.CrtMaterial) { name = "ArcadeCRT (run)" };
                _screen.material = _crt;
            }

            if (_crt != null)
            {
                _crt.SetFloat(CurvatureId, _config.CrtCurvature);
                _crt.SetFloat(ScanlinesId, _config.CrtScanlines);
                _crt.SetFloat(LinesId, _config.ScreenResolution.y);
                _crt.SetFloat(VignetteId, _config.CrtVignette);
            }

            if (_gloss != null)
            {
                _gloss.color = new Color(1f, 1f, 1f, _config.CrtGloss);
            }
        }

        private void Activate()
        {
            IsActive = true;
            Vector2Int resolution = _config.ScreenResolution;
            _texture = new RenderTexture(resolution.x, resolution.y, 24)
            {
                name = "ArcadeScreen",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            _texture.Create();
            _gameCamera.targetTexture = _texture;
            if (_gameCamera.TryGetComponent(out PixelPerfectCamera pixelPerfect))
            {
                pixelPerfect.refResolutionX = resolution.x;
                pixelPerfect.refResolutionY = resolution.y;
                // The Pixel Perfect Camera sets the same size when it renders; set it now for the first frame.
                _gameCamera.orthographicSize = resolution.y / (2f * pixelPerfect.assetsPPU);
            }

            // Without a graphics device (batch mode tests with -nographics) rendering into a texture crashes Unity;
            // the camera keeps its texture and size, so everything that reads them still works.
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                _gameCamera.enabled = false;
            }

            if (_screen != null)
            {
                _screen.texture = _texture;
            }

            ApplyCrt();

            if (_displayCamera != null)
            {
                _displayCamera.backgroundColor = _config.Background;
            }

            if (_background != null)
            {
                _background.color = Color.white;
            }

            if (_touchControls != null)
            {
                _touchControls.SetActive(false);
            }

            SetCabinetVisible(true);
            MoveHudIntoScreen();
            ApplyLayout();
        }

        private void SetCabinetVisible(bool visible)
        {
            if (_displayCamera != null)
            {
                _displayCamera.gameObject.SetActive(visible);
            }

            if (_cabinetCanvas != null)
            {
                _cabinetCanvas.gameObject.SetActive(visible);
            }
        }

        // Everything on the HUD canvas (flash, safe area with hearts and score, pause overlay, results, upgrade choice)
        // moves under one square root that is scaled into the screen; the safe area is the screen now.
        private void MoveHudIntoScreen()
        {
            if (_hud == null)
            {
                return;
            }

            var hudRoot = (RectTransform)_hud.transform;
            var go = new GameObject(ScreenRootName, typeof(RectTransform));
            _screenRoot = (RectTransform)go.transform;
            _screenRoot.SetParent(hudRoot, false);
            int count = hudRoot.childCount;
            var children = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                children[i] = hudRoot.GetChild(i);
            }

            foreach (Transform child in children)
            {
                if (child == _screenRoot)
                {
                    continue;
                }

                child.SetParent(_screenRoot, false);
                if (child.TryGetComponent(out SafeAreaFitter fitter))
                {
                    fitter.enabled = false;
                    var rect = (RectTransform)child;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                }
            }
        }

        private Vector2 DisplaySize()
        {
            Camera camera = _displayCamera != null ? _displayCamera : _gameCamera;
            if (camera != null && camera.targetTexture != null)
            {
                return new Vector2(camera.pixelWidth, camera.pixelHeight);
            }

            return new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
        }

        // A camera rendering into a texture (e.g. for a screenshot) has no notch: its whole picture is safe.
        private Rect SafeArea(Vector2 display)
        {
            Camera camera = _displayCamera != null ? _displayCamera : _gameCamera;
            return camera != null && camera.targetTexture != null ? new Rect(Vector2.zero, display) : UnityEngine.Screen.safeArea;
        }

        private static Vector2 CanvasSize(Canvas canvas)
        {
            return canvas != null ? ((RectTransform)canvas.transform).rect.size : Vector2.zero;
        }

        private bool NeedsLayout()
        {
            Vector2 display = DisplaySize();
            return display != _appliedDisplay || SafeArea(display) != _appliedSafeArea
                || CanvasSize(_hud) != _appliedHudCanvas || CanvasSize(_cabinetCanvas) != _appliedCabinetCanvas;
        }

        private void ApplyLayout()
        {
            _appliedDisplay = DisplaySize();
            _appliedSafeArea = SafeArea(_appliedDisplay);
            _appliedHudCanvas = CanvasSize(_hud);
            _appliedCabinetCanvas = CanvasSize(_cabinetCanvas);
            if (_appliedDisplay.x <= 0f || _appliedDisplay.y <= 0f)
            {
                return;
            }

            Layout = ArcadeLayout.Fit(_appliedSafeArea, _config.Reference);

            if (_cabinet != null && _appliedCabinetCanvas.x > 0f)
            {
                float units = _appliedCabinetCanvas.x / _appliedDisplay.x;
                _cabinet.anchorMin = Vector2.zero;
                _cabinet.anchorMax = Vector2.zero;
                _cabinet.pivot = Vector2.zero;
                _cabinet.anchoredPosition = Layout.Cabinet.position * units;
                _cabinet.sizeDelta = _config.Reference;
                _cabinet.localScale = Vector3.one * (Layout.Scale * units);
                FitBackgroundTiles(units);
            }

            if (_screenRoot != null && _appliedHudCanvas.x > 0f)
            {
                float units = _appliedHudCanvas.x / _appliedDisplay.x;
                Rect screen = Layout.ToDisplay(_config.Screen);
                float size = _config.HudSize;
                _screenRoot.anchorMin = Vector2.zero;
                _screenRoot.anchorMax = Vector2.zero;
                _screenRoot.pivot = new Vector2(0.5f, 0.5f);
                _screenRoot.anchoredPosition = screen.center * units;
                _screenRoot.sizeDelta = new Vector2(size, size * screen.height / Mathf.Max(screen.width, 1f));
                _screenRoot.localScale = Vector3.one * (screen.width * units / size);
            }

            LayoutChanged?.Invoke();
        }

        // One tile pixel of the cabinet side equals one reference pixel of the cabinet.
        private void FitBackgroundTiles(float units)
        {
            if (_background == null || _background.sprite == null || _cabinetCanvas == null)
            {
                return;
            }

            float tileUnits = _background.sprite.rect.width * Layout.Scale * units;
            float naturalUnits = _background.sprite.rect.width * _cabinetCanvas.referencePixelsPerUnit
                / _background.sprite.pixelsPerUnit;
            _background.pixelsPerUnitMultiplier = tileUnits > 0f ? naturalUnits / tileUnits : 1f;
        }
    }
}
