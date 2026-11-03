using NUnit.Framework;
using RushOps.Combat;

namespace RushOps.Tests.EditMode
{
    public class WeaponMathTests
    {
        [Test]
        public void ShotDamage_WithoutModifier_IsBaseDamage()
        {
            Assert.AreEqual(3, WeaponMath.ShotDamage(3, ShotModifiers.None.DamageMultiplier));
        }

        [Test]
        public void ShotDamage_DoubleDamage_Doubles()
        {
            Assert.AreEqual(2, WeaponMath.ShotDamage(1, 2f));
            Assert.AreEqual(6, WeaponMath.ShotDamage(3, 2f));
        }

        [Test]
        public void ShotDamage_SmallMultiplier_StillDoesOne()
        {
            Assert.AreEqual(1, WeaponMath.ShotDamage(1, 0.2f));
        }

        [Test]
        public void ShotDamage_NoBaseDamageOrMultiplier_IsZero()
        {
            Assert.AreEqual(0, WeaponMath.ShotDamage(0, 2f));
            Assert.AreEqual(0, WeaponMath.ShotDamage(2, 0f));
        }

        [Test]
        public void FireRate_RapidFire_Doubles()
        {
            Assert.AreEqual(12f, WeaponMath.FireRate(6f, 2f), 1e-5f);
            Assert.AreEqual(6f, WeaponMath.FireRate(6f, 1f), 1e-5f);
            Assert.AreEqual(0f, WeaponMath.FireRate(6f, -1f), 1e-5f);
        }

        [Test]
        public void FireRate_RapidFire_FiresTwiceAsManyShots()
        {
            var normal = new FireRateTimer();
            var rapid = new FireRateTimer();
            int normalShots = 0;
            int rapidShots = 0;
            for (int frame = 0; frame < 60; frame++)
            {
                normalShots += normal.Tick(1f / 60f, true, WeaponMath.FireRate(6f, 1f));
                rapidShots += rapid.Tick(1f / 60f, true, WeaponMath.FireRate(6f, 2f));
            }

            Assert.AreEqual(6, normalShots);
            Assert.AreEqual(12, rapidShots);
        }

        [Test]
        public void HitDamage_NormalHit_IsUnchanged()
        {
            Assert.AreEqual(2, WeaponMath.HitDamage(2, false, false, 10));
        }

        [Test]
        public void HitDamage_InstaKill_TakesAllHealth()
        {
            Assert.AreEqual(10, WeaponMath.HitDamage(1, true, false, 10));
        }

        [Test]
        public void HitDamage_InstaKill_KeepsHigherDamage()
        {
            Assert.AreEqual(4, WeaponMath.HitDamage(4, true, false, 3));
        }

        [Test]
        public void HitDamage_InstaKillOnBoss_DoesNormalDamage()
        {
            Assert.AreEqual(1, WeaponMath.HitDamage(1, true, true, 50));
        }

        [Test]
        public void HitDamage_DoubleDamageOnBoss_Counts()
        {
            Assert.AreEqual(2, WeaponMath.HitDamage(WeaponMath.ShotDamage(1, 2f), false, true, 50));
        }

        [Test]
        public void HitDamage_ZeroDamage_StaysZeroEvenWithInstaKill()
        {
            Assert.AreEqual(0, WeaponMath.HitDamage(0, true, false, 10));
        }

        [Test]
        public void Look_PicksSpriteForModifiers()
        {
            Assert.AreEqual(ShotLook.Normal, WeaponMath.Look(ShotModifiers.None));
            Assert.AreEqual(ShotLook.Normal, WeaponMath.Look(new ShotModifiers(1f, 2f, false)));
            Assert.AreEqual(ShotLook.DoubleDamage, WeaponMath.Look(new ShotModifiers(2f, 2f, false)));
            Assert.AreEqual(ShotLook.InstaKill, WeaponMath.Look(new ShotModifiers(2f, 1f, true)));
        }
    }
}
