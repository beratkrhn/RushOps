using UnityEngine;

namespace RushOps.UI
{
    /// <summary>
    /// Recognises a vertical swipe on the island map (issue #127, design question M10) and a horizontal one in the
    /// skin selection (issue #130).
    /// </summary>
    public static class SwipeMath
    {
        /// <summary>
        /// +1 for a swipe up (finger moves up), -1 for a swipe down, 0 if the move is shorter than
        /// <paramref name="minDistance"/> or more sideways than vertical.
        /// </summary>
        public static int Vertical(Vector2 start, Vector2 end, float minDistance)
        {
            Vector2 delta = end - start;
            if (Mathf.Abs(delta.y) < minDistance || Mathf.Abs(delta.y) < Mathf.Abs(delta.x))
            {
                return 0;
            }

            return delta.y > 0f ? 1 : -1;
        }

        /// <summary>
        /// +1 for a swipe to the right (finger moves right), -1 for a swipe to the left, 0 if the move is shorter than
        /// <paramref name="minDistance"/> or more vertical than sideways.
        /// </summary>
        public static int Horizontal(Vector2 start, Vector2 end, float minDistance)
        {
            Vector2 delta = end - start;
            if (Mathf.Abs(delta.x) < minDistance || Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
            {
                return 0;
            }

            return delta.x > 0f ? 1 : -1;
        }

        /// <summary>Moves <paramref name="current"/> towards <paramref name="target"/>, independent of the frame rate.</summary>
        public static float Approach(float current, float target, float sharpness, float deltaTime)
        {
            if (sharpness <= 0f)
            {
                return target;
            }

            float next = Mathf.Lerp(current, target, 1f - Mathf.Exp(-sharpness * deltaTime));
            return Mathf.Abs(next - target) < 0.5f ? target : next;
        }

        /// <summary>
        /// 0 for an item in the middle of a carousel, rising to 1 one <paramref name="spacing"/> away and beyond;
        /// blends its size and opacity towards the neighbours' (issue #130).
        /// </summary>
        public static float SideBlend(float itemPosition, float offset, float spacing)
        {
            return spacing > 0f ? Mathf.Clamp01(Mathf.Abs(itemPosition - offset) / spacing) : 1f;
        }
    }
}
