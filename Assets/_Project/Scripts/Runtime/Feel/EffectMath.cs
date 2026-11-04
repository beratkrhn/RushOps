namespace RushOps.Feel
{
    /// <summary>Timing of one-shot frame effects and their accent lights (issue #107).</summary>
    public static class EffectMath
    {
        /// <summary>
        /// Frame to show <paramref name="elapsed"/> seconds into an effect of <paramref name="count"/> frames of
        /// <paramref name="frameDuration"/> seconds each, or -1 once it is over.
        /// </summary>
        public static int FrameAt(float elapsed, float frameDuration, int count)
        {
            if (count <= 0 || frameDuration <= 0f)
            {
                return -1;
            }

            int frame = (int)(elapsed / frameDuration);
            return frame < count ? (frame < 0 ? 0 : frame) : -1;
        }

        /// <summary>
        /// Light intensity <paramref name="elapsed"/> seconds into an effect lasting <paramref name="duration"/>:
        /// <paramref name="peak"/> at the start, fading quadratically to 0 at the end.
        /// </summary>
        public static float LightFade(float elapsed, float duration, float peak)
        {
            if (duration <= 0f || elapsed >= duration)
            {
                return 0f;
            }

            float left = 1f - (elapsed < 0f ? 0f : elapsed) / duration;
            return peak * left * left;
        }
    }
}
