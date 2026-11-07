using NUnit.Framework;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class CompanionRulesTests
    {
        private static CompanionTarget Alive(float x, float y) => new CompanionTarget(new Vector2(x, y), true);

        [Test]
        public void FindTarget_NoEnemies_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, CompanionRules.FindTarget(Vector2.zero, new CompanionTarget[0], 5f));
        }

        [Test]
        public void FindTarget_NoEnemyInRange_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, CompanionRules.FindTarget(Vector2.zero, new[] { Alive(5.01f, 0f) }, 5f));
        }

        [Test]
        public void FindTarget_ExactlyOnRange_IsTargeted()
        {
            Assert.AreEqual(0, CompanionRules.FindTarget(Vector2.zero, new[] { Alive(3f, 4f) }, 5f));
        }

        [Test]
        public void FindTarget_PicksTheNearest()
        {
            var list = new[] { Alive(4f, 0f), Alive(2f, 0f), Alive(3f, 0f) };
            Assert.AreEqual(1, CompanionRules.FindTarget(Vector2.zero, list, 5f));
        }

        [Test]
        public void FindTarget_TwoAtSameDistance_LowerIndexWins()
        {
            var list = new[] { Alive(0f, 3f), Alive(3f, 0f) };
            Assert.AreEqual(0, CompanionRules.FindTarget(Vector2.zero, list, 5f));
        }

        [Test]
        public void FindTarget_SkipsDeadEnemies()
        {
            var list = new[] { new CompanionTarget(new Vector2(1f, 0f), false), Alive(4f, 0f) };
            Assert.AreEqual(1, CompanionRules.FindTarget(Vector2.zero, list, 5f));
        }

        [Test]
        public void ShotsPerSecond_HalfOfTheWeaponDamagePerSecond()
        {
            // Pistol: 6 shots/s x 10 damage = 60 dps; half = 30 dps = 3 shots/s of 10.
            Assert.AreEqual(3f, CompanionRules.ShotsPerSecond(6f, 10, 1, 0.5f), 1e-4f);
        }

        [Test]
        public void ShotsPerSecond_CountsProjectilesPerShot()
        {
            // Shotgun: 1.5 shots/s x 5 projectiles x 8 damage = 60 dps; half = 30 dps = 3.75 shots/s of 8.
            Assert.AreEqual(3.75f, CompanionRules.ShotsPerSecond(1.5f, 8, 5, 0.5f), 1e-4f);
        }

        [TestCase(0f, 10, 0.5f)]
        [TestCase(6f, 0, 0.5f)]
        [TestCase(6f, 10, 0f)]
        public void ShotsPerSecond_NothingToShoot_IsZero(float rate, int damage, float share)
        {
            Assert.AreEqual(0f, CompanionRules.ShotsPerSecond(rate, damage, 1, share));
        }

        [Test]
        public void Timer_RunsOutAfterTheDuration()
        {
            var rules = new CompanionRules();
            rules.Activate(15f);
            Assert.IsTrue(rules.IsActive);

            Assert.IsFalse(rules.Tick(14.5f));
            Assert.IsTrue(rules.IsActive);
            Assert.IsTrue(rules.Tick(0.5f));
            Assert.IsFalse(rules.IsActive);
        }

        [Test]
        public void Activate_Again_RestartsTheTimer()
        {
            var rules = new CompanionRules();
            rules.Activate(15f);
            rules.Tick(10f);

            rules.Activate(15f);

            Assert.AreEqual(15f, rules.Remaining, 1e-4f);
            Assert.IsFalse(rules.Tick(14f));
            Assert.IsTrue(rules.IsActive);
        }

        [Test]
        public void ShotsDue_AccumulatesAcrossFrames()
        {
            var rules = new CompanionRules();
            rules.Activate(15f);
            int total = 0;
            for (int i = 0; i < 60; i++)
            {
                total += rules.ShotsDue(1f / 60f, 3f);
            }

            Assert.AreEqual(3, total);
        }

        [Test]
        public void ShotsDue_NotActive_IsZero()
        {
            Assert.AreEqual(0, new CompanionRules().ShotsDue(1f, 3f));
        }

        [Test]
        public void Clear_EndsTheCompanion()
        {
            var rules = new CompanionRules();
            rules.Activate(15f);
            rules.Clear();
            Assert.IsFalse(rules.IsActive);
        }

        [Test]
        public void Follow_MovesTowardsTargetWithoutOvershooting()
        {
            Vector2 position = CompanionRules.Follow(Vector2.zero, new Vector2(4f, 0f), 6f, 0.1f);
            Assert.Greater(position.x, 0f);
            Assert.Less(position.x, 4f);
        }

        [Test]
        public void Follow_ALongStepNeverPassesTheTarget()
        {
            Vector2 position = CompanionRules.Follow(Vector2.zero, new Vector2(4f, 0f), 6f, 100f);
            Assert.AreEqual(4f, position.x, 0.001f);
        }

        [Test]
        public void Follow_WithoutSpeedOrTime_StaysPut()
        {
            Assert.AreEqual(Vector2.one, CompanionRules.Follow(Vector2.one, new Vector2(4f, 0f), 0f, 0.1f));
            Assert.AreEqual(Vector2.one, CompanionRules.Follow(Vector2.one, new Vector2(4f, 0f), 6f, 0f));
        }
    }
}
