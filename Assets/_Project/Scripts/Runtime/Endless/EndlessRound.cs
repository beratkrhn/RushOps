using System;
using RushOps.Bosses;
using RushOps.Combat;
using RushOps.Enemies;

namespace RushOps.Endless
{
    /// <summary>Everything one round of the endless mode brings, as built by <see cref="EndlessRules.Build"/>.</summary>
    public sealed class EndlessRound
    {
        /// <summary>Creates a round; used by <see cref="EndlessRules"/>.</summary>
        public EndlessRound(int number, WaveEntry[] enemies, float spawnInterval, int maxActive, BossDefinition boss,
            float bossHealthFactor, WeaponDefinition[] weapons, WeaponDefinition newWeapon)
        {
            Number = number;
            Enemies = enemies ?? Array.Empty<WaveEntry>();
            SpawnInterval = spawnInterval;
            MaxActive = maxActive;
            Boss = boss;
            BossHealthFactor = bossHealthFactor;
            Weapons = weapons ?? Array.Empty<WeaponDefinition>();
            NewWeapon = newWeapon;
        }

        /// <summary>Round number, from 1.</summary>
        public int Number { get; }

        /// <summary>Regular enemies by type and number; empty in a boss round.</summary>
        public WaveEntry[] Enemies { get; }

        /// <summary>Total number of regular enemies.</summary>
        public int EnemyCount
        {
            get
            {
                int total = 0;
                foreach (WaveEntry entry in Enemies)
                {
                    total += entry.Count;
                }

                return total;
            }
        }

        /// <summary>Seconds between two enemies.</summary>
        public float SpawnInterval { get; }

        /// <summary>Most regular enemies alive at once; bosses do not count.</summary>
        public int MaxActive { get; }

        /// <summary>Whether this is a boss round.</summary>
        public bool IsBossRound => Boss != null;

        /// <summary>The boss of this round, or null.</summary>
        public BossDefinition Boss { get; }

        /// <summary>Factor on the boss's health: 1 at its first appearance in the run; 1 without boss.</summary>
        public float BossHealthFactor { get; }

        /// <summary>Weapons available in this round, start weapon first, then the wall weapons in order.</summary>
        public WeaponDefinition[] Weapons { get; }

        /// <summary>Wall weapon that becomes available at the start of this round, or null.</summary>
        public WeaponDefinition NewWeapon { get; }
    }
}
