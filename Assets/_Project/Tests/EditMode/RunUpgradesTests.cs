using NUnit.Framework;
using RushOps.Endless;

namespace RushOps.Tests.EditMode
{
    /// <summary>Levels and combined values of the run upgrades (issue #276, F-END-12, N19).</summary>
    public class RunUpgradesTests
    {
        private UpgradeTestCatalog _catalog;
        private RunUpgrades _run;

        [SetUp]
        public void SetUp()
        {
            _catalog = new UpgradeTestCatalog();
            _run = _catalog.NewRun();
        }

        [TearDown]
        public void TearDown()
        {
            _catalog.Destroy();
        }

        [Test]
        public void NewRun_HasNoUpgradesAndNeutralValues()
        {
            Assert.AreEqual(0, _run.TotalLevels);
            Assert.AreEqual(0, _run.ExtraBounces);
            Assert.AreEqual(0, _run.ExtraHearts);
            Assert.AreEqual(1f, _run.PerkDurationFactor);
            Assert.AreEqual(0f, _run.MagnetRadiusBonus);
            Assert.IsFalse(_run.HasFireTrail);
            Assert.IsFalse(_run.HasPermanentCompanion);
            Assert.AreEqual(1f, _run.MoveSpeedFactor);
            Assert.AreEqual(1f, _run.FireRateFactor);
            Assert.AreEqual(1f, _run.DamageFactor);
            Assert.AreEqual(0f, _run.PerkChanceBonus);
            Assert.AreEqual(0, _run.RushKillsReduction);
            Assert.AreEqual(1f, _run.DashCooldownFactor);
            Assert.IsFalse(_run.AllMaxed);
        }

        [Test]
        public void Take_ById_RaisesTheLevel()
        {
            Assert.IsTrue(_run.Take("damage"));
            Assert.IsTrue(_run.Take("damage"));

            Assert.AreEqual(2, _run.Level("damage"));
            Assert.AreEqual(2, _run.Level(_catalog.Damage));
            Assert.AreSame(_catalog.Damage, _run.Find("damage"));
        }

        [Test]
        public void Take_UnknownId_ChangesNothing()
        {
            Assert.IsFalse(_run.Take("nope"));
            Assert.IsFalse(_run.Take((string)null));
            Assert.AreEqual(0, _run.TotalLevels);
        }

        [Test]
        public void Take_AtTheHighestLevel_IsRefused()
        {
            Assert.IsTrue(_run.Take(_catalog.GoldMagnet));
            Assert.IsTrue(_run.Take(_catalog.GoldMagnet));
            Assert.IsTrue(_run.IsMaxed(_catalog.GoldMagnet));

            Assert.IsFalse(_run.Take(_catalog.GoldMagnet));
            Assert.AreEqual(2, _run.Level(_catalog.GoldMagnet));
        }

        [Test]
        public void Take_RaisesEvents()
        {
            int changed = 0;
            UpgradeDefinition taken = null;
            int level = 0;
            _run.Changed += () => changed++;
            _run.Taken += (u, l) => { taken = u; level = l; };

            _run.Take(_catalog.Bounce);
            _run.Take(_catalog.Bounce);

            Assert.AreEqual(2, changed);
            Assert.AreSame(_catalog.Bounce, taken);
            Assert.AreEqual(2, level);
        }

        // Every numeric upgrade, level by level up to its highest; one more take changes nothing.
        [TestCase("bounce", new[] { 1f, 2f, 3f })]
        [TestCase("extra_heart", new[] { 1f, 2f, 3f })]
        [TestCase("longer_perks", new[] { 1.3f, 1.6f, 1.9f })]
        [TestCase("gold_magnet", new[] { 1f, 2f })]
        [TestCase("quick_feet", new[] { 1.1f, 1.2f, 1.3f })]
        [TestCase("fire_rate", new[] { 1.15f, 1.3f, 1.45f })]
        [TestCase("damage", new[] { 1.15f, 1.3f, 1.45f })]
        [TestCase("perk_luck", new[] { 0.03f, 0.06f, 0.09f })]
        [TestCase("fast_rush", new[] { 5f })]
        [TestCase("short_dash", new[] { 0.75f, 0.5f })]
        public void NumericUpgrade_StacksUpToItsHighestLevel(string id, float[] perLevel)
        {
            foreach (float expected in perLevel)
            {
                Assert.IsTrue(_run.Take(id));
                Assert.AreEqual(expected, Value(id), 1e-6f, $"{id} at level {_run.Level(id)}");
            }

            Assert.IsFalse(_run.Take(id));
            Assert.AreEqual(perLevel[perLevel.Length - 1], Value(id), 1e-6f);
            Assert.AreEqual(perLevel.Length, _run.Level(id));
        }

        [Test]
        public void FireTrailAndCompanion_AreSwitchesWithOneLevel()
        {
            Assert.IsTrue(_run.Take(_catalog.FireTrail));
            Assert.IsTrue(_run.Take(_catalog.Companion));

            Assert.IsTrue(_run.HasFireTrail);
            Assert.IsTrue(_run.HasPermanentCompanion);
            Assert.IsFalse(_run.Take(_catalog.FireTrail));
            Assert.IsFalse(_run.Take(_catalog.Companion));
        }

        [Test]
        public void Upgrades_DoNotTouchEachOther()
        {
            _run.Take(_catalog.Damage);
            _run.Take(_catalog.QuickFeet);

            Assert.AreEqual(1.15f, _run.DamageFactor, 1e-6f);
            Assert.AreEqual(1.1f, _run.MoveSpeedFactor, 1e-6f);
            Assert.AreEqual(1f, _run.FireRateFactor);
            Assert.AreEqual(2, _run.TotalLevels);
        }

        [Test]
        public void AllMaxed_AfterEveryUpgradeIsAtItsHighestLevel()
        {
            _catalog.MaxAllExcept(_run);

            Assert.IsTrue(_run.AllMaxed);
            Assert.AreEqual(3 + 3 + 3 + 2 + 1 + 1 + 3 + 3 + 3 + 3 + 1 + 2, _run.TotalLevels);
        }

        [Test]
        public void Clear_StartsANewRunWithoutUpgrades()
        {
            _run.Take(_catalog.Damage);
            _run.Take(_catalog.FireTrail);
            int changed = 0;
            _run.Changed += () => changed++;

            _run.Clear();

            Assert.AreEqual(1, changed);
            Assert.AreEqual(0, _run.TotalLevels);
            Assert.AreEqual(1f, _run.DamageFactor);
            Assert.IsFalse(_run.HasFireTrail);
        }

        [Test]
        public void Constructor_SkipsNullsAndDuplicates()
        {
            var run = new RunUpgrades(new[] { _catalog.Damage, null, _catalog.Damage });

            Assert.AreEqual(1, run.Definitions.Count);
            Assert.IsTrue(run.IsMaxed(_catalog.Bounce), "unknown upgrades count as maxed");
        }

        private float Value(string id)
        {
            switch (id)
            {
                case "bounce": return _run.ExtraBounces;
                case "extra_heart": return _run.ExtraHearts;
                case "longer_perks": return _run.PerkDurationFactor;
                case "gold_magnet": return _run.MagnetRadiusBonus;
                case "quick_feet": return _run.MoveSpeedFactor;
                case "fire_rate": return _run.FireRateFactor;
                case "damage": return _run.DamageFactor;
                case "perk_luck": return _run.PerkChanceBonus;
                case "fast_rush": return _run.RushKillsReduction;
                case "short_dash": return _run.DashCooldownFactor;
                default: throw new System.ArgumentException(id);
            }
        }
    }
}
