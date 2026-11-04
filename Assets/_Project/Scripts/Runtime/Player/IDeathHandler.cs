namespace RushOps.Player
{
    /// <summary>What happens after the death moment of the player, e.g. the „Gescheitert“ screen (issue #35).</summary>
    public interface IDeathHandler
    {
        /// <summary>Called once after the player died and the death moment passed.</summary>
        void HandleDeath();
    }
}
