using System;
using System.Collections.Generic;

namespace RushOps.Endless
{
    /// <summary>
    /// The upgrades taken in the running endless run (F-END-12, decisions E10 and N19): a level per upgrade and the
    /// combined values the game reads. Lives for one run only and is never saved; a new run starts with a new instance
    /// or <see cref="Clear"/>. Numeric upgrades stack additively per level (3 × +15 % = +45 %) up to their highest level.
    /// The combined values are worked out on every change, so reading them per frame does not allocate.
    /// </summary>
    public class RunUpgrades
    {
        private readonly UpgradeDefinition[] _definitions;
        private readonly int[] _levels;

        /// <summary>Creates an empty run over <paramref name="definitions"/>; null entries are skipped.</summary>
        public RunUpgrades(IReadOnlyList<UpgradeDefinition> definitions)
        {
            var list = new List<UpgradeDefinition>();
            if (definitions != null)
            {
                foreach (UpgradeDefinition definition in definitions)
                {
                    if (definition != null && !list.Contains(definition))
                    {
                        list.Add(definition);
                    }
                }
            }

            _definitions = list.ToArray();
            _levels = new int[_definitions.Length];
            Recalculate();
        }

        /// <summary>All upgrades of the run, in the order of the config.</summary>
        public IReadOnlyList<UpgradeDefinition> Definitions => _definitions;

        /// <summary>Raised after an upgrade was taken or the run was cleared.</summary>
        public event Action Changed;

        /// <summary>Raised after an upgrade was taken, with the upgrade and its new level.</summary>
        public event Action<UpgradeDefinition, int> Taken;

        /// <summary>Extra wall bounces of player shots.</summary>
        public int ExtraBounces { get; private set; }

        /// <summary>Hearts added to the maximum.</summary>
        public int ExtraHearts { get; private set; }

        /// <summary>Factor on the duration of timed perks; 1 without the upgrade.</summary>
        public float PerkDurationFactor { get; private set; }

        /// <summary>Tiles added to the gold magnet radius.</summary>
        public float MagnetRadiusBonus { get; private set; }

        /// <summary>Whether the dash leaves a fire trail.</summary>
        public bool HasFireTrail { get; private set; }

        /// <summary>Whether the combat drone stays for the rest of the run.</summary>
        public bool HasPermanentCompanion { get; private set; }

        /// <summary>Factor on the move speed; 1 without the upgrade.</summary>
        public float MoveSpeedFactor { get; private set; }

        /// <summary>Factor on the fire rate (shots per second); 1 without the upgrade.</summary>
        public float FireRateFactor { get; private set; }

        /// <summary>Factor on every player damage; 1 without the upgrade.</summary>
        public float DamageFactor { get; private set; }

        /// <summary>Added to the perk drop chance, as probability (0.03 = 3 percentage points).</summary>
        public float PerkChanceBonus { get; private set; }

        /// <summary>Kills fewer the rush meter needs to be full.</summary>
        public int RushKillsReduction { get; private set; }

        /// <summary>Factor on the dash cooldown, between 0 and 1; 1 without the upgrade.</summary>
        public float DashCooldownFactor { get; private set; }

        /// <summary>Number of levels taken in total.</summary>
        public int TotalLevels { get; private set; }

        /// <summary>
        /// Most hearts the upgrades of <paramref name="definitions"/> can add at their highest levels, e.g. for the number
        /// of heart slots in the HUD.
        /// </summary>
        public static int MaxExtraHearts(IReadOnlyList<UpgradeDefinition> definitions)
        {
            double hearts = 0;
            if (definitions != null)
            {
                foreach (UpgradeDefinition definition in definitions)
                {
                    if (definition != null && definition.Kind == UpgradeKind.ExtraHeart)
                    {
                        hearts += (double)definition.ValuePerLevel * definition.MaxLevel;
                    }
                }
            }

            return Whole(hearts);
        }

        /// <summary>The upgrade with <paramref name="id"/>, or null.</summary>
        public UpgradeDefinition Find(string id)
        {
            int index = IndexOf(id);
            return index >= 0 ? _definitions[index] : null;
        }

        /// <summary>Level of the upgrade with <paramref name="id"/>; 0 if not taken or unknown.</summary>
        public int Level(string id)
        {
            int index = IndexOf(id);
            return index >= 0 ? _levels[index] : 0;
        }

        /// <summary>Level of <paramref name="definition"/>; 0 if not taken or unknown.</summary>
        public int Level(UpgradeDefinition definition)
        {
            int index = Array.IndexOf(_definitions, definition);
            return index >= 0 ? _levels[index] : 0;
        }

