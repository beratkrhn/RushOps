namespace RushOps.Scoring
{
    /// <summary>Expected kills of one kind of enemy in a level, with what each one is worth (issue #301).</summary>
    public readonly struct KillTally
    {
        public KillTally(int points, int gold, float count)
        {
            Points = points;
            Gold = gold;
            Count = count;
        }

        /// <summary>Points of one kill.</summary>
        public int Points { get; }

        /// <summary>Coins one kill drops.</summary>
        public int Gold { get; }

        /// <summary>How many of them the level sends.</summary>
        public float Count { get; }
    }
}
