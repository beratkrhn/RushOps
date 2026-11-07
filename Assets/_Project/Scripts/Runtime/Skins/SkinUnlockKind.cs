namespace RushOps.Skins
{
    /// <summary>How a skin is unlocked (F-SKN-02, F-SKN-03).</summary>
    public enum SkinUnlockKind
    {
        /// <summary>Always available, the skin of a new game.</summary>
        Default,

        /// <summary>Unlocked by finishing a certain level (F-SKN-02).</summary>
        LevelCompleted,

        /// <summary>Unlocked once a statistic across all games reaches a threshold (F-SKN-03).</summary>
        Milestone,

        /// <summary>Unlocked once an endless run reaches a certain round (issue #211, F-END-09).</summary>
        EndlessRound,

        /// <summary>Bought with gold; the price is the threshold (issue #237, F-GLD-06).</summary>
        Purchase,
    }
}
