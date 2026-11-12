using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>Rules for an enemy crawling out of a breach (F-END-15): how far it is, how tall and how opaque it looks.</summary>
    public static class EmergeMath
    {
        /// <summary>Height of the enemy sprite at the start of the crawl-out, as a factor of its full height.</summary>
        public const float StartScale = 0.2f;

        /// <summary>Opacity at the start of the crawl-out.</summary>
        public const float StartAlpha = 0.35f;

        /// <summary>Progress from 0 to 1 after <paramref name="elapsed"/> of <paramref name="duration"/> seconds; 1 without duration.</summary>
        public static float Progress(float elapsed, float duration)
        {
            return duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
        }

        /// <summary>Height factor of the sprite at <paramref name="progress"/>: it rises out of the ground at its feet.</summary>
        public static float Scale(float progress)
        {
            return Mathf.Lerp(StartScale, 1f, Mathf.Clamp01(progress));
        }

        /// <summary>Opacity at <paramref name="progress"/>.</summary>
        public static float Alpha(float progress)
        {
            return Mathf.Lerp(StartAlpha, 1f, Mathf.Clamp01(progress));
        }
    }
}
