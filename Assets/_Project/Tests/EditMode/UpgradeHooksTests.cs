using NUnit.Framework;
using RushOps.Combat;
using RushOps.Endless;
using RushOps.Rush;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Pure hooks the run upgrades use: higher maximum hearts, earlier full rush meter (issue #277).</summary>
    public class UpgradeHooksTests
    {
        [Test]
        public void Health_SetMaxHigher_HealsTheGain()
        {
            var health = new Health(5);
            health.TakeDamage(2);
            int changes = 0;
            health.Changed += (current, max) => changes++;

            health.SetMax(6);

            Assert.AreEqual(6, health.Max);
            Assert.AreEqual(4, health.Current);
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void Health_SetMaxLower_CutsTheCurrent()
        {
            var health = new Health(8);

            health.SetMax(5);

            Assert.AreEqual(5, health.Max);
            Assert.AreEqual(5, health.Current);
        }

        [Test]
        public void Health_SetMax_IgnoredWhenDeadAndAtLeastOne()
        {
            var dead = new Health(2);
            dead.TakeDamage(2);
            dead.SetMax(4);
            Assert.AreEqual(2, dead.Max);
            Assert.AreEqual(0, dead.Current);

            var health = new Health(3);
            health.SetMax(0);
            Assert.AreEqual(1, health.Max);
            Assert.AreEqual(1, health.Current);
        }

        [Test]
        public void RushMeter_LowerTarget_FillAboveIsFullAtOnce()
        {
            var meter = new RushMeter(25, 5f);
            meter.RegisterKill(22);
            int filled = 0;
            meter.Filled += () => filled++;

            meter.SetFillTarget(20);

            Assert.AreEqual(20, meter.FillTarget);
            Assert.AreEqual(20, meter.Fill);
            Assert.IsTrue(meter.IsFull);
            Assert.AreEqual(1, filled);
        }

        [Test]
        public void RushMeter_LowerTarget_FillBelowStays()
        {
            var meter = new RushMeter(25, 5f);
            meter.RegisterKill(10);

            meter.SetFillTarget(20);
            meter.RegisterKill(9);
            Assert.IsFalse(meter.IsFull);
            meter.RegisterKill(1);

            Assert.IsTrue(meter.IsFull);
            Assert.AreEqual(1f, meter.FillProgress);
        }

        [Test]
        public void RushMeter_AlreadyFull_DoesNotFillAgain()
        {
            var meter = new RushMeter(25, 5f);
            meter.RegisterKill(25);
            int filled = 0;
            meter.Filled += () => filled++;

            meter.SetFillTarget(20);

            Assert.AreEqual(0, filled);
            Assert.IsTrue(meter.IsFull);
        }

        [Test]
        public void RushMeter_TargetBackUp_KeepsTheFill()
        {
            var meter = new RushMeter(20, 5f);
            meter.RegisterKill(15);

            meter.SetFillTarget(25);

            Assert.AreEqual(25, meter.FillTarget);
            Assert.AreEqual(15, meter.Fill);
        }

        [Test]
        public void EndlessRun_HigherMaximum_HeartsAreKept()
        {
            var config = ScriptableObject.CreateInstance<EndlessConfig>();
            try
            {
                var run = new EndlessRun(config, 5, 5);
                run.Start();

                run.NotifyMaxHearts(6, 6);

                Assert.AreEqual(6, run.MaxHearts);
                Assert.AreEqual(6, run.Hearts);
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void MaxExtraHearts_IsValueTimesMaxLevel()
        {
            var catalog = new UpgradeTestCatalog();
            try
            {
                Assert.AreEqual(3, RunUpgrades.MaxExtraHearts(catalog.All));
                Assert.AreEqual(0, RunUpgrades.MaxExtraHearts(null));
            }
            finally
            {
                catalog.Destroy();
            }
        }
    }
}
