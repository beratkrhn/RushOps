using System.Globalization;

namespace RushOps.UI
{
    /// <summary>Texts of the result screens (issue #35).</summary>
    public static class ResultText
    {
        /// <summary>Title when the level is done.</summary>
        public const string Done = "Level geschafft!";

        /// <summary>Title when the player died.</summary>
        public const string Failed = "Gescheitert";

        /// <summary>Score line, e.g. „Score: 1250“.</summary>
        public static string Score(int score)
        {
            return "Score: " + ScoreText.Score(score);
        }

        /// <summary>Gold line of the result screens (F-GLD-04), e.g. „+12 Gold“.</summary>
        public static string Gold(int gold)
        {
            return "+" + (gold > 0 ? gold : 0).ToString(CultureInfo.InvariantCulture) + " Gold";
        }

        /// <summary>Kills line, e.g. „Kills: 42“.</summary>
        public static string Kills(int kills)
        {
            return "Kills: " + (kills > 0 ? kills : 0).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Hint for the next star, e.g. „2 Sterne ab 1500 Punkten“; empty with all three stars.
        /// </summary>
        public static string NextStar(int stars, int twoStarScore, int threeStarScore)
        {
            if (stars >= 3)
            {
                return string.Empty;
            }

            return stars <= 1
                ? $"2 Sterne ab {ScoreText.Score(twoStarScore)} Punkten"
                : $"3 Sterne ab {ScoreText.Score(threeStarScore)} Punkten";
        }

        /// <summary>
        /// Line for skins unlocked by the level (issue #128), e.g. „Neuer Skin: Surfer“ or „Neue Skins: Surfer +1“.
        /// </summary>
        public static string NewSkin(string name, int more)
        {
            return more > 0
                ? $"Neue Skins: {name} +{more.ToString(CultureInfo.InvariantCulture)}"
                : $"Neuer Skin: {name}";
        }

        /// <summary>Time line in minutes and seconds, e.g. „Zeit: 3:07“; seconds are rounded down.</summary>
        public static string Time(float seconds)
        {
            int total = seconds > 0f ? (int)seconds : 0;
            return $"Zeit: {total / 60}:{(total % 60).ToString("00", CultureInfo.InvariantCulture)}";
        }
    }
}
