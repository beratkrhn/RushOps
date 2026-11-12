using UnityEngine;

namespace RushOps.Arcade
{
    /// <summary>
    /// Values of the arcade mode (F-ARC-01 to F-ARC-05): where the parts of the cabinet sit in the reference grid
    /// (copied from <c>Art/Sprites/Arcade/Arcade_Layout.json</c> by „RushOps → Arcade → Arcade-Daten aktualisieren“),
    /// their sprites, the resolution of the screen and how large the HUD is drawn inside it.
    /// Rectangles are in reference pixels with the origin top left.
    /// </summary>
    [CreateAssetMenu(fileName = "ArcadeConfig", menuName = "RushOps/Arcade Config")]
    public class ArcadeConfig : ScriptableObject
    {
        [Header("Layout (reference pixels, origin top left)")]
        [SerializeField] private Vector2Int _reference = new Vector2Int(360, 640);
        [SerializeField] private RectInt _marquee;
        [SerializeField] private RectInt _bezel;
        [SerializeField] private RectInt _screen;
        [SerializeField] private RectInt _panel;
        [SerializeField] private RectInt _stickLeft;
        [SerializeField] private RectInt _stickRight;
        [SerializeField] private RectInt _dashButton;
        [SerializeField] private RectInt[] _decoButtons = System.Array.Empty<RectInt>();

        [Header("Screen")]
        [Tooltip("Pixels the game camera renders into; 320x320 shows 10x10 tiles at 32 px per tile (start value).")]
        [SerializeField] private Vector2Int _screenResolution = new Vector2Int(320, 320);
        [Tooltip("Size in HUD units of the square the HUD lays out in; it is scaled down to the screen. Larger values draw the HUD smaller.")]
        [SerializeField, Min(100f)] private float _hudSize = 560f;
        [Tooltip("Color behind the cabinet where the display is wider or taller than the cabinet.")]
        [SerializeField] private Color _background = new Color(31f / 255f, 29f / 255f, 43f / 255f);

        [Header("Sprites")]
        [SerializeField] private Sprite _marqueeSprite;
        [SerializeField] private Sprite _bezelSprite;
        [SerializeField] private Sprite _screenFrameSprite;
        [SerializeField] private Sprite _screenGlossSprite;
        [SerializeField] private Sprite _panelSprite;
        [SerializeField] private Sprite _bodySprite;
        [Tooltip("Stick at rest, then tilted to E, NE, N, NW, W, SW, S, SE.")]
        [SerializeField] private Sprite[] _stickSprites = System.Array.Empty<Sprite>();
        [SerializeField] private Sprite _dashSprite;
        [SerializeField] private Sprite _dashPressedSprite;
        [SerializeField] private Sprite[] _decoSprites = System.Array.Empty<Sprite>();

        /// <summary>Size of the whole cabinet in reference pixels.</summary>
        public Vector2Int Reference => _reference;

        /// <summary>Marquee with the logo.</summary>
        public RectInt Marquee => _marquee;

        /// <summary>Painted frame around the screen.</summary>
        public RectInt Bezel => _bezel;

        /// <summary>The screen the game shows in.</summary>
        public RectInt Screen => _screen;

        /// <summary>Control panel with the sticks and buttons.</summary>
        public RectInt Panel => _panel;

        /// <summary>Canvas of the left stick (move).</summary>
        public RectInt StickLeft => _stickLeft;

        /// <summary>Canvas of the right stick (aim).</summary>
        public RectInt StickRight => _stickRight;

        /// <summary>Canvas of the dash button.</summary>
        public RectInt DashButton => _dashButton;

        /// <summary>Canvases of the decorative buttons.</summary>
        public RectInt[] DecoButtons => _decoButtons;

        /// <summary>Pixels the game camera renders into.</summary>
        public Vector2Int ScreenResolution => _screenResolution;

        /// <summary>Size in HUD units of the square the HUD lays out in.</summary>
        public float HudSize => _hudSize;

        /// <summary>Color behind the cabinet.</summary>
        public Color Background => _background;

        /// <summary>Marquee sprite.</summary>
        public Sprite MarqueeSprite => _marqueeSprite;

        /// <summary>Bezel sprite with the hole for the screen.</summary>
        public Sprite BezelSprite => _bezelSprite;

        /// <summary>Rounded corners laid over the game image.</summary>
        public Sprite ScreenFrameSprite => _screenFrameSprite;

        /// <summary>Reflection on the glass.</summary>
        public Sprite ScreenGlossSprite => _screenGlossSprite;

        /// <summary>Control panel sprite.</summary>
        public Sprite PanelSprite => _panelSprite;

        /// <summary>Tile of the cabinet side behind everything.</summary>
        public Sprite BodySprite => _bodySprite;

        /// <summary>Stick at rest (0) and tilted to the 8 directions from E counter-clockwise (1 to 8).</summary>
        public Sprite[] StickSprites => _stickSprites;

        /// <summary>Dash button at rest.</summary>
        public Sprite DashSprite => _dashSprite;

        /// <summary>Dash button pressed.</summary>
        public Sprite DashPressedSprite => _dashPressedSprite;

        /// <summary>Sprites of the decorative buttons, in the order of <see cref="DecoButtons"/>.</summary>
        public Sprite[] DecoSprites => _decoSprites;

        /// <summary>Sets the rectangles; used by the editor builder.</summary>
        public void SetLayout(Vector2Int reference, RectInt marquee, RectInt bezel, RectInt screen, RectInt panel,
            RectInt stickLeft, RectInt stickRight, RectInt dashButton, RectInt[] decoButtons)
        {
            _reference = reference;
            _marquee = marquee;
            _bezel = bezel;
            _screen = screen;
            _panel = panel;
            _stickLeft = stickLeft;
            _stickRight = stickRight;
            _dashButton = dashButton;
            _decoButtons = decoButtons ?? System.Array.Empty<RectInt>();
        }

        /// <summary>Sets the sprites; used by the editor builder.</summary>
        public void SetSprites(Sprite marquee, Sprite bezel, Sprite screenFrame, Sprite screenGloss, Sprite panel,
            Sprite body, Sprite[] sticks, Sprite dash, Sprite dashPressed, Sprite[] deco)
        {
            _marqueeSprite = marquee;
            _bezelSprite = bezel;
            _screenFrameSprite = screenFrame;
            _screenGlossSprite = screenGloss;
            _panelSprite = panel;
            _bodySprite = body;
            _stickSprites = sticks ?? System.Array.Empty<Sprite>();
            _dashSprite = dash;
            _dashPressedSprite = dashPressed;
            _decoSprites = deco ?? System.Array.Empty<Sprite>();
        }
    }
}
