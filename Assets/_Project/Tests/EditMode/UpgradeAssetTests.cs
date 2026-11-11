using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Endless;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    /// <summary>Checks the start values in Data/Endless/Upgrades and the endless config (issue #276, backlog v1.7.0).</summary>
    public class UpgradeAssetTests
    {
        private const string ConfigPath = "Assets/_Project/Data/Endless/EndlessConfig.asset";

        private static EndlessConfig LoadConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<EndlessConfig>(ConfigPath);
            Assert.IsNotNull(config, $"missing {ConfigPath}; run RushOps → Daten → Endlos-Konfiguration erzeugen");
            return config;
        }

        [Test]
        public void Config_EveryThirdRound_ThreeOffers()
        {
            EndlessConfig config = LoadConfig();

            Assert.AreEqual(3, config.UpgradeEveryRounds);
            Assert.AreEqual(3, config.UpgradeOffers);
        }

        [TestCase(0, "bounce", UpgradeKind.Bounce, "Abpraller", 1f, 3)]
        [TestCase(1, "extra_heart", UpgradeKind.ExtraHeart, "+1 Herz", 1f, 3)]
        [TestCase(2, "longer_perks", UpgradeKind.LongerPerks, "Längere Perks", 0.3f, 3)]
        [TestCase(3, "gold_magnet", UpgradeKind.GoldMagnet, "Gold-Magnet", 1f, 2)]
        [TestCase(4, "fire_trail", UpgradeKind.FireTrail, "Feuerspur", 1f, 1)]
        [TestCase(5, "permanent_companion", UpgradeKind.PermanentCompanion, "Begleiter dauerhaft", 1f, 1)]
        [TestCase(6, "quick_feet", UpgradeKind.QuickFeet, "Schnelle Füße", 0.1f, 3)]
        [TestCase(7, "fire_rate", UpgradeKind.FireRate, "Feuerrate", 0.15f, 3)]
        [TestCase(8, "damage", UpgradeKind.Damage, "Schaden", 0.15f, 3)]
        [TestCase(9, "perk_luck", UpgradeKind.PerkLuck, "Perk-Glück", 0.03f, 3)]
        [TestCase(10, "fast_rush", UpgradeKind.FastRush, "Schneller Rush", 5f, 1)]
        [TestCase(11, "short_dash", UpgradeKind.ShortDash, "Kurzer Dash", 0.25f, 2)]
        public void Upgrade_HasItsStartValues(int index, string id, UpgradeKind kind, string name, float value, int max)
        {
            UpgradeDefinition[] upgrades = LoadConfig().Upgrades;
            Assert.AreEqual(12, upgrades.Length);

            UpgradeDefinition upgrade = upgrades[index];
            Assert.IsNotNull(upgrade);
            Assert.AreEqual(id, upgrade.Id);
            Assert.AreEqual(kind, upgrade.Kind);
            Assert.AreEqual(name, upgrade.DisplayName);
            Assert.IsNotEmpty(upgrade.Description);
            Assert.AreEqual(value, upgrade.ValuePerLevel, 1e-6f);
            Assert.AreEqual(max, upgrade.MaxLevel);
            StringAssert.StartsWith(UpgradeCatalogPath, AssetDatabase.GetAssetPath(upgrade));
        }

        [Test]
        public void Upgrades_HaveUniqueIdsAndKinds()
        {
            var ids = new HashSet<string>();
            var kinds = new HashSet<UpgradeKind>();
            foreach (UpgradeDefinition upgrade in LoadConfig().Upgrades)
            {
                Assert.IsTrue(ids.Add(upgrade.Id), $"id {upgrade.Id} twice");
                Assert.IsTrue(kinds.Add(upgrade.Kind), $"kind {upgrade.Kind} twice");
            }
        }

        [Test]
        public void FastRush_FillsTheMeterAt20()
        {
            var rush = AssetDatabase.LoadAssetAtPath<RushOps.Rush.RushConfig>("Assets/_Project/Data/Player/RushConfig.asset");
            var run = new RunUpgrades(LoadConfig().Upgrades);
            run.Take("fast_rush");

            Assert.AreEqual(20, rush.KillsToFill - run.RushKillsReduction);
        }

        private const string UpgradeCatalogPath = "Assets/_Project/Data/Endless/Upgrades/";
    }
}
