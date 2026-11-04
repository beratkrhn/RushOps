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

        /// <summary>Kills line, e.g. „Kills: 42“.</summary>
        public static string Kills(int kills)
        {
            return "Kills: " + (kills > 0 ? kills : 0).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Time line in minutes and seconds, e.g. „Zeit: 3:07“; seconds are rounded down.</summary>
        public static string Time(float seconds)
        {
            int total = seconds > 0f ? (int)seconds : 0;
            return $"Zeit: {total / 60}:{(total % 60).ToString("00", CultureInfo.InvariantCulture)}";
        }
    }
}
