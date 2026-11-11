using System;
using System.Collections.Generic;
using RushOps.Bosses;
using RushOps.Combat;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// The rules of the endless mode (F-END-03 to F-END-07, F-END-10): turns the values of an <see cref="EndlessConfig"/> into
    /// enemies, dog rounds, spawn interval, limit, boss and weapons of one round. Rounds count from 1; lower numbers count as 1.
    /// A round is played as a <see cref="WaveDefinition"/> with exactly one wave (ADR 0013).
    /// </summary>
    public class EndlessRules
    {
        // Shares are rounded to this many decimals, so float noise does not flip a count at exactly .5.
        private const int ShareDecimals = 4;
        private const double RoundingEpsilon = 1e-6;

        private readonly EndlessConfig _config;

        /// <summary>Creates the rules over <paramref name="config"/>.</summary>
        public EndlessRules(EndlessConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>The values these rules are built on.</summary>
        public EndlessConfig Config => _config;

        /// <summary>Whether <paramref name="round"/> is a boss round: every n-th round, if any boss is set (E3).</summary>
        public bool IsBossRound(int round)
        {
            round = Clamp(round);
            return _config.Bosses.Length > 0 && round % _config.BossEveryRounds == 0;
        }

        /// <summary>The boss of <paramref name="round"/>, in the order of the config and then over again; null in other rounds.</summary>
        public BossDefinition Boss(int round)
        {
            if (!IsBossRound(round))
            {
                return null;
            }

            BossDefinition[] bosses = _config.Bosses;
            return bosses[(BossAppearance(round) - 1) % bosses.Length];
        }

        /// <summary>
        /// Which boss appearance of the run <paramref name="round"/> brings: 1 in the first boss round, 2 in the second
        /// and so on; 0 in other rounds.
        /// </summary>
        public int BossAppearance(int round)
        {
            return IsBossRound(round) ? Clamp(round) / _config.BossEveryRounds : 0;
        }

        /// <summary>
        /// Factor on the boss's health in <paramref name="round"/>: 1 at the first boss of the run, then +growth with every
        /// further boss appearance, whichever boss it is (F-END-05: 1, 1.25, 1.5, … for +25 %). 1 in other rounds.
        /// </summary>
        public float BossHealthFactor(int round)
        {
            int appearance = BossAppearance(round);
            return appearance > 0 ? 1f + _config.BossHealthGrowth * (appearance - 1) : 1f;
        }

        /// <summary>Health of a boss with <paramref name="baseHealth"/> in <paramref name="round"/>, rounded, at least 1.</summary>
        public int BossHealth(int baseHealth, int round)
        {
            double health = (double)baseHealth * BossHealthFactor(round);
            return (int)Math.Max(1, Math.Min(int.MaxValue, Math.Round(health, MidpointRounding.AwayFromZero)));
        }

        /// <summary>
        /// Whether <paramref name="round"/> is a dog round with dogs only (F-END-10): every n-th round, if a dog is set. A
        /// boss round never is one; if it falls on a dog round, the following round is the dog round instead (N6).
        /// </summary>
        public bool IsDogRound(int round)
        {
            int every = _config.DogRoundEveryRounds;
            if (_config.DogEnemy == null || every <= 0)
            {
                return false;
            }

            round = Clamp(round);
            if (IsBossRound(round))
            {
                return false;
            }

            if (round % every == 0)
            {
                return true;
            }

            int previous = round - 1;
            return previous >= 1 && previous % every == 0 && IsBossRound(previous);
        }

        /// <summary>
        /// Number of regular enemies in <paramref name="round"/>: first round plus a step per round, in a dog round
        /// rounded up to full packs; 0 in a boss round.
        /// </summary>
        public int EnemyCount(int round)
        {
            if (IsBossRound(round))
            {
                return 0;
            }

            long count = _config.EnemiesFirstRound + (long)_config.EnemiesPerRound * (Clamp(round) - 1);
            int regular = (int)Math.Min(int.MaxValue, count);
            return IsDogRound(round) ? PackRules.RoundUpToPacks(regular, _config.DogEnemy.PackSize) : regular;
        }

        /// <summary>
        /// Share of <paramref name="share"/>'s type in <paramref name="round"/>: 0 before its first round, then from its
        /// start share up by a step per round to its largest share.
        /// </summary>
        public static float Share(EndlessEnemyShare share, int round)
        {
            round = Clamp(round);
            if (share.Enemy == null || round < share.FirstRound)
            {
                return 0f;
            }

            double value = share.StartShare + (double)share.SharePerRound * (round - share.FirstRound);
            value = Math.Min(share.MaxShare, value);
            return (float)Math.Round(Math.Max(0d, value), ShareDecimals);
        }

        /// <summary>
        /// The regular enemies of <paramref name="round"/> by type and number, base type first, then the heavier types in
        /// the order of the table (F-END-04). Every heavier type that has joined brings at least one enemy; the base type
        /// fills up the rest. A dog round has dogs only. Empty in a boss round. Allocates; call once per round, not per
        /// frame.
        /// </summary>
        public WaveEntry[] EnemyMix(int round)
        {
            int total = EnemyCount(round);
            if (total <= 0)
            {
                return Array.Empty<WaveEntry>();
            }

            if (IsDogRound(round))
            {
                return new[] { new WaveEntry(_config.DogEnemy, total) };
            }

            EndlessEnemyShare[] shares = _config.EnemyShares;
            var counts = new int[shares.Length];
            double shareSum = 0d;
            for (int i = 0; i < shares.Length; i++)
            {
                shareSum += Share(shares[i], round);
            }

            // More than 100 % in the table: scale all heavier types down evenly, no walkers are left then.
            double scale = shareSum > 1d ? 1d / shareSum : 1d;
            long heavy = 0;
            for (int i = 0; i < shares.Length; i++)
            {
                float share = Share(shares[i], round);
                if (share <= 0f)
                {
                    continue;
                }

                double exact = total * share * scale;
                counts[i] = (int)Math.Max(1d, Math.Floor(exact + 0.5d + RoundingEpsilon));
                heavy += counts[i];
            }

            // Rounding or the minimum of one may overshoot the total: take from the most numerous type.
            while (heavy > total)
            {
                int largest = 0;
                for (int i = 1; i < counts.Length; i++)
                {
                    if (counts[i] > counts[largest])
                    {
                        largest = i;
                    }
                }

                counts[largest]--;
                heavy--;
            }

            var entries = new List<WaveEntry>(shares.Length + 1);
            int baseCount = total - (int)heavy;
            if (baseCount > 0 && _config.BaseEnemy != null)
            {
                entries.Add(new WaveEntry(_config.BaseEnemy, baseCount));
            }

            for (int i = 0; i < shares.Length; i++)
            {
                if (counts[i] > 0)
                {
                    entries.Add(new WaveEntry(shares[i].Enemy, counts[i]));
                }
            }

            return entries.ToArray();
        }

        /// <summary>Seconds between two enemies in <paramref name="round"/>: shrinks per round down to the shortest interval.</summary>
        public float SpawnInterval(int round)
        {
            double interval = _config.SpawnIntervalFirstRound - (double)_config.SpawnIntervalStep * (Clamp(round) - 1);
            double min = Math.Min(_config.SpawnIntervalMin, _config.SpawnIntervalFirstRound);
            return (float)Math.Round(Math.Max(min, interval), ShareDecimals);
        }

        /// <summary>
        /// Most regular enemies alive at once in <paramref name="round"/> (F-END-04, N3): start value, grown by a step at
        /// every n-th round, at most the upper limit.
        /// </summary>
        public int MaxActive(int round)
        {
            long steps = Clamp(round) / _config.MaxActiveEveryRounds;
            long value = _config.MaxActiveStart + steps * _config.MaxActiveStep;
            long cap = Math.Max(_config.MaxActiveCap, _config.MaxActiveStart);
            return (int)Math.Min(cap, value);
        }

        /// <summary>Whether <paramref name="weapon"/> can be used in <paramref name="round"/> (F-END-07).</summary>
        public bool IsWeaponAvailable(WeaponDefinition weapon, int round)
        {
            if (weapon == null)
            {
                return false;
            }

            if (weapon == _config.StartWeapon)
            {
                return true;
            }

            foreach (EndlessWeaponRound entry in _config.WallWeapons)
            {
                if (entry.Weapon == weapon && Clamp(round) >= entry.Round)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Weapons available in <paramref name="round"/>: start weapon first, then the unlocked wall weapons in order.</summary>
        public WeaponDefinition[] AvailableWeapons(int round)
        {
            var weapons = new List<WeaponDefinition>(_config.WallWeapons.Length + 1);
            if (_config.StartWeapon != null)
            {
                weapons.Add(_config.StartWeapon);
            }

            foreach (EndlessWeaponRound entry in _config.WallWeapons)
            {
                if (entry.Weapon != null && Clamp(round) >= entry.Round && !weapons.Contains(entry.Weapon))
                {
                    weapons.Add(entry.Weapon);
                }
            }

            return weapons.ToArray();
        }

        /// <summary>The wall weapon that becomes available at the start of <paramref name="round"/>, or null.</summary>
        public WeaponDefinition NewWeapon(int round)
        {
            foreach (EndlessWeaponRound entry in _config.WallWeapons)
            {
                if (entry.Weapon != null && entry.Round == Clamp(round))
                {
                    return entry.Weapon;
                }
            }

            return null;
        }

        /// <summary>
        /// Whether clearing <paramref name="clearedRound"/> brings an upgrade choice (F-END-11, T2, N20): every n-th round
        /// and every boss round.
        /// </summary>
        public bool IsUpgradeRound(int clearedRound)
        {
            return UpgradeOfferRules.IsOfferRound(clearedRound, _config.UpgradeEveryRounds,
                clearedRound >= 1 && IsBossRound(clearedRound));
        }

        /// <summary>Everything <paramref name="round"/> brings.</summary>
        public EndlessRound Build(int round)
        {
            round = Clamp(round);
            return new EndlessRound(round, EnemyMix(round), SpawnInterval(round), MaxActive(round), Boss(round),
                BossHealthFactor(round), AvailableWeapons(round), NewWeapon(round), IsDogRound(round));
        }

        /// <summary>
        /// A new <see cref="WaveDefinition"/> that plays <paramref name="round"/> as exactly one wave without repeat; empty
        /// in a boss round. The caller owns it and destroys it after the round.
        /// </summary>
        public WaveDefinition CreateWave(int round)
        {
            var wave = ScriptableObject.CreateInstance<WaveDefinition>();
            wave.name = $"Endless_Round{Clamp(round)}";
            wave.hideFlags = HideFlags.DontSave;
            WaveEntry[] enemies = EnemyMix(round);
            Wave[] waves = enemies.Length > 0 ? new[] { new Wave(enemies) } : Array.Empty<Wave>();
            // The pause between rounds belongs to the run (EndlessRun), not to the wave.
            wave.Configure(waves, SpawnInterval(round), 0f, AfterLastWave.End);
            return wave;
        }

        private static int Clamp(int round)
        {
            return Math.Max(1, round);
        }
    }
}
