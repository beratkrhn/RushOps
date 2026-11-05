using NUnit.Framework;
using RushOps.Combat;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    /// <summary>Checks the weapon assets under Data/Weapons (issue #173).</summary>
    public class WeaponAssetTests
    {
        private const string ConfigPath = "Assets/_Project/Data/Weapons/WeaponConfig.asset";

        // W7: every weapon deals about the damage per second of the start weapon.
        private const float DamagePerSecondTolerance = 0.15f;

        private static WeaponConfig LoadConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<WeaponConfig>(ConfigPath);
            Assert.IsNotNull(config, $"missing {ConfigPath}");
            return config;
        }

        [Test]
        public void StartWeapon_KeepsTheOldPistolValues()
        {
            WeaponDefinition pistol = LoadConfig().StartWeapon;

            Assert.IsNotNull(pistol, "WeaponConfig has no start weapon");
            Assert.AreEqual(6f, pistol.FireRate);
            Assert.AreEqual(1, pistol.Damage);
            Assert.AreEqual(1, pistol.ProjectilesPerShot);
            Assert.AreEqual(0f, pistol.Spread);
            Assert.AreEqual(10f, pistol.Range);
            Assert.AreEqual(14f, pistol.ProjectileSpeed);
            Assert.IsNotNull(pistol.ShotSound);
            Assert.AreEqual(8f, WeaponMath.AimAssistRange(pistol.Range, LoadConfig().AimAssistRangeFactor), 1e-4f,
                "aim assist range stays as before");
        }

        [Test]
        public void AllWeapons_DealAboutTheDamagePerSecondOfTheStartWeapon()
        {
            float reference = LoadConfig().StartWeapon.DamagePerSecond;
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(WeaponDefinition), new[] { "Assets/_Project/Data" });
            Assert.IsNotEmpty(guids);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
                Assert.That(weapon.DamagePerSecond,
                    Is.InRange(reference * (1f - DamagePerSecondTolerance), reference * (1f + DamagePerSecondTolerance)),
                    $"{path}: {weapon.DamagePerSecond:0.##} damage per second, start weapon {reference:0.##}");
                Assert.IsFalse(string.IsNullOrEmpty(weapon.DisplayName), $"{path} has no name");
            }
        }
    }
}
