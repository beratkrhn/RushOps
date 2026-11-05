using NUnit.Framework;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Spread, projectiles per shot, range and damage per second of weapons (issue #173).</summary>
    public class WeaponShotTests
    {
        private const float Tolerance = 1e-4f;

        private static float SignedAngle(Vector2 from, Vector2 to)
        {
            return Vector2.SignedAngle(from, to);
        }

        [Test]
        public void ProjectileDirection_NoFanNoSpread_IsAim()
        {
            Vector2 direction = WeaponMath.ProjectileDirection(new Vector2(3f, 0f), 0, 1, 0f, 0f, 0.9f);

            Assert.AreEqual(1f, direction.x, Tolerance);
            Assert.AreEqual(0f, direction.y, Tolerance);
        }

        [Test]
        public void ProjectileDirection_Fan_SpreadsProjectilesEvenly()
        {
            const int count = 5;
            float[] expected = { -20f, -10f, 0f, 10f, 20f };
            for (int i = 0; i < count; i++)
            {
                Vector2 direction = WeaponMath.ProjectileDirection(Vector2.right, i, count, 40f, 0f, 0.5f);

                Assert.AreEqual(expected[i], SignedAngle(Vector2.right, direction), 1e-3f, $"projectile {i}");
                Assert.AreEqual(1f, direction.magnitude, Tolerance);
            }
        }

        [Test]
        public void ProjectileDirection_Fan_IsCentredOnAim()
        {
            Vector2 aim = new Vector2(1f, 1f).normalized;
            Vector2 first = WeaponMath.ProjectileDirection(aim, 0, 3, 30f, 0f, 0.5f);
            Vector2 middle = WeaponMath.ProjectileDirection(aim, 1, 3, 30f, 0f, 0.5f);
            Vector2 last = WeaponMath.ProjectileDirection(aim, 2, 3, 30f, 0f, 0.5f);

            Assert.AreEqual(0f, SignedAngle(aim, middle), 1e-3f);
            Assert.AreEqual(-SignedAngle(aim, first), SignedAngle(aim, last), 1e-3f);
        }

        [Test]
        public void ProjectileDirection_Spread_StaysWithinLimits()
        {
            Assert.AreEqual(-8f, SignedAngle(Vector2.up, WeaponMath.ProjectileDirection(Vector2.up, 0, 1, 0f, 8f, 0f)), 1e-3f);
            Assert.AreEqual(0f, SignedAngle(Vector2.up, WeaponMath.ProjectileDirection(Vector2.up, 0, 1, 0f, 8f, 0.5f)), 1e-3f);
            Assert.AreEqual(8f, SignedAngle(Vector2.up, WeaponMath.ProjectileDirection(Vector2.up, 0, 1, 0f, 8f, 1f)), 1e-3f);

            for (float random = 0f; random < 1f; random += 0.05f)
            {
                float angle = SignedAngle(Vector2.up, WeaponMath.ProjectileDirection(Vector2.up, 0, 1, 0f, 8f, random));
                Assert.That(angle, Is.InRange(-8f - 1e-3f, 8f + 1e-3f));
            }
        }

        [Test]
        public void ProjectileDirection_FanAndSpread_Add()
        {
            Vector2 direction = WeaponMath.ProjectileDirection(Vector2.right, 2, 3, 20f, 4f, 1f);

            Assert.AreEqual(14f, SignedAngle(Vector2.right, direction), 1e-3f);
        }

        [Test]
        public void ProjectileDirection_NoAim_IsZero()
        {
            Assert.AreEqual(Vector2.zero, WeaponMath.ProjectileDirection(Vector2.zero, 0, 3, 20f, 4f, 0.2f));
        }

        [Test]
        public void ProjectilesPerShot_IsAtLeastOne()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            JsonUtility.FromJsonOverwrite("{\"_projectilesPerShot\":0}", weapon);

            Assert.AreEqual(1, weapon.ProjectilesPerShot);
            Object.DestroyImmediate(weapon);
        }

        [Test]
        public void AimAssistRange_IsShareOfWeaponRange()
        {
            Assert.AreEqual(8f, WeaponMath.AimAssistRange(10f, 0.8f), Tolerance);
            Assert.AreEqual(4f, WeaponMath.AimAssistRange(5f, 0.8f), Tolerance, "short range, short assist");
            Assert.AreEqual(10f, WeaponMath.AimAssistRange(10f, 2f), Tolerance, "never beyond the weapon range");
            Assert.AreEqual(0f, WeaponMath.AimAssistRange(-1f, 0.8f), Tolerance);
        }

        [Test]
        public void DamagePerSecond_CountsEveryProjectile()
        {
            Assert.AreEqual(6f, WeaponMath.DamagePerSecond(6f, 1, 1), Tolerance);
            Assert.AreEqual(7.5f, WeaponMath.DamagePerSecond(1.5f, 1, 5), Tolerance);
            Assert.AreEqual(0f, WeaponMath.DamagePerSecond(-2f, 1, 1), Tolerance);
        }

        [Test]
        public void DamagePerSecond_OfDefinition_UsesItsValues()
        {
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            JsonUtility.FromJsonOverwrite("{\"_fireRate\":2,\"_damage\":3,\"_projectilesPerShot\":2}", weapon);

            Assert.AreEqual(12f, weapon.DamagePerSecond, Tolerance);
            Object.DestroyImmediate(weapon);
        }
    }
}
