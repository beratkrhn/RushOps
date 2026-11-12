using UnityEngine;

namespace RushOps.Arcade
{
    /// <summary>
    /// Where the arcade cabinet sits on the display (F-ARC-02, F-PLT-05): the cabinet of the reference grid
    /// (360x640) scaled as large as it fits into the safe area, centred horizontally and standing on the bottom of the
    /// safe area, so the control panel stays near the thumbs. Converts rectangles of the reference grid (origin top
    /// left, as the artwork is laid out) into display pixels (origin bottom left, as Unity counts them).
    /// </summary>
    public readonly struct ArcadeLayout
    {
        private readonly Vector2 _reference;

        private ArcadeLayout(Rect cabinet, float scale, Vector2 reference)
        {
            Cabinet = cabinet;
            Scale = scale;
            _reference = reference;
        }

        /// <summary>The whole cabinet in display pixels.</summary>
        public Rect Cabinet { get; }

        /// <summary>Display pixels per reference pixel.</summary>
        public float Scale { get; }

        /// <summary>
        /// Fits a cabinet of <paramref name="reference"/> pixels into <paramref name="safeArea"/> (display pixels).
        /// An empty safe area or reference gives a layout of scale 0.
        /// </summary>
        public static ArcadeLayout Fit(Rect safeArea, Vector2Int reference)
        {
            if (reference.x <= 0 || reference.y <= 0 || safeArea.width <= 0f || safeArea.height <= 0f)
            {
                return new ArcadeLayout(new Rect(safeArea.position, Vector2.zero), 0f, reference);
            }

            float scale = Mathf.Min(safeArea.width / reference.x, safeArea.height / reference.y);
            var size = new Vector2(reference.x * scale, reference.y * scale);
            float x = safeArea.x + (safeArea.width - size.x) * 0.5f;
            return new ArcadeLayout(new Rect(new Vector2(x, safeArea.y), size), scale, reference);
        }

        /// <summary>A rectangle of the reference grid (origin top left) in display pixels (origin bottom left).</summary>
        public Rect ToDisplay(RectInt reference)
        {
            float x = Cabinet.x + reference.x * Scale;
            float y = Cabinet.y + (_reference.y - reference.y - reference.height) * Scale;
            return new Rect(x, y, reference.width * Scale, reference.height * Scale);
        }
    }
}
