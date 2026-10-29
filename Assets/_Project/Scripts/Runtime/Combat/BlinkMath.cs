namespace RushOps.Combat
{
    /// <summary>On/off pattern for blinking sprites, e.g. during invulnerability (F-SPF-02).</summary>
    public static class BlinkMath
    {
        /// <summary>
        /// True if the sprite shows at <paramref name="elapsed"/> seconds into the blink: visible for one
        /// <paramref name="interval"/>, hidden for the next, starting hidden. A non-positive interval never hides.
        /// </summary>
        public static bool IsVisible(float elapsed, float interval)
        {
            if (interval <= 0f || elapsed < 0f)
            {
                return true;
            }

            return (int)(elapsed / interval) % 2 == 1;
        }
    }
}
