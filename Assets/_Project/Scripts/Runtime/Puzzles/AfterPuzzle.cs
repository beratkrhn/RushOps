namespace RushOps.Puzzles
{
    /// <summary>What a zone's waves do once its puzzle with own waves is solved (issue #34).</summary>
    public enum AfterPuzzle
    {
        /// <summary>The zone runs its normal waves again, from the first wave.</summary>
        ResumeZoneWaves,

        /// <summary>The zone sends no more enemies; living ones stay.</summary>
        EndWaves,
    }
}
