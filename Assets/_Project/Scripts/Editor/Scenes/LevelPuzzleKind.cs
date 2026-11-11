namespace RushOps.Editor.Scenes
{
    /// <summary>Puzzle types a <see cref="LevelBlueprint"/> can place (Lastenheft 2.6).</summary>
    internal enum LevelPuzzleKind
    {
        HoldZone,
        TimedSwitches,
        Transport,

        /// <summary>Pressure plate only a brute pushes down (issue #289).</summary>
        PressurePlate,

        /// <summary>Cracked wall in front of a gate, only a boomer breaks it (issue #290).</summary>
        CrackedWall,
    }
}
