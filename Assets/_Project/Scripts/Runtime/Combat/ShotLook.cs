namespace RushOps.Combat
{
    /// <summary>Which sprite a projectile shows, so the player sees an active offensive perk (issue #25).</summary>
    public enum ShotLook
    {
        /// <summary>The normal bullet.</summary>
        Normal = 0,

        /// <summary>Damage is raised, e.g. by the double damage perk.</summary>
        DoubleDamage = 1,

        /// <summary>Every hit kills, except bosses.</summary>
        InstaKill = 2,
    }
}
