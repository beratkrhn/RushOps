namespace RushOps.Save
{
    /// <summary>What <see cref="EndlessRecords.Record"/> made of one finished run (issue #211).</summary>
    public readonly struct EndlessRecord
    {
        /// <summary>Creates the record of a run.</summary>
        public EndlessRecord(int score, int round, int rank, bool isNewBest)
        {
            Score = score;
            Round = round;
            Rank = rank;
            IsNewBest = isNewBest;
        }

        /// <summary>Score of the run.</summary>
        public int Score { get; }

        /// <summary>Round the run ended in.</summary>
        public int Round { get; }

        /// <summary>Place in the top 10, from 1; 0 if the run did not make it.</summary>
        public int Rank { get; }

        /// <summary>True if the run beat the best score so far (or set the first one).</summary>
        public bool IsNewBest { get; }
    }
}
