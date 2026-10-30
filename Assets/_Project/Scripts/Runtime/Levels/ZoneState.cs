namespace RushOps.Levels
{
    /// <summary>Progress of a <see cref="Zone"/> (F-LVL-02).</summary>
    public enum ZoneState
    {
        /// <summary>The player has not entered the zone yet.</summary>
        Inactive,

        /// <summary>The player entered the zone; its waves run.</summary>
        Active,

        /// <summary>The zone's exit gate opened.</summary>
        Completed,
    }
}
