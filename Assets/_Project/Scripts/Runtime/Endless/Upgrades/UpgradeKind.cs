namespace RushOps.Endless
{
    /// <summary>What a run upgrade changes (F-END-12, decision N19). The value per level of the asset is read per kind.</summary>
    public enum UpgradeKind
    {
        /// <summary>Player shots bounce off walls; value = extra bounces per level.</summary>
        Bounce = 0,

        /// <summary>Most hearts +value per level, heals the same.</summary>
        ExtraHeart = 1,

        /// <summary>Timed perks last longer; value = share per level (0.3 = +30 %).</summary>
        LongerPerks = 2,

        /// <summary>Gold magnet radius; value = tiles per level.</summary>
        GoldMagnet = 3,

        /// <summary>The dash leaves burning tiles; on or off.</summary>
        FireTrail = 4,

        /// <summary>The combat drone stays for the rest of the run; on or off.</summary>
        PermanentCompanion = 5,

        /// <summary>Move speed; value = share per level (0.1 = +10 %).</summary>
        QuickFeet = 6,

        /// <summary>Fire rate; value = share per level (0.15 = +15 %).</summary>
        FireRate = 7,

        /// <summary>Every player damage; value = share per level (0.15 = +15 %).</summary>
        Damage = 8,

        /// <summary>Perk drop chance; value = probability points per level (0.03 = +3 percentage points).</summary>
        PerkLuck = 9,

        /// <summary>Rush meter fills with fewer kills; value = kills less per level.</summary>
        FastRush = 10,

        /// <summary>Dash cooldown; value = share less per level (0.25 = −25 %).</summary>
        ShortDash = 11,
    }
}
