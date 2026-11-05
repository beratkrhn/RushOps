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
            Assert.AreEqual(10, pistol.Damage, "ADR 0012: damage in tenths of the old unit");
            Assert.AreEqual(1, pistol.ProjectilesPerShot);
            Assert.AreEqual(0f, pistol.Spread);
            Assert.AreEqual(10f, pistol.Range);
            Assert.AreEqual(14f, pistol.ProjectileSpeed);
            Assert.IsNotNull(pistol.ShotSound);
            Assert.AreEqual(8f, WeaponMath.AimAssistRange(pistol.Range, LoadConfig().AimAssistRangeFactor), 1e-4f,
                "aim assist range stays as before");
        }

        private static WeaponDefinition Load(string file)
        {
            var weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/_Project/Data/Weapons/" + file);
            Assert.IsNotNull(weapon, $"missing weapon {file}");
            return weapon;
        }

        [Test]
        public void AssaultRifle_IsFasterThanThePistol_WithSomeSpread()
        {
            WeaponDefinition pistol = LoadConfig().StartWeapon;
            WeaponDefinition rifle = Load("AssaultRifle.asset");

            Assert.AreEqual("Sturmgewehr", rifle.DisplayName);
            Assert.Greater(rifle.FireRate, pistol.FireRate, "W2: faster");
            Assert.Greater(rifle.Spread, 0f, "W2: some spread");
            Assert.AreEqual(1, rifle.ProjectilesPerShot);
        }

        [Test]
        public void Shotgun_IsSlow_WithFivePelletsInAFan_AndShortRange()
        {
            WeaponDefinition pistol = LoadConfig().StartWeapon;
            WeaponDefinition shotgun = Load("Shotgun.asset");

            Assert.AreEqual("Schrotflinte", shotgun.DisplayName);
            Assert.Less(shotgun.FireRate, pistol.FireRate, "W2: slow");
            Assert.AreEqual(5, shotgun.ProjectilesPerShot, "W2: 5 pellets");
            Assert.Greater(shotgun.FanAngle, 0f, "W2: in a fan");
            Assert.Less(shotgun.Range, pistol.Range, "W2: short range");
        }

        [Test]
        public void DualPistols_AreVeryFast_WithLittleDamage_AlternatingSides()
        {
            WeaponDefinition pistol = LoadConfig().StartWeapon;
            WeaponDefinition dual = Load("DualPistols.asset");

            Assert.AreEqual("Dual-Pistolen", dual.DisplayName);
            Assert.Greater(dual.FireRate, Load("AssaultRifle.asset").FireRate, "W2: very fast");
            Assert.Less(dual.Damage, pistol.Damage, "W2: little damage");
            Assert.IsTrue(dual.AlternateMuzzles, "W2: left and right in turn");
            Assert.Greater(dual.MuzzleSeparation, 0f);
        }

        [Test]
        public void EveryWeapon_HasItsOwnIcon_Sound_AndNewOnesTheirOwnBullet()
        {
            string[] files = { "Pistol.asset", "AssaultRifle.asset", "Shotgun.asset", "DualPistols.asset" };
            var sounds = new System.Collections.Generic.HashSet<UnityEngine.AudioClip>();
            var bullets = new System.Collections.Generic.HashSet<UnityEngine.Sprite>();
            foreach (string file in files)
            {
                WeaponDefinition weapon = Load(file);
                Assert.IsNotNull(weapon.Icon, $"{file} has no icon");
                Assert.IsNotNull(weapon.ShotSound, $"{file} has no shot sound");
                Assert.IsTrue(sounds.Add(weapon.ShotSound), $"{file} shares its shot sound");
                if (file != "Pistol.asset")
                {
                    Assert.IsNotNull(weapon.ProjectileSprite, $"{file} has no own bullet");
                    Assert.IsTrue(bullets.Add(weapon.ProjectileSprite), $"{file} shares its bullet");
                }
            }
        }

        [Test]
        public void Catalog_StartsWithThePistol_AndEachBossUnlocksOneWeapon()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<WeaponCatalog>("Assets/_Project/Data/Weapons/WeaponCatalog.asset");
            Assert.IsNotNull(catalog, "missing weapon catalog");
            Assert.AreEqual(4, catalog.Weapons.Count);
            Assert.AreSame(LoadConfig().StartWeapon, catalog.Weapons[0]);
            Assert.AreEqual(string.Empty, catalog.Weapons[0].UnlockLevel);

            // W4 as proposed: Koloss gives the shotgun, Brutmutter the assault rifle, Mega-Boomer the dual pistols.
            Assert.AreEqual("Level_1-3", Load("Shotgun.asset").UnlockLevel);
            Assert.AreEqual("Besiege den Koloss", Load("Shotgun.asset").UnlockHint);
            Assert.AreEqual("Level_2-3", Load("AssaultRifle.asset").UnlockLevel);
            Assert.AreEqual("Besiege die Brutmutter", Load("AssaultRifle.asset").UnlockHint);
            Assert.AreEqual("Level_3-3", Load("DualPistols.asset").UnlockLevel);
            Assert.AreEqual("Besiege den Mega-Boomer", Load("DualPistols.asset").UnlockHint);
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                Assert.IsTrue(ids.Add(weapon.Id), $"{weapon.name}: id {weapon.Id} twice");
            }
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
