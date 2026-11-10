using NUnit.Framework;
using RushOps.Combat;
using RushOps.Rush;

namespace RushOps.Tests.EditMode
{
    /// <summary>Effects of a running rush (issue #269, F-RSH-03 to F-RSH-05, RM2, RM6, N8, N17).</summary>
    public class RushRulesTests
    {
        [Test]
        public void EnemySpeed_NormalOutsideRush()
        {
            Assert.AreEqual(1f, RushRules.EnemySpeedFactor(false, false, 0.5f, 0.75f));
            Assert.AreEqual(1f, RushRules.EnemySpeedFactor(false, true, 0.5f, 0.75f));
        }

        [Test]
        public void EnemySpeed_HalfDuringRush()
        {
            Assert.AreEqual(0.5f, RushRules.EnemySpeedFactor(true, false, 0.5f, 0.75f), 1e-5f);
        }

        [Test]
        public void BossSpeed_WeakerSlowDuringRush()
        {
            Assert.AreEqual(0.75f, RushRules.EnemySpeedFactor(true, true, 0.5f, 0.75f), 1e-5f);
        }

        [Test]
        public void EnemySpeed_WithoutConfig_Normal()
        {
            Assert.AreEqual(1f, RushRules.EnemySpeedFactor(true, false, null));
        }

        [Test]
        public void DashCooldown_ZeroDuringRush()
        {
            Assert.AreEqual(0f, RushRules.DashCooldown(true, 1f));
            Assert.AreEqual(1f, RushRules.DashCooldown(false, 1f));
        }

        [Test]
        public void Damage_OneAndAHalfDuringRush()
        {
            Assert.AreEqual(1.5f, RushRules.DamageMultiplier(true, 1.5f), 1e-5f);
            Assert.AreEqual(1f, RushRules.DamageMultiplier(false, 1.5f), 1e-5f);
        }

        [Test]
        public void Damage_DoubleDamagePlusRush_IsThree()
        {
            Assert.AreEqual(3f, RushRules.DamageMultiplier(true, 1.5f, 2f), 1e-5f);
            Assert.AreEqual(2f, RushRules.DamageMultiplier(false, 1.5f, 2f), 1e-5f);
        }

        [Test]
        public void Apply_ScalesDamage_KeepsFireRateAndInstaKill()
        {
            var perks = new ShotModifiers(2f, 2f, true);

            ShotModifiers rush = RushRules.Apply(perks, true, 1.5f);

            Assert.AreEqual(3f, rush.DamageMultiplier, 1e-5f);
            Assert.AreEqual(2f, rush.FireRateMultiplier, 1e-5f);
            Assert.IsTrue(rush.InstaKill);
        }

        [Test]
        public void Apply_OutsideRush_Unchanged()
        {
            ShotModifiers mods = RushRules.Apply(ShotModifiers.None, false, 1.5f);

            Assert.AreEqual(1f, mods.DamageMultiplier);
            Assert.IsFalse(mods.InstaKill);
        }

        [Test]
        public void ShotDamage_WithRush_RoundsLikeOtherFactors()
        {
            // Pistol damage 10 → 15 during the rush, 30 with double damage.
            Assert.AreEqual(15, WeaponMath.ShotDamage(10, RushRules.DamageMultiplier(true, 1.5f)));
            Assert.AreEqual(30, WeaponMath.ShotDamage(10, RushRules.DamageMultiplier(true, 1.5f, 2f)));
        }
    }
}
