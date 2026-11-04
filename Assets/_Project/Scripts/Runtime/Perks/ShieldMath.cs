using RushOps.Combat;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>Rules of the shield perk (issue #26): when the barrier shows and where it pushes enemies.</summary>
    public static class ShieldMath
    {
        /// <summary>
        /// True if the barrier shows with <paramref name="remaining"/> seconds of shield left: always before the last
        /// <paramref name="warningTime"/> seconds, then it flickers every <paramref name="blinkInterval"/>, starting
        /// hidden. No shield left shows nothing.
        /// </summary>
        public static bool IsBarrierVisible(float remaining, float warningTime, float blinkInterval)
        {
            if (remaining <= 0f)
            {
                return false;
            }

            if (remaining > warningTime)
            {
                return true;
            }

            return BlinkMath.IsVisible(warningTime - remaining, blinkInterval);
        }

        /// <summary>
        /// Unit direction from the player's <paramref name="center"/> to the touching <paramref name="other"/>; straight
        /// up if both lie on the same point.
        /// </summary>
        public static Vector2 PushDirection(Vector2 center, Vector2 other)
        {
            Vector2 offset = other - center;
            return offset.sqrMagnitude > 1e-8f ? offset.normalized : Vector2.up;
        }
    }
}
