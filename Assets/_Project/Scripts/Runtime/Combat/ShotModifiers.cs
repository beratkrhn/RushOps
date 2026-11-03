namespace RushOps.Combat
{
    /// <summary>
    /// Temporary changes to a weapon, e.g. from perks (issue #25): multipliers for damage and fire rate and whether
    /// every hit kills. Several modifiers combine freely.
    /// </summary>
    public readonly struct ShotModifiers
    {
        public ShotModifiers(float damageMultiplier, float fireRateMultiplier, bool instaKill)
        {
            DamageMultiplier = damageMultiplier;
            FireRateMultiplier = fireRateMultiplier;
            InstaKill = instaKill;
        }

        /// <summary>No change: both multipliers 1, no insta-kill.</summary>
        public static ShotModifiers None => new ShotModifiers(1f, 1f, false);

        /// <summary>Factor on the weapon damage.</summary>
        public float DamageMultiplier { get; }

        /// <summary>Factor on the shots per second.</summary>
        public float FireRateMultiplier { get; }

        /// <summary>True if every hit kills, except bosses (F-BOS-02).</summary>
        public bool InstaKill { get; }
    }
}