        /// <summary>Whether <paramref name="definition"/> is at its highest level; unknown upgrades count as maxed.</summary>
        public bool IsMaxed(UpgradeDefinition definition)
        {
            int index = Array.IndexOf(_definitions, definition);
            return index < 0 || _levels[index] >= _definitions[index].MaxLevel;
        }

        /// <summary>Whether every upgrade is at its highest level.</summary>
        public bool AllMaxed
        {
            get
            {
                for (int i = 0; i < _definitions.Length; i++)
                {
                    if (_levels[i] < _definitions[i].MaxLevel)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Raises the upgrade with <paramref name="id"/> by one level. Returns false and changes nothing if the id is
        /// unknown or the upgrade is maxed.
        /// </summary>
        public bool Take(string id)
        {
            int index = IndexOf(id);
            return index >= 0 && TakeAt(index);
        }

        /// <summary>Raises <paramref name="definition"/> by one level; false if unknown or maxed.</summary>
        public bool Take(UpgradeDefinition definition)
        {
            int index = Array.IndexOf(_definitions, definition);
            return index >= 0 && TakeAt(index);
        }

        /// <summary>Drops all levels, for a new run.</summary>
        public void Clear()
        {
            Array.Clear(_levels, 0, _levels.Length);
            Recalculate();
            Changed?.Invoke();
        }

        private bool TakeAt(int index)
        {
            UpgradeDefinition definition = _definitions[index];
            if (_levels[index] >= definition.MaxLevel)
            {
                return false;
            }

            _levels[index]++;
            Recalculate();
            Taken?.Invoke(definition, _levels[index]);
            Changed?.Invoke();
            return true;
        }

        private int IndexOf(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return -1;
            }

            for (int i = 0; i < _definitions.Length; i++)
            {
                if (string.Equals(_definitions[i].Id, id, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        private void Recalculate()
        {
            double bounces = 0, hearts = 0, perkDuration = 0, magnet = 0, moveSpeed = 0, fireRate = 0, damage = 0;
            double perkChance = 0, rushKills = 0, dashCooldown = 0;
            bool fireTrail = false, companion = false;
            int total = 0;

            for (int i = 0; i < _definitions.Length; i++)
            {
                int level = _levels[i];
                if (level <= 0)
                {
                    continue;
                }

                total += level;
                double value = (double)_definitions[i].ValuePerLevel * level;
                switch (_definitions[i].Kind)
                {
                    case UpgradeKind.Bounce: bounces += value; break;
                    case UpgradeKind.ExtraHeart: hearts += value; break;
                    case UpgradeKind.LongerPerks: perkDuration += value; break;
                    case UpgradeKind.GoldMagnet: magnet += value; break;
                    case UpgradeKind.FireTrail: fireTrail = true; break;
                    case UpgradeKind.PermanentCompanion: companion = true; break;
                    case UpgradeKind.QuickFeet: moveSpeed += value; break;
                    case UpgradeKind.FireRate: fireRate += value; break;
                    case UpgradeKind.Damage: damage += value; break;
                    case UpgradeKind.PerkLuck: perkChance += value; break;
                    case UpgradeKind.FastRush: rushKills += value; break;
                    case UpgradeKind.ShortDash: dashCooldown += value; break;
                }
            }

            ExtraBounces = Whole(bounces);
            ExtraHearts = Whole(hearts);
            PerkDurationFactor = Factor(perkDuration);
            MagnetRadiusBonus = (float)Math.Max(0d, magnet);
            HasFireTrail = fireTrail;
            HasPermanentCompanion = companion;
            MoveSpeedFactor = Factor(moveSpeed);
            FireRateFactor = Factor(fireRate);
            DamageFactor = Factor(damage);
            PerkChanceBonus = (float)Math.Round(Math.Max(0d, perkChance), 6);
            RushKillsReduction = Whole(rushKills);
            DashCooldownFactor = (float)Math.Round(Math.Min(1d, Math.Max(0d, 1d - dashCooldown)), 6);
            TotalLevels = total;
        }

        // Rounded to 6 decimals, so 3 × 0.1 gives exactly 1.3 and not 1.2999999.
        private static float Factor(double bonus)
        {
            return (float)Math.Round(1d + Math.Max(0d, bonus), 6);
        }

        private static int Whole(double value)
        {
            return (int)Math.Max(0d, Math.Round(value, MidpointRounding.AwayFromZero));
        }
    }
}
