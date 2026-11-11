namespace RushOps.Save
{
    /// <summary>What <see cref="DailyRecords.Record"/> made of one finished daily run (issue #276).</summary>
    public readonly struct DailyRecord
    {
        /// <summary>Creates the record of a daily run.</summary>
        public DailyRecord(string date, int score, int round, int bestScore, int bestRound, int tries, bool isNewBest)
        {
            Date = date;
            Score = score;
            Round = round;
            BestScore = bestScore;
            BestRound = bestRound;
            Tries = tries;
            IsNewBest = isNewBest;
        }

        /// <summary>Day of the run as <c>yyyy-MM-dd</c>.</summary>
        public string Date { get; }

        /// <summary>Score of this run.</summary>
        public int Score { get; }

        /// <summary>Round this run ended in.</summary>
        public int Round { get; }

        /// <summary>Best score of the day after this run.</summary>
        public int BestScore { get; }

        /// <summary>Round of the best run of the day after this run.</summary>
        public int BestRound { get; }

        /// <summary>Daily runs of the day, this one included.</summary>
        public int Tries { get; }

        /// <summary>True if this run is the new best of its day (also the first run of a day).</summary>
        public bool IsNewBest { get; }
    }
}
