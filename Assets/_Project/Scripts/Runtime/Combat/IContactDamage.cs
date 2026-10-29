namespace RushOps.Combat
{
    /// <summary>Something that hurts the player on touch, e.g. an enemy.</summary>
    public interface IContactDamage
    {
        /// <summary>Damage per hit in hearts; 0 or less does nothing.</summary>
        int ContactDamage { get; }
    }
}
