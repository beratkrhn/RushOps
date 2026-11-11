using System.Collections.Generic;
using RushOps.Endless;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>The 12 upgrades of the backlog as in-memory assets, for the upgrade tests (issue #276).</summary>
    internal sealed class UpgradeTestCatalog
    {
        private readonly List<UpgradeDefinition> _all = new List<UpgradeDefinition>();

        public UpgradeTestCatalog()
        {
            Bounce = Add("bounce", UpgradeKind.Bounce, 1f, 3);
            ExtraHeart = Add("extra_heart", UpgradeKind.ExtraHeart, 1f, 3);
            LongerPerks = Add("longer_perks", UpgradeKind.LongerPerks, 0.3f, 3);
            GoldMagnet = Add("gold_magnet", UpgradeKind.GoldMagnet, 1f, 2);
            FireTrail = Add("fire_trail", UpgradeKind.FireTrail, 1f, 1);
            Companion = Add("permanent_companion", UpgradeKind.PermanentCompanion, 1f, 1);
            QuickFeet = Add("quick_feet", UpgradeKind.QuickFeet, 0.1f, 3);
            FireRate = Add("fire_rate", UpgradeKind.FireRate, 0.15f, 3);
            Damage = Add("damage", UpgradeKind.Damage, 0.15f, 3);
            PerkLuck = Add("perk_luck", UpgradeKind.PerkLuck, 0.03f, 3);
            FastRush = Add("fast_rush", UpgradeKind.FastRush, 5f, 1);
            ShortDash = Add("short_dash", UpgradeKind.ShortDash, 0.25f, 2);
        }

        public IReadOnlyList<UpgradeDefinition> All => _all;
        public UpgradeDefinition Bounce { get; }
        public UpgradeDefinition ExtraHeart { get; }
        public UpgradeDefinition LongerPerks { get; }
        public UpgradeDefinition GoldMagnet { get; }
        public UpgradeDefinition FireTrail { get; }
        public UpgradeDefinition Companion { get; }
        public UpgradeDefinition QuickFeet { get; }
        public UpgradeDefinition FireRate { get; }
        public UpgradeDefinition Damage { get; }
        public UpgradeDefinition PerkLuck { get; }
        public UpgradeDefinition FastRush { get; }
        public UpgradeDefinition ShortDash { get; }

        public RunUpgrades NewRun() => new RunUpgrades(_all);

        public void Destroy()
        {
            foreach (UpgradeDefinition upgrade in _all)
            {
                Object.DestroyImmediate(upgrade);
            }

            _all.Clear();
        }

        /// <summary>Takes every upgrade up to its highest level in <paramref name="run"/>, except <paramref name="keep"/>.</summary>
        public void MaxAllExcept(RunUpgrades run, params UpgradeDefinition[] keep)
        {
            foreach (UpgradeDefinition upgrade in _all)
            {
                if (System.Array.IndexOf(keep, upgrade) >= 0)
                {
                    continue;
                }

                while (run.Take(upgrade))
                {
                }
            }
        }

        private UpgradeDefinition Add(string id, UpgradeKind kind, float value, int max)
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.name = id;
            upgrade.Configure(id, kind, id, "", value, max);
            _all.Add(upgrade);
            return upgrade;
        }
    }
}
