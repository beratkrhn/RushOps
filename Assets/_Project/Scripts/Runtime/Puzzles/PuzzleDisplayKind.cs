namespace RushOps.Puzzles
{
    /// <summary>How the mission tracker shows a puzzle (F-HUD-03, issue #30).</summary>
    public enum PuzzleDisplayKind
    {
        /// <summary>Only a text.</summary>
        Text,

        /// <summary>A text and a progress bar, e.g. while holding a zone.</summary>
        Progress,

        /// <summary>A text with the seconds left, e.g. for timed switches.</summary>
        Countdown,
    }
}
