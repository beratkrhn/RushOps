using UnityEngine;

namespace RushOps.UI
{
    /// <summary>Recognises a vertical swipe on the island map (issue #127, design question M10).</summary>
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
    }
}
