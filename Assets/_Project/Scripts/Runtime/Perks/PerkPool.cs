using System;
using System.Collections.Generic;

namespace RushOps.Perks
{
    /// <summary>
    /// Decides whether a killed enemy drops a perk and which one (F-PRK-02): a global drop chance, then a weighted
    /// pick from the perks. Perks with a weight of 0 or less never drop. Allocates nothing per roll.
    /// </summary>
    public sealed class PerkPool
    {
        private readonly IReadOnlyList<PerkDefinition> _perks;
        private readonly IRandomSource _random;

        /// <summary>Pool over <paramref name="perks"/> (null entries are skipped) rolling with <paramref name="random"/>.</summary>
        public PerkPool(IReadOnlyList<PerkDefinition> perks, IRandomSource random)
        {
            _perks = perks ?? Array.Empty<PerkDefinition>();
            _random = random ?? throw new ArgumentNullException(nameof(random));
            for (int i = 0; i < _perks.Count; i++)
            {
                TotalWeight += WeightOf(_perks[i]);
            }
        }

        /// <summary>Sum of all positive weights; 0 if nothing can drop.</summary>
        public float TotalWeight { get; }

        /// <summary>
        /// Rolls the drop chance (0..1) and, on success, picks a perk. False if the roll failed or no perk can drop.
        /// </summary>
        public bool TryDrop(float dropChance, out PerkDefinition perk)
        {
            perk = null;
            if (dropChance <= 0f || TotalWeight <= 0f || _random.Value() >= dropChance)
            {
                return false;
            }

            perk = Pick();
            return perk != null;
        }

        /// <summary>Picks a perk by weight with the next random number; null if no perk can drop.</summary>
        public PerkDefinition Pick()
        {
            return Pick(_random.Value());
        }

        /// <summary>
        /// Picks the perk that <paramref name="roll"/> (0..1) lands on when the weights are laid out in list order;
        /// null if no perk can drop.
        /// </summary>
        public PerkDefinition Pick(float roll)
        {
            if (TotalWeight <= 0f)
            {
                return null;
            }

            float target = Math.Clamp(roll, 0f, 1f) * TotalWeight;
            float cumulative = 0f;
            PerkDefinition last = null;
            for (int i = 0; i < _perks.Count; i++)
            {
                float weight = WeightOf(_perks[i]);
                if (weight <= 0f)
                {
                    continue;
                }

                cumulative += weight;
                last = _perks[i];
                if (target < cumulative)
                {
                    return last;
                }
            }

            // A roll of 1 or float rounding lands past the end: the last perk that can drop.
            return last;
        }

        private static float WeightOf(PerkDefinition perk)
        {
            return perk != null && perk.Weight > 0f ? perk.Weight : 0f;
        }
    }
}
