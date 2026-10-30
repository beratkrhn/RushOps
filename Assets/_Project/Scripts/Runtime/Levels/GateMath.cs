using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>Opening animation of a <see cref="Gate"/>: the bars shake briefly, then sink into the floor.</summary>
    public static class GateMath
    {
        // Shake frequency in radians per second.
        private const float ShakeSpeed = 90f;

        /// <summary>
        /// How far the bars have sunk (0 = standing, 1 = gone) at <paramref name="progress"/> (0..1) of an opening that
        /// lasts <paramref name="openDuration"/> seconds and starts with <paramref name="shakeDuration"/> seconds of shaking.
        /// </summary>
        public static float Sink(float progress, float openDuration, float shakeDuration)
        {
            float seconds = Mathf.Clamp01(progress) * openDuration;
            float sinkDuration = openDuration - shakeDuration;
            if (sinkDuration <= 0f)
            {
                return progress >= 1f ? 1f : 0f;
            }

            float t = Mathf.Clamp01((seconds - shakeDuration) / sinkDuration);
            // Ease in: the bars start slowly and drop faster, like a heavy portcullis.
            return t * t;
        }

        /// <summary>Sideways offset of the bars in units; zero once the shaking is over.</summary>
        public static float Shake(float progress, float openDuration, float shakeDuration, float amplitude)
        {
            float seconds = Mathf.Clamp01(progress) * openDuration;
            if (progress <= 0f || seconds >= shakeDuration)
            {
                return 0f;
            }

            return Mathf.Sin(seconds * ShakeSpeed) >= 0f ? amplitude : -amplitude;
        }
    }
}
