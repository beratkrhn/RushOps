using System;
using System.Collections.Generic;
using RushOps.Perks;

namespace RushOps.Endless
{
    /// <summary>
    /// When the endless run offers upgrades and which (F-END-11, decisions T2 and N20): after every n-th cleared round and
    /// after every boss round, a number of different upgrades drawn through an <see cref="IRandomSource"/>, never one at
    /// its highest level. Fewer are offered if fewer are left, none if all are maxed.
    /// </summary>
    public static class UpgradeOfferRules
    {
        /// <summary>
        /// Whether clearing <paramref name="clearedRound"/> brings an upgrade choice: every
        /// <paramref name="everyRounds"/>-th round, and every boss round even if it is not a multiple. 0 or less for
        /// <paramref name="everyRounds"/> means only after boss rounds.
        /// </summary>
        public static bool IsOfferRound(int clearedRound, int everyRounds, bool isBossRound)
        {
            if (clearedRound < 1)
            {
                return false;
            }

            return isBossRound || (everyRounds > 0 && clearedRound % everyRounds == 0);
        }

        /// <summary>
        /// Draws up to <paramref name="count"/> different upgrades of <paramref name="upgrades"/> that are not maxed, in
        /// draw order, into <paramref name="result"/> (cleared first). The same random sequence and the same levels give
        /// the same offer. Allocates nothing if <paramref name="result"/> has room; call once per offer.
        /// </summary>
        /// <returns>The number of upgrades offered.</returns>
        public static int Draw(RunUpgrades upgrades, IRandomSource random, int count, List<UpgradeDefinition> result)
        {
            if (upgrades == null)
            {
                throw new ArgumentNullException(nameof(upgrades));
            }

            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            result.Clear();
            IReadOnlyList<UpgradeDefinition> all = upgrades.Definitions;
            for (int i = 0; i < all.Count; i++)
            {
                if (!upgrades.IsMaxed(all[i]))
                {
                    result.Add(all[i]);
                }
            }

            // Partial Fisher–Yates: the first `take` entries become the offer, in the order they were drawn.
            int take = Math.Min(Math.Max(0, count), result.Count);
            for (int i = 0; i < take; i++)
            {
                int remaining = result.Count - i;
                int pick = i + Math.Min(remaining - 1, (int)(random.Value() * remaining));
                (result[i], result[pick]) = (result[pick], result[i]);
            }

            result.RemoveRange(take, result.Count - take);
            return take;
        }
    }
}
