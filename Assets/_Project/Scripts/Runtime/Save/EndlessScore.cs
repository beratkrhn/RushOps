using System;

namespace RushOps.Save
{
    /// <summary>One entry of the endless top 10 (issue #211, F-END-08): score, round reached and the day of the run.</summary>
    [Serializable]
    public class EndlessScore
    {
        /// <summary>Score of the run.</summary>
        public int Score;

        /// <summary>Round the run ended in.</summary>
        public int Round;

        /// <summary>Day of the run as <c>yyyy-MM-dd</c>.</summary>
        public string Date = "";

        /// <summary>Creates an empty entry, for <c>JsonUtility</c>.</summary>
        public EndlessScore()
        {
        }

        /// <summary>Creates an entry.</summary>
        public EndlessScore(int score, int round, string date)
        {
            Score = score;
            Round = round;
            Date = date ?? "";
        }
    }
}
