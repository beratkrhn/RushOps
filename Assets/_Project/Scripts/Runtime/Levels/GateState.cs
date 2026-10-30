namespace RushOps.Levels
{
    /// <summary>State of a <see cref="Gate"/> (F-LVL-02).</summary>
    public enum GateState
    {
        /// <summary>The gate blocks the passage and waits for its condition.</summary>
        Closed,

        /// <summary>The condition was met; the gate plays its opening and still blocks.</summary>
        Opening,

        /// <summary>The passage is free.</summary>
        Open,
    }
}
