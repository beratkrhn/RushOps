using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Pure dash rules: which way the dash goes and how fast it moves.
    /// </summary>
    public static class PlayerDashMath
    {
        private const float MinSqrMagnitude = 1e-6f;

        /// <summary>
        /// Unit vector of the dash: the move direction, or <paramref name="facing"/> while not moving (F-STE-03).
        /// Returns zero only if both are (near) zero.
        /// </summary>
        public static Vector2 Direction(Vector2 move, Vector2 facing)
        {
            if (move.sqrMagnitude >= MinSqrMagnitude)
            {
                return move.normalized;
            }

            return facing.sqrMagnitude >= MinSqrMagnitude ? facing.normalized : Vector2.zero;
        }

        /// <summary>Speed that covers <paramref name="distance"/> in <paramref name="duration"/> seconds; 0 for a non-positive duration.</summary>
        public static float Speed(float distance, float duration)
        {
            return duration > 0f ? distance / duration : 0f;
        }
    }
}
