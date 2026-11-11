namespace RushOps.Audio
{
    /// <summary>The music tracks of the game (issue #299, N31).</summary>
    public enum MusicTrack
    {
        /// <summary>No music, e.g. on the boot screen.</summary>
        None,

        /// <summary>Main menu, island map and skin selection.</summary>
        Menu,

        /// <summary>Campaign levels, boss levels and the endless mode.</summary>
        Game,
    }
}
