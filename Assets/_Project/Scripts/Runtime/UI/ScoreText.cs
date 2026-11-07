namespace RushOps.UI
{
    /// <summary>Texts of the score display (F-HUD-04, issue #28).</summary>
    public static class ScoreText
    {
        /// <summary>The score, e.g. „1250“; negative values show as 0.</summary>
        public static string Score(int score)
        {
            return (score > 0 ? score : 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>The multiplier, e.g. „×3“.</summary>
        public static string Multiplier(int multiplier)
        {
            return "×" + (multiplier > 1 ? multiplier : 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>The gold of the level or run in the HUD, e.g. „12“; negative values show as 0.</summary>
        public static string Gold(int gold)
        {
            return (gold > 0 ? gold : 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Scale of the multiplier label <paramref name="elapsed"/> seconds after a rise: <paramref name="peak"/> at the
        /// rise, easing back to 1 over <paramref name="duration"/> seconds.
        /// </summary>
        public static float PulseScale(float elapsed, float duration, float peak)
        {
            if (elapsed < 0f || duration <= 0f || elapsed >= duration)
            {
                return 1f;
            }

            return 1f + (peak - 1f) * (1f - elapsed / duration);
        }
    }
}
