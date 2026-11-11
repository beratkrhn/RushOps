using System;

namespace RushOps.Save
{
    /// <summary>The best daily run of one day (issue #276, F-END-14): date, score, round and how often it was tried.</summary>
    [Serializable]
    public class DailyScore
    {
        /// <summary>Day of the daily run as <c>yyyy-MM-dd</c>.</summary>
        public string Date = "";

        /// <summary>Best score of the day.</summary>
        public int Score;

        /// <summary>Round the best run of the day ended in.</summary>
        public int Round;

        /// <summary>Daily runs played to the end on this day.</summary>
        public int Tries;

        /// <summary>Creates an empty entry, for <c>JsonUtility</c>.</summary>
        public DailyScore()
        {
        }

        /// <summary>Creates an entry.</summary>
        public DailyScore(string date, int score, int round, int tries)
        {
            Date = date ?? "";
            Score = score;
            Round = round;
            Tries = tries;
        }
    }
}
