namespace RushOps.Perks
{
    /// <summary>How a perk acts once collected (F-PRK-02).</summary>
    public enum PerkKind
    {
        /// <summary>Acts once at pickup, e.g. medkit or nuke.</summary>
        Instant = 0,

        /// <summary>Acts for <see cref="PerkDefinition.Duration"/> seconds, e.g. speed boost.</summary>
        Timed = 1,
    }
}
