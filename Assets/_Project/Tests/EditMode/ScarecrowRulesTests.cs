using NUnit.Framework;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class ScarecrowRulesTests
    {
        [Test]
        public void Timer_ExplodesAfterTheDuration()
        {
            var rules = new ScarecrowRules();
            rules.Activate(8f);

            Assert.IsFalse(rules.Tick(7.9f));
            Assert.IsTrue(rules.IsActive);
            Assert.IsTrue(rules.Tick(0.1f));
            Assert.IsFalse(rules.IsActive);
        }

        [Test]
        public void Activate_Again_RestartsTheTimerAndStaysOne()
        {
            var rules = new ScarecrowRules();
            rules.Activate(8f);
            rules.Tick(6f);

            rules.Activate(8f);

            Assert.AreEqual(8f, rules.Remaining, 1e-4f);
        }

        [Test]
        public void Clear_RemovesItWithoutExplosion()
        {
            var rules = new ScarecrowRules();
            rules.Activate(8f);
            rules.Clear();

            Assert.IsFalse(rules.IsActive);
            Assert.IsFalse(rules.Tick(10f));
        }

        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, false)]
        public void Attracts_NormalLivingEnemiesOnly(bool alive, bool boss, bool expected)
        {
            var rules = new ScarecrowRules();
            rules.Activate(8f);
            Assert.AreEqual(expected, rules.Attracts(alive, boss));
        }

        [Test]
        public void Attracts_NothingWhenNoScarecrowStands()
        {
            Assert.IsFalse(new ScarecrowRules().Attracts(true, false));
        }

        [TestCase(0f, 0f, true)]
        [TestCase(2.9f, 0f, true)]
        [TestCase(3f, 0f, true)]
        [TestCase(3.01f, 0f, false)]
        [TestCase(3f, 4f, false)]
        public void Hits_RadiusEdgeIsIncluded(float x, float y, bool expected)
        {
            Assert.AreEqual(expected, ScarecrowRules.Hits(new Vector2(10f, 10f), 3f, new Vector2(10f + x, 10f + y), true));
        }

        [Test]
        public void Hits_DeadEnemyIsNotHit()
        {
            Assert.IsFalse(ScarecrowRules.Hits(Vector2.zero, 3f, Vector2.zero, false));
        }

        [Test]
        public void PlayerDamage_IsAlwaysZero()
        {
            Assert.AreEqual(0, ScarecrowRules.PlayerDamage);
        }
    }
}
