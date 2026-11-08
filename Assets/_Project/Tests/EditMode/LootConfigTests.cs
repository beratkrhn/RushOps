using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Perks;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>The shipped drop pool (F-PRK-02, issue #247): all nine perks with the weights of the Lastenheft.</summary>
    public class LootConfigTests
    {
        private const string LootConfigPath = "Assets/_Project/Data/Perks/LootConfig.asset";

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
        public void Perks_AllNineAreInThePoolOnce_WeightsSumTo118()
        {
            var ids = new HashSet<string>();
            float sum = 0f;
            foreach (PerkDefinition perk in _config.Perks)
            {
                Assert.IsNotNull(perk);
                Assert.IsTrue(ids.Add(perk.Id), $"{perk.Id} is listed twice");
                sum += perk.Weight;
            }

            Assert.AreEqual(9, ids.Count);
            Assert.AreEqual(118f, sum, 1e-4f);
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
        }

        [Test]
        public void Pool_PicksAtTheEndOfTheList_Scarecrow()
        {
            var pool = new PerkPool(_config.Perks, new SystemRandomSource(1));

            Assert.AreEqual("scarecrow", pool.Pick(1f).Id);
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
