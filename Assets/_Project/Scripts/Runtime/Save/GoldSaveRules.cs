using RushOps.Loot;

namespace RushOps.Save
{
    /// <summary>
    /// Moves gold between the <see cref="GoldWallet"/> of a level and the <see cref="GoldData"/> of the save game
    /// (issue #236, F-GLD-05, N12). Gold is credited to the wallet at pickup; the save game is brought up to date at the
    /// autosave moments, so gold is kept after death, quitting and a closed app.
    /// </summary>
    public static class GoldSaveRules
    {
        /// <summary>Loads the saved account into <paramref name="wallet"/> when a level starts.</summary>
        public static void Load(GoldData saved, GoldWallet wallet)
        {
            wallet.Restore(saved.Balance, saved.TotalEarned);
        }

        /// <summary>Writes the wallet into <paramref name="saved"/>. True if the save game changed.</summary>
        public static bool Commit(GoldData saved, GoldWallet wallet)
        {
            if (saved.Balance == wallet.Balance && saved.TotalEarned == wallet.TotalEarned)
            {
                return false;
            }

            saved.Balance = wallet.Balance;
            saved.TotalEarned = wallet.TotalEarned;
            return true;
        }
    }
}
