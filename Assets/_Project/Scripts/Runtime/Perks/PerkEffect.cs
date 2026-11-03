namespace RushOps.Perks
{
    /// <summary>What a perk does once collected (F-PRK-04); the strength comes from <see cref="PerkDefinition.Magnitude"/>.</summary>
    public enum PerkEffect
    {
        /// <summary>No effect yet.</summary>
        None = 0,

        /// <summary>Walking speed times the magnitude.</summary>
        SpeedBoost = 1,

        /// <summary>Restores the magnitude in hearts, at most to the maximum.</summary>
        Medkit = 2,

        /// <summary>Weapon damage times the magnitude (issue #25).</summary>
        DoubleDamage = 3,

        /// <summary>Fire rate times the magnitude (issue #25).</summary>
        RapidFire = 4,

        /// <summary>Invulnerability (issue #26).</summary>
        Shield = 5,

        /// <summary>Every hit kills, except bosses (issue #25).</summary>
        InstaKill = 6,

        /// <summary>Kills all visible enemies, except bosses (issue #27).</summary>
        Nuke = 7,
    }
}
