namespace RushOps.UI
{
    /// <summary>Texts of the mission tracker (F-HUD-03), including puzzles (issue #30).</summary>
    public static class MissionText
    {
        /// <summary>Shown for a moment once the tracked gate opens.</summary>
        public const string GateOpen = "Tor offen!";

        /// <summary>Shown for a moment once a puzzle without a gate is solved.</summary>
        public const string PuzzleSolved = "Geschafft!";

        /// <summary>Shown in the exit's zone while the exit is locked by the last waves.</summary>
        public const string ClearZone = "Alle Gegner besiegen";

        /// <summary>Shown in the exit's zone once the exit is open.</summary>
        public const string ToExit = "Zum Ausgang!";

        /// <summary>Progress of a closed kill gate, e.g. „Tor gesperrt: 4/10 Kills“.</summary>
        public static string KillGate(int kills, int required)
        {
            return $"Tor gesperrt: {kills}/{required} Kills";
        }

        /// <summary>
        /// Text of a puzzle: its own text, for a countdown followed by the whole seconds left rounded up, e.g.
        /// „Schalter: 7 s“.
        /// </summary>
        public static string Puzzle(RushOps.Puzzles.PuzzleDisplay display)
        {
            return display.Kind == RushOps.Puzzles.PuzzleDisplayKind.Countdown
                ? $"{display.Text}: {CountdownSeconds(display.Value)} s"
                : display.Text;
        }

        /// <summary>Whole seconds shown for <paramref name="secondsLeft"/>: rounded up, so 0 shows only when the time is up.</summary>
        public static int CountdownSeconds(float secondsLeft)
        {
            return secondsLeft > 0f ? (int)System.Math.Ceiling(secondsLeft - 1e-4f) : 0;
        }
    }
}
