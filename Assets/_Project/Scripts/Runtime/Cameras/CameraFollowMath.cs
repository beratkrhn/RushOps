using UnityEngine;

namespace RushOps.Cameras
{
    /// <summary>
    /// Pure camera follow rules: keeping the view inside level bounds and snapping to the pixel grid.
    /// </summary>
    public static class CameraFollowMath
    {
        /// <summary>
        /// Clamps a camera center so that a view with the given half extents stays inside <paramref name="bounds"/>.
        /// If the level is smaller than the view on an axis, the camera is centered on that axis.
        /// </summary>
        public static Vector2 ClampToBounds(Vector2 center, Rect bounds, Vector2 halfExtents)
        {
            return new Vector2(
                ClampAxis(center.x, bounds.xMin, bounds.xMax, halfExtents.x),
                ClampAxis(center.y, bounds.yMin, bounds.yMax, halfExtents.y));
        }

        /// <summary>
        /// Rounds a position to the nearest art pixel (1 / <paramref name="pixelsPerUnit"/> units).
        /// Returns the position unchanged if <paramref name="pixelsPerUnit"/> is not positive.
        /// </summary>
        public static Vector2 SnapToPixelGrid(Vector2 position, int pixelsPerUnit)
        {
            if (pixelsPerUnit <= 0)
            {
                return position;
            }

            return new Vector2(
                Mathf.Round(position.x * pixelsPerUnit) / pixelsPerUnit,
                Mathf.Round(position.y * pixelsPerUnit) / pixelsPerUnit);
        }

        private static float ClampAxis(float value, float min, float max, float halfExtent)
        {
            float lower = min + halfExtent;
            float upper = max - halfExtent;
            if (lower > upper)
            {
                return (min + max) * 0.5f;
            }

            return Mathf.Clamp(value, lower, upper);
        }

        /// <summary>
        /// Camera offset <paramref name="elapsed"/> seconds into a shake: smooth noise on both axes, scaled by
        /// <paramref name="strength"/> and fading out quadratically until <paramref name="duration"/>; zero outside.
        /// </summary>
        public static Vector2 ShakeOffset(float elapsed, float duration, float strength, float frequency)
        {
            if (elapsed < 0f || elapsed >= duration || duration <= 0f)
            {
                return Vector2.zero;
            }

            float fade = 1f - elapsed / duration;
            fade *= fade;
            float t = elapsed * frequency;
            // Separate noise rows per axis, so the camera does not only shake diagonally.
            float x = Mathf.PerlinNoise(t, 0.37f) * 2f - 1f;
            float y = Mathf.PerlinNoise(0.71f, t) * 2f - 1f;
            return new Vector2(x, y) * (strength * fade);
        }
    }
}
