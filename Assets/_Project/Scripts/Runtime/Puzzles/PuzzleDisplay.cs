namespace RushOps.Puzzles
{
    /// <summary>What the mission tracker shows for a puzzle (F-HUD-03, issue #30): a text, optionally with progress or a countdown.</summary>
    public readonly struct PuzzleDisplay
    {
        private PuzzleDisplay(PuzzleDisplayKind kind, string text, float value)
        {
            Kind = kind;
            Text = text ?? string.Empty;
            Value = value;
        }

        /// <summary>Text only, progress bar or countdown.</summary>
        public PuzzleDisplayKind Kind { get; }

        /// <summary>Short German text, e.g. „Zone halten“.</summary>
        public string Text { get; }

        /// <summary>Progress 0..1 or seconds left, depending on <see cref="Kind"/>; 0 for text only.</summary>
        public float Value { get; }

        /// <summary>Shows only <paramref name="text"/>.</summary>
        public static PuzzleDisplay TextOnly(string text)
        {
            return new PuzzleDisplay(PuzzleDisplayKind.Text, text, 0f);
        }

        /// <summary>Shows <paramref name="text"/> and a bar filled to <paramref name="progress"/>, clamped to 0..1.</summary>
        public static PuzzleDisplay WithProgress(string text, float progress)
        {
            float clamped = progress < 0f ? 0f : progress > 1f ? 1f : progress;
            return new PuzzleDisplay(PuzzleDisplayKind.Progress, text, clamped);
        }

        /// <summary>Shows <paramref name="text"/> with <paramref name="secondsLeft"/>, at least 0.</summary>
        public static PuzzleDisplay WithCountdown(string text, float secondsLeft)
        {
            return new PuzzleDisplay(PuzzleDisplayKind.Countdown, text, secondsLeft > 0f ? secondsLeft : 0f);
        }
    }
}
