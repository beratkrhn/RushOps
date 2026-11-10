using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Perks;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>The shipped drop pool (F-PRK-02, issues #247 and #256): all eleven perks with the weights of the Lastenheft.</summary>
    public class LootConfigTests
    {
        private const string LootConfigPath = "Assets/_Project/Data/Perks/LootConfig.asset";
        private const string FlamethrowerConfigPath = "Assets/_Project/Data/Weapons/FlamethrowerConfig.asset";
        private const string RocketLauncherConfigPath = "Assets/_Project/Data/Weapons/RocketLauncherConfig.asset";

        private LootConfig _config;

        [SetUp]
        public void LoadConfig()
        {
            _config = AssetDatabase.LoadAssetAtPath<LootConfig>(LootConfigPath);
            Assert.IsNotNull(_config, LootConfigPath);
        }

        [Test]
        public void DropChance_StaysAtTwelvePercent()
        {
            Assert.AreEqual(0.12f, _config.DropChance, 1e-5f);
        }

        [Test]
        public void Perks_WeightsOfCompanionAndScarecrow_AreTenAndEight()
        {
            Assert.AreEqual(10f, WeightOf("companion"), 1e-5f);
            Assert.AreEqual(8f, WeightOf("scarecrow"), 1e-5f);
        }

        [Test]
        public void Perks_AllElevenAreInThePoolOnce_WeightsSumTo126()
        {
            var ids = new HashSet<string>();
            float sum = 0f;
            foreach (PerkDefinition perk in _config.Perks)
            {
                Assert.IsNotNull(perk);
                Assert.IsTrue(ids.Add(perk.Id), $"{perk.Id} is listed twice");
                sum += perk.Weight;
            }

            Assert.AreEqual(11, ids.Count);
            Assert.AreEqual(126f, sum, 1e-4f);
        }

        [Test]
        public void Pool_CanReturnCompanionAndScarecrow()
        {
            var pool = new PerkPool(_config.Perks, new SystemRandomSource(1));
            var seen = new HashSet<string>();
            for (int i = 0; i < 5000; i++)
            {
                seen.Add(pool.Pick().Id);
            }

            Assert.IsTrue(seen.Contains("companion"));
            Assert.IsTrue(seen.Contains("scarecrow"));
            Assert.IsTrue(seen.Contains("flamethrower"));
            Assert.IsTrue(seen.Contains("rocket_launcher"));
        }

        [Test]
        public void Pool_PicksAtTheEndOfTheList_RocketLauncher()
        {
            var pool = new PerkPool(_config.Perks, new SystemRandomSource(1));

            Assert.AreEqual("rocket_launcher", pool.Pick(1f).Id);
        }

        [Test]
        public void CompanionAndScarecrow_AreTimedPerksWithIconAndGermanName()
        {
            PerkDefinition companion = Find("companion");
            PerkDefinition scarecrow = Find("scarecrow");

            Assert.AreEqual(PerkKind.Timed, companion.Kind);
            Assert.AreEqual(15f, companion.Duration, 1e-5f);
            Assert.AreEqual("Begleiter", companion.DisplayName);
            Assert.IsNotNull(companion.Icon);
            Assert.AreEqual(PerkKind.Timed, scarecrow.Kind);
            Assert.AreEqual(8f, scarecrow.Duration, 1e-5f);
            Assert.AreEqual("Strohmann", scarecrow.DisplayName);
            Assert.IsNotNull(scarecrow.Icon);
        }

        [Test]
        public void Perks_WeightsOfFlamethrowerAndRocketLauncher_AreFourEach()
        {
            Assert.AreEqual(4f, WeightOf("flamethrower"), 1e-5f);
            Assert.AreEqual(4f, WeightOf("rocket_launcher"), 1e-5f);
        }

        [Test]
        public void SuperWeapons_AreTwelveSecondTimedPerksWithIconAndGermanName()
        {
            PerkDefinition flamethrower = Find("flamethrower");
            PerkDefinition rocketLauncher = Find("rocket_launcher");

            Assert.AreEqual(PerkKind.Timed, flamethrower.Kind);
            Assert.AreEqual(12f, flamethrower.Duration, 1e-5f);
            Assert.AreEqual("Flammenwerfer", flamethrower.DisplayName);
            Assert.IsNotNull(flamethrower.Icon);
            Assert.AreEqual(PerkKind.Timed, rocketLauncher.Kind);
            Assert.AreEqual(12f, rocketLauncher.Duration, 1e-5f);
            Assert.AreEqual("Raketenwerfer", rocketLauncher.DisplayName);
            Assert.IsNotNull(rocketLauncher.Icon);
        }

        [Test]
        public void SuperWeapons_PerkDurationMatchesTheWeaponConfig()
        {
            var flamethrower = AssetDatabase.LoadAssetAtPath<FlamethrowerConfig>(FlamethrowerConfigPath);
            var rocketLauncher = AssetDatabase.LoadAssetAtPath<RocketLauncherConfig>(RocketLauncherConfigPath);
            Assert.IsNotNull(flamethrower, FlamethrowerConfigPath);
            Assert.IsNotNull(rocketLauncher, RocketLauncherConfigPath);

            // The HUD timer comes from the perk, the weapon in the hands from the config; both must end together.
            Assert.AreEqual(flamethrower.Duration, Find("flamethrower").Duration, 1e-5f);
            Assert.AreEqual(rocketLauncher.Duration, Find("rocket_launcher").Duration, 1e-5f);
        }

        private float WeightOf(string id)
        {
            return Find(id).Weight;
        }

        private PerkDefinition Find(string id)
        {
            foreach (PerkDefinition perk in _config.Perks)
            {
                if (perk != null && perk.Id == id)
                {
                    return perk;
                }
            }

            Assert.Fail($"{id} is not in the drop pool");
            return null;
        }
    }
}
