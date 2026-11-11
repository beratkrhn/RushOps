namespace RushOps.Puzzles
{
    /// <summary>Puzzles only a certain zombie can solve (issue #287, F-RAE-09 to F-RAE-13, Z1).</summary>
    public enum ZombiePuzzleKind
    {
        /// <summary>Pressure plate only a brute pushes down (F-RAE-09).</summary>
        PressurePlate,

        /// <summary>Cracked wall only a boomer's explosion breaks (F-RAE-11).</summary>
        CrackedWall,
    }
}
