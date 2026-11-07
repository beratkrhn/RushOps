using System;

namespace RushOps.Loot
{
    /// <summary>
    /// The player's gold (F-GLD-02 to F-GLD-05): spendable balance, gold earned over all time and gold earned in the
    /// current level or run. Spending lowers only the balance.
    /// </summary>
    public sealed class GoldWallet
    {
        /// <param name="balance">Saved balance, below 0 counts as 0.</param>
        /// <param name="totalEarned">Saved gold earned over all time, below 0 counts as 0.</param>
        public GoldWallet(int balance = 0, int totalEarned = 0)
        {
            Balance = Math.Max(0, balance);
            TotalEarned = Math.Max(0, totalEarned);
        }

        /// <summary>Raised when <see cref="Balance"/> changes, with the new balance.</summary>
        public event Action<int> BalanceChanged;

        /// <summary>Gold the player can spend.</summary>
        public int Balance { get; private set; }

        /// <summary>Gold earned over all time, not lowered by spending.</summary>
        public int TotalEarned { get; private set; }

        /// <summary>Gold earned since the last <see cref="ResetRun"/>.</summary>
        public int EarnedThisRun { get; private set; }

        /// <summary>Adds <paramref name="amount"/> to balance, total and run; ignores amounts of 0 or less.</summary>
        public void Credit(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Balance += amount;
            TotalEarned += amount;
            EarnedThisRun += amount;
            BalanceChanged?.Invoke(Balance);
        }

        /// <summary>True if the balance covers <paramref name="amount"/>.</summary>
        public bool CanAfford(int amount) => amount >= 0 && Balance >= amount;

        /// <summary>Takes <paramref name="amount"/> from the balance, only if there is enough. True on success.</summary>
        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount))
            {
                return false;
            }

            if (amount > 0)
            {
                Balance -= amount;
                BalanceChanged?.Invoke(Balance);
            }

            return true;
        }

        /// <summary>Starts counting the gold of a new level or run.</summary>
        public void ResetRun() => EarnedThisRun = 0;
    }
}
