using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Endless;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>When upgrades are offered and which (issue #276, F-END-11, T2, N20).</summary>
    public class UpgradeOfferRulesTests
    {
        private sealed class SequenceRandom : IRandomSource
        {
            private readonly float[] _values;
            private int _next;

            public SequenceRandom(params float[] values) => _values = values;

            public float Value() => _values[_next++ % _values.Length];
        }

        private UpgradeTestCatalog _catalog;
        private RunUpgrades _run;
        private EndlessConfig _config;
        private BossDefinition _boss;
        private readonly List<UpgradeDefinition> _offer = new List<UpgradeDefinition>();

        [SetUp]
        public void SetUp()
        {
            _catalog = new UpgradeTestCatalog();
            _run = _catalog.NewRun();
            _boss = ScriptableObject.CreateInstance<BossDefinition>();
            _config = ScriptableObject.CreateInstance<EndlessConfig>();
            _config.ConfigureBosses(10, new[] { _boss }, 0.25f);
            _config.ConfigureUpgrades(3, 3, new List<UpgradeDefinition>(_catalog.All).ToArray());
        }

        [TearDown]
        public void TearDown()
        {
            _catalog.Destroy();
            Object.DestroyImmediate(_boss);
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Config_DefaultsToEveryThirdRoundAndThreeOffers()
        {
            var config = ScriptableObject.CreateInstance<EndlessConfig>();
            try
            {
                Assert.AreEqual(3, config.UpgradeEveryRounds);
                Assert.AreEqual(3, config.UpgradeOffers);
                Assert.IsEmpty(config.Upgrades);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, true)]
        [TestCase(4, false)]
        [TestCase(6, true)]
        [TestCase(9, true)]
        [TestCase(10, true)] // boss round, not a multiple of 3
        [TestCase(11, false)]
        [TestCase(12, true)]
        [TestCase(20, true)] // boss round
        [TestCase(30, true)] // boss round and a multiple of 3: still one choice
        public void IsUpgradeRound_EveryThirdAndEveryBossRound(int clearedRound, bool expected)
        {
            Assert.AreEqual(expected, new EndlessRules(_config).IsUpgradeRound(clearedRound));
        }

        [Test]
        public void IsOfferRound_NoRoundCleared_IsFalse()
        {
            Assert.IsFalse(UpgradeOfferRules.IsOfferRound(0, 3, false));
            Assert.IsFalse(UpgradeOfferRules.IsOfferRound(0, 3, true));
        }

        [Test]
        public void Draw_GivesThreeDifferentUpgrades()
        {
            int count = UpgradeOfferRules.Draw(_run, new SystemRandomSource(42), 3, _offer);

            Assert.AreEqual(3, count);
            Assert.AreEqual(3, _offer.Count);
            CollectionAssert.AllItemsAreUnique(_offer);
        }

        [Test]
        public void Draw_FollowsTheRandomSequence()
        {
            // 0 picks the first left, 0.99 the last left: bounce, then short dash, then longer perks (now first left).
            UpgradeOfferRules.Draw(_run, new SequenceRandom(0f, 0.99f, 0f), 3, _offer);

            CollectionAssert.AreEqual(new[] { _catalog.Bounce, _catalog.ShortDash, _catalog.LongerPerks }, _offer);
        }

        [Test]
        public void Draw_NeverOffersAMaxedUpgrade()
        {
            for (int i = 0; i < 3; i++)
            {
                _run.Take(_catalog.Damage);
            }

            _run.Take(_catalog.FireTrail);
            var random = new SystemRandomSource(7);
            for (int i = 0; i < 200; i++)
            {
                UpgradeOfferRules.Draw(_run, random, 3, _offer);
                CollectionAssert.DoesNotContain(_offer, _catalog.Damage);
                CollectionAssert.DoesNotContain(_offer, _catalog.FireTrail);
                CollectionAssert.AllItemsAreUnique(_offer);
            }
        }

        [Test]
        public void Draw_TwoLeft_GivesTwo()
        {
            _catalog.MaxAllExcept(_run, _catalog.Bounce, _catalog.PerkLuck);

            int count = UpgradeOfferRules.Draw(_run, new SystemRandomSource(1), 3, _offer);

            Assert.AreEqual(2, count);
            CollectionAssert.AreEquivalent(new[] { _catalog.Bounce, _catalog.PerkLuck }, _offer);
        }

        [Test]
        public void Draw_AllMaxed_GivesNone()
        {
            _catalog.MaxAllExcept(_run);

            int count = UpgradeOfferRules.Draw(_run, new SystemRandomSource(1), 3, _offer);

            Assert.AreEqual(0, count);
            Assert.IsEmpty(_offer);
        }

        [Test]
        public void Draw_SameSeed_GivesTheSameOffers()
        {
            var first = new List<UpgradeDefinition>();
            var second = new List<UpgradeDefinition>();
            var a = new SystemRandomSource(DailySeed.FromKey("2026-11-11"));
            var b = new SystemRandomSource(DailySeed.FromKey("2026-11-11"));

            for (int i = 0; i < 5; i++)
            {
                UpgradeOfferRules.Draw(_run, a, 3, first);
                UpgradeOfferRules.Draw(_run, b, 3, second);
                CollectionAssert.AreEqual(first, second, $"offer {i + 1}");
                _run.Take(first[0]);
            }
        }

        [Test]
        public void Draw_ClearsThePreviousOffer()
        {
            _offer.Add(_catalog.Damage);
            _offer.Add(_catalog.Damage);

            UpgradeOfferRules.Draw(_run, new SequenceRandom(0f), 1, _offer);

            CollectionAssert.AreEqual(new[] { _catalog.Bounce }, _offer);
        }
    }
}
