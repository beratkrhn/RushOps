using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>Curves of the hit feedback (issue #12).</summary>
    public static class FeelMath
    {
        /// <summary>Flash amount (0..1) <paramref name="elapsed"/> seconds after a hit: full white, then off after <paramref name="duration"/>.</summary>
        public static float FlashAmount(float elapsed, float duration)
        {
            return elapsed >= 0f && elapsed < duration ? 1f : 0f;
        }

        /// <summary>Knockback factor (1..0) <paramref name="elapsed"/> seconds into a push lasting <paramref name="duration"/>, easing out linearly.</summary>
        public static float KnockbackFactor(float elapsed, float duration)
        {
            if (elapsed < 0f || elapsed >= duration || duration <= 0f)
            {
                return 0f;
            }

            return 1f - elapsed / duration;
        }

        /// <summary>
        /// Start velocity of a knockback along <paramref name="direction"/>: <paramref name="speed"/> times
        /// <paramref name="scale"/> (clamped to 0..1), e.g. heavy enemies take only a fraction of it.
        /// </summary>
        public static Vector2 KnockbackVelocity(Vector2 direction, float speed, float scale)
        {
            return direction.normalized * (speed * Mathf.Clamp01(scale));
        }

        /// <summary>
        /// Opacity of a screen flash <paramref name="elapsed"/> seconds after it started at <paramref name="peak"/>,
        /// fading out linearly over <paramref name="duration"/> seconds; 0 before the start and afterwards.
        /// </summary>
        public static float FlashFade(float elapsed, float duration, float peak)
        {
            if (elapsed < 0f || duration <= 0f || elapsed >= duration)
            {
                return 0f;
            }

            return peak * (1f - elapsed / duration);
        }

        /// <summary>Pitch for a sound from <paramref name="random01"/> in [0, 1]: 1 plus or minus up to <paramref name="variation"/>.</summary>
        public static float Pitch(float random01, float variation)
        {
            return 1f + (random01 * 2f - 1f) * variation;
        }
    }
}
