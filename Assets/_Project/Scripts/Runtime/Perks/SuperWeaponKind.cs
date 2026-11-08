namespace RushOps.Perks
{
    /// <summary>Which super weapon replaces the player's weapon right now (F-PRK-09).</summary>
    public enum SuperWeaponKind
    {
        /// <summary>No super weapon, the normal weapon fires.</summary>
        None = 0,

        /// <summary>Flamethrower (F-PRK-10).</summary>
        Flamethrower = 1,

        /// <summary>Rocket launcher (F-PRK-11).</summary>
        RocketLauncher = 2,
    }
}
