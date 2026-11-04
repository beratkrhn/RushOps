namespace RushOps.Combat
{
    /// <summary>Something on a character that blocks all damage while it is up, e.g. the shield perk (issue #26).</summary>
    public interface IDamageBlocker
    {
        /// <summary>True while no damage hurts the character.</summary>
        bool BlocksDamage { get; }
    }
}
