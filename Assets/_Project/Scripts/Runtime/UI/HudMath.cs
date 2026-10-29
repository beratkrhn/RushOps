namespace RushOps.UI
{
    /// <summary>Display rules of the HUD (F-HUD-01).</summary>
    public static class HudMath
    {
        /// <summary>True if heart <paramref name="index"/> (from 0, left to right) shows as full with <paramref name="current"/> hearts.</summary>
        public static bool IsHeartFull(int index, int current)
        {
            return index < current;
        }

        /// <summary>
        /// True if heart <paramref name="index"/> shows as full while the hearts blink after a hit from
        /// <paramref name="before"/> to <paramref name="current"/> hearts: the lost ones show full in the red phases.
        /// </summary>
        public static bool IsHeartFullWhileFlashing(int index, int current, int before, bool redPhase)
        {
            return index < current || (redPhase && index < before);
        }

        /// <summary>
        /// True if the hearts show the red tint <paramref name="sinceHit"/> seconds after a lost heart:
        /// red first, then alternating every <paramref name="interval"/> until <paramref name="duration"/> ends.
        /// Negative times mean no hit yet.
        /// </summary>
        public static bool IsFlashRed(float sinceHit, float duration, float interval)
        {
            if (sinceHit < 0f || sinceHit >= duration)
            {
                return false;
            }

            return interval <= 0f || (int)(sinceHit / interval) % 2 == 0;
        }
    }
}
