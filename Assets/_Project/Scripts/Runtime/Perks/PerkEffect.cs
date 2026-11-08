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

        /// <summary>A combat drone that fights along (issue #244).</summary>
        Companion = 8,

        /// <summary>A scarecrow that lures enemies and explodes (issue #244).</summary>
        Scarecrow = 9,

        /// <summary>Replaces the weapon with a flamethrower for a while (issue #253).</summary>
        Flamethrower = 10,

        /// <summary>Replaces the weapon with a rocket launcher for a while (issue #253).</summary>
        RocketLauncher = 11,
    }
}
