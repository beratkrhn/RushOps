namespace RushOps.Levels
{
    /// <summary>How a level shows on the island map (issue #127, F-UI-06).</summary>
    public enum LevelNodeState
    {
        /// <summary>The level is not built yet („bald“).</summary>
        Soon,

        /// <summary>Built, but the level before it or its island is not finished yet.</summary>
        Locked,

        /// <summary>Playable and not finished yet.</summary>
        Open,

        /// <summary>Finished at least once; shows its stars.</summary>
        Done,
    }
}
