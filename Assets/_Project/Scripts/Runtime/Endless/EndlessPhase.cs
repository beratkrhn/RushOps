namespace RushOps.Endless
{
    /// <summary>Phase of an <see cref="EndlessRun"/>.</summary>
    public enum EndlessPhase
    {
        /// <summary>Not started yet.</summary>
        Idle,

        /// <summary>Break before a round with the large „Runde X“ (F-END-03).</summary>
        Pause,

        /// <summary>The round's enemies come and fight.</summary>
        Running,

        /// <summary>The player has no hearts left; the run is over (F-END-08).</summary>
        Ended,
    }
}
