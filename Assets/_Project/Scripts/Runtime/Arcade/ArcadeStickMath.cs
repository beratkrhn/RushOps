using UnityEngine;

namespace RushOps.Arcade
{
    /// <summary>
    /// Which way a joystick of the cabinet tilts (F-ARC-04): the stick value is cut into <c>steps</c> directions,
    /// counted counter-clockwise from east, 1-based; 0 is the stick at rest. The sprites show 8 directions.
    /// </summary>
    public static class ArcadeStickMath
    {
        /// <summary>Directions the stick sprites show (E, NE, N, NW, W, SW, S, SE).</summary>
        public const int SpriteDirections = 8;

        /// <summary>
        /// Tilt step of <paramref name="value"/>: 0 below <paramref name="threshold"/>, otherwise 1 to
        /// <paramref name="steps"/>, 1 being east and counting counter-clockwise.
        /// </summary>
        public static int Step(Vector2 value, float threshold, int steps)
        {
            if (steps <= 0 || value.sqrMagnitude <= threshold * threshold || value == Vector2.zero)
            {
                return 0;
            }

            float angle = Mathf.Atan2(value.y, value.x);
            if (angle < 0f)
            {
                angle += 2f * Mathf.PI;
            }

            int index = Mathf.RoundToInt(angle / (2f * Mathf.PI / steps)) % steps;
            return index + 1;
        }

        /// <summary>The step pointing the other way, for the spring back on release; 0 stays 0.</summary>
        public static int Opposite(int step, int steps)
        {
            if (step <= 0 || steps <= 0)
            {
                return 0;
            }

            return (step - 1 + steps / 2) % steps + 1;
        }

        /// <summary>Index into the stick sprites (0 at rest, 1 to 8 for E to SE) for a step of <paramref name="steps"/>.</summary>
        public static int SpriteIndex(int step, int steps)
        {
            if (step <= 0 || steps <= 0)
            {
                return 0;
            }

            int direction = Mathf.RoundToInt((step - 1) * (float)SpriteDirections / steps) % SpriteDirections;
            return direction + 1;
        }
    }
}
