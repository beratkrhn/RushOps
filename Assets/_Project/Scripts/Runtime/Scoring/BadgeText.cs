using System.Globalization;

namespace RushOps.Scoring
{
    /// <summary>Texts of the level badges (issue #288, F-SCO-05, N25).</summary>
    public static class BadgeText
    {
        /// <summary>German name of <paramref name="badge"/>.</summary>
        public static string Name(BadgeKind badge)
        {
            return badge switch
            {
                BadgeKind.Unharmed => "Unversehrt",
                BadgeKind.Blitz => "Blitz",
                BadgeKind.Combo => "Kombo",
                _ => string.Empty,
            };
        }

        /// <summary>
        /// The condition of <paramref name="badge"/> as one line, e.g. „Blitz: unter 4:30“; a level without a target time
        /// says so.
        /// </summary>
        public static string Condition(BadgeKind badge, float targetSeconds, int comboTarget)
        {
            return badge switch
            {
                BadgeKind.Unharmed => "Unversehrt: kein Herz verlieren",
                BadgeKind.Blitz => targetSeconds > 0f ? "Blitz: unter " + Clock(targetSeconds) : "Blitz: keine Zielzeit",
                BadgeKind.Combo => "Kombo: x" + comboTarget.ToString(CultureInfo.InvariantCulture) + " erreichen",
                _ => string.Empty,
            };
        }

        /// <summary>Seconds as „m:ss“, rounded down.</summary>
        public static string Clock(float seconds)
        {
            int total = seconds > 0f ? (int)seconds : 0;
            return (total / 60).ToString(CultureInfo.InvariantCulture) + ":" +
                   (total % 60).ToString("00", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Whether <paramref name="badge"/> can still be earned in the running try: „Unversehrt“ not after a lost heart,
        /// „Blitz“ not after the target time; „Kombo“ always.
        /// </summary>
        public static bool StillPossible(BadgeKind badge, int heartsLost, float seconds, float targetSeconds)
        {
            return badge switch
            {
                BadgeKind.Unharmed => heartsLost <= 0,
                BadgeKind.Blitz => targetSeconds > 0f && seconds <= targetSeconds,
                _ => true,
            };
        }
    }
}
