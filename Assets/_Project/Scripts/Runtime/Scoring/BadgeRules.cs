namespace RushOps.Scoring
{
    /// <summary>
    /// Rules of the level badges (issue #287, F-SCO-04, CH1, N23): each badge counts on its own, can be earned in a
    /// different try and stays once earned.
    /// </summary>
    public static class BadgeRules
    {
        /// <summary>Number of badges of one level.</summary>
        public const int PerLevel = 3;

        /// <summary>The badges in display order: Unversehrt, Blitz, Kombo.</summary>
        public static readonly BadgeKind[] Order = { BadgeKind.Unharmed, BadgeKind.Blitz, BadgeKind.Combo };

        /// <summary>Position of <paramref name="badge"/> in <see cref="Order"/>; -1 for none or several.</summary>
        public static int IndexOf(BadgeKind badge)
        {
            for (int i = 0; i < Order.Length; i++)
            {
                if (Order[i] == badge)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Scale of a newly earned badge <paramref name="seconds"/> into its pulse: from 1 up to
        /// <paramref name="maxScale"/> and back once per <paramref name="period"/>.
        /// </summary>
        public static float PulseScale(float seconds, float period, float maxScale)
        {
            if (period <= 0f || maxScale <= 1f)
            {
                return 1f;
            }

            float phase = seconds / period;
            phase -= (float)System.Math.Floor(phase);
            float wave = 0.5f - 0.5f * (float)System.Math.Cos(phase * 2.0 * System.Math.PI);
            return 1f + (maxScale - 1f) * wave;
        }

        /// <summary>
        /// Badges of one try: none if the level was not finished; „Unversehrt“ without a lost heart, „Blitz“ within
        /// <paramref name="targetSeconds"/> (exactly on it counts; 0 or less means the level has no target time),
        /// „Kombo“ once <paramref name="bestMultiplier"/> reached <paramref name="comboTarget"/>.
        /// </summary>
        public static BadgeKind Earned(bool finished, int heartsLost, float seconds, float targetSeconds,
            int bestMultiplier, int comboTarget)
        {
            if (!finished)
            {
                return BadgeKind.None;
            }

            BadgeKind earned = BadgeKind.None;
            if (heartsLost <= 0)
            {
                earned |= BadgeKind.Unharmed;
            }

            if (targetSeconds > 0f && seconds <= targetSeconds)
            {
                earned |= BadgeKind.Blitz;
            }

            if (comboTarget > 0 && bestMultiplier >= comboTarget)
            {
                earned |= BadgeKind.Combo;
            }

            return earned;
        }

        /// <summary>Saved badges together with newly earned ones; nothing is ever taken away.</summary>
        public static BadgeKind Merge(BadgeKind saved, BadgeKind earned)
        {
            return (saved | earned) & BadgeKind.All;
        }

        /// <summary>Badges in <paramref name="earned"/> that are not yet in <paramref name="saved"/>.</summary>
        public static BadgeKind NewOnes(BadgeKind saved, BadgeKind earned)
        {
            return earned & ~saved & BadgeKind.All;
        }

        /// <summary>Number of badges set in <paramref name="badges"/>, 0 to 3.</summary>
        public static int Count(BadgeKind badges)
        {
            int count = 0;
            int bits = (int)(badges & BadgeKind.All);
            while (bits != 0)
            {
                count += bits & 1;
                bits >>= 1;
            }

            return count;
        }

        /// <summary>True if <paramref name="badges"/> contains <paramref name="badge"/>.</summary>
        public static bool Has(BadgeKind badges, BadgeKind badge)
        {
            return badge != BadgeKind.None && (badges & badge) == badge;
        }
    }
}
