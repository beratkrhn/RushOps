using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Timing of an enemy's death (issue #100): the death animation plays to its end, then the body fades out.
    /// Without a death animation the body shrinks while it fades.
    /// </summary>
    public static class EnemyDeathMath
    {
        /// <summary>Seconds from the killing hit until the enemy returns to its pool.</summary>
        public static float TotalDuration(float animationDuration, float fadeDuration)
        {
            return Mathf.Max(0f, animationDuration) + Mathf.Max(0f, fadeDuration);
        }

        /// <summary>
        /// Fade progress 0..1 after <paramref name="dyingTime"/> seconds: 0 while the death animation plays,
        /// then rising to 1 over <paramref name="fadeDuration"/> seconds.
        /// </summary>
        public static float FadeProgress(float dyingTime, float animationDuration, float fadeDuration)
        {
            float sinceAnimation = dyingTime - Mathf.Max(0f, animationDuration);
            if (sinceAnimation < 0f)
            {
                return 0f;
            }

            return fadeDuration > 0f ? Mathf.Clamp01(sinceAnimation / fadeDuration) : 1f;
        }
    }
}
