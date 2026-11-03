using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>Floating and glowing of a dropped perk (F-PRK-01).</summary>
    public static class PickupMath
    {
        /// <summary>Height above the rest position in units: a sine of <paramref name="height"/> amplitude and <paramref name="period"/> seconds, never below 0.</summary>
        public static float Bob(float elapsed, float height, float period)
        {
            if (period <= 0f || height <= 0f)
            {
                return 0f;
            }

            return height * 0.5f * (1f - Mathf.Cos(elapsed * 2f * Mathf.PI / period));
        }

        /// <summary>Glow alpha pulsing between <paramref name="min"/> and <paramref name="max"/> every <paramref name="period"/> seconds, starting bright.</summary>
        public static float Pulse(float elapsed, float min, float max, float period)
        {
            if (period <= 0f)
            {
                return max;
            }

            float t = 0.5f * (1f + Mathf.Cos(elapsed * 2f * Mathf.PI / period));
            return Mathf.Lerp(min, max, t);
        }
    }
}
