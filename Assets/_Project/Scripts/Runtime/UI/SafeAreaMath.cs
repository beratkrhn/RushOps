using UnityEngine;

namespace RushOps.UI
{
    /// <summary>Converts the device safe area (F-PLT-05) into anchors of a full-screen <see cref="RectTransform"/>.</summary>
    public static class SafeAreaMath
    {
        /// <summary>
        /// Normalized anchors for <paramref name="safeArea"/> in pixels on a screen of <paramref name="screenSize"/> pixels,
        /// clamped to the screen. Without a valid screen size the anchors span the whole screen.
        /// </summary>
        public static void Anchors(Rect safeArea, Vector2 screenSize, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f)
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return;
            }

            anchorMin = Clamp01(new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y));
            anchorMax = Clamp01(new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y));
        }

        private static Vector2 Clamp01(Vector2 value)
        {
            return new Vector2(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y));
        }
    }
}
