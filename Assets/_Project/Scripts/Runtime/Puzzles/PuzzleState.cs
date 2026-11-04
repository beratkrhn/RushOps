namespace RushOps.Puzzles
{
    /// <summary>Progress of a <see cref="Puzzle"/> (F-LVL-04, issue #30).</summary>
    public enum PuzzleState
    {
        /// <summary>The puzzle's zone is not active yet; nothing can be done.</summary>
        Inactive,

        /// <summary>The puzzle can be worked on.</summary>
        Active,

        /// <summary>The puzzle is solved for good; its gate may open.</summary>
        Solved,
    }
}
