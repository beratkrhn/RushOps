using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class PerkEffectTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerPerks _perks;
        private PlayerMovement _movement;
        private PlayerHealth _health;
        private PlayerWeapon _weapon;
        private Projectile _shot;
        private PerkPickupPool _pickups;
        private ActivePerksView _view;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _perks = Object.FindFirstObjectByType<PlayerPerks>();
            _pickups = Object.FindFirstObjectByType<PerkPickupPool>();
            _view = Object.FindFirstObjectByType<ActivePerksView>();
            Assert.IsNotNull(_perks, "Arena_Test has no PlayerPerks");
            Assert.IsNotNull(_view, "Arena_Test has no ActivePerksView");
            _movement = _perks.GetComponent<PlayerMovement>();
            _health = _perks.GetComponent<PlayerHealth>();
            _weapon = _perks.GetComponent<PlayerWeapon>();
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _movement.GetComponent<PlayerMoveInput>().enabled = false;
            _movement.GetComponent<PlayerAimInput>().enabled = false;
        }

        [Test]
        public void PerkDefinitions_HaveEffectStartValuesOfLastenheft()
        {
            PerkDefinition speed = Perk("speed_boost");
            Assert.AreEqual(PerkEffect.SpeedBoost, speed.Effect);
            Assert.AreEqual(1.5f, speed.Magnitude, 1e-5f);
            Assert.AreEqual(15f, speed.Duration, 1e-5f);

            PerkDefinition medkit = Perk("medkit");
            Assert.AreEqual(PerkEffect.Medkit, medkit.Effect);
            Assert.AreEqual(PerkKind.Instant, medkit.Kind);
            Assert.AreEqual(2f, medkit.Magnitude, 1e-5f);
        }

        [UnityTest]
        public IEnumerator CollectedPickup_AppliesPerk()
        {
            PerkPickup pickup = _pickups.Spawn(new Vector2(34.5f, 18.5f), Perk("speed_boost"));

            pickup.Collect();
            yield return null;

            Assert.IsTrue(_perks.Active.IsActive("speed_boost"));
        }

        [UnityTest]
        public IEnumerator SpeedBoost_RaisesWalkingSpeed_UntilExpiry()
        {
            float normal = _movement.Config.MoveSpeed;

            _perks.Apply(Perk("speed_boost"));
            _movement.MoveInput = Vector2.up;
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(normal * 1.5f, _movement.Velocity.magnitude, 0.05f);

            _perks.Active.Tick(15f);
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(1f, _movement.SpeedMultiplier, 1e-5f);
            Assert.AreEqual(normal, _movement.Velocity.magnitude, 0.05f);
        }

        [Test]
        public void Medkit_RestoresTwoHearts()
        {
            _health.TakeDamage(3);
            int hurt = _health.Current;

            _perks.Apply(Perk("medkit"));

            Assert.AreEqual(hurt + 2, _health.Current);
            Assert.AreEqual(0, _perks.Active.Count, "medkit acts at once and shows no timer");
        }

        [Test]
        public void Medkit_DoesNotHealAboveMaximum()
        {
            _health.TakeDamage(1);

            _perks.Apply(Perk("medkit"));

            Assert.AreEqual(_health.Max, _health.Current);
        }

        [UnityTest]
        public IEnumerator Hud_ShowsRunningPerksWithShrinkingTimer()
        {
            Assert.AreEqual(0, _view.ShownCount);

            _perks.Apply(Perk("speed_boost"));
            _perks.Apply(Perk("double_damage"));
            yield return null;

            Assert.AreEqual(2, _view.ShownCount);
            Assert.AreSame(Perk("speed_boost").Icon, _view.Icon(0).sprite);
            Assert.AreSame(Perk("double_damage").Icon, _view.Icon(1).sprite);
            Assert.IsTrue(_view.Icon(0).gameObject.activeInHierarchy);

            _perks.Active.Tick(7.5f);
            yield return null;

            Assert.AreEqual(0.5f, _view.Timer(0).fillAmount, 0.02f);
            Assert.AreEqual(0.25f, _view.Timer(1).fillAmount, 0.02f);

            _perks.Active.Tick(2.5f);
            yield return null;

            Assert.AreEqual(1, _view.ShownCount, "expired perk leaves the HUD");
            Assert.AreSame(Perk("speed_boost").Icon, _view.Icon(0).sprite);
            Assert.IsFalse(_view.Icon(1).gameObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator Hud_RefreshedPerk_FillsTimerAgain()
        {
            _perks.Apply(Perk("speed_boost"));
            _perks.Active.Tick(10f);
            yield return null;
            Assert.Less(_view.Timer(0).fillAmount, 0.4f);

            _perks.Apply(Perk("speed_boost"));
            yield return null;

            Assert.AreEqual(1, _view.ShownCount);
            Assert.Greater(_view.Timer(0).fillAmount, 0.99f);
        }

        [Test]
        public void OffensivePerkDefinitions_HaveStartValuesOfBacklog()
        {
            PerkDefinition doubleDamage = Perk("double_damage");
            Assert.AreEqual(PerkEffect.DoubleDamage, doubleDamage.Effect);
            Assert.AreEqual(2f, doubleDamage.Magnitude, 1e-5f);
            Assert.AreEqual(10f, doubleDamage.Duration, 1e-5f);

            PerkDefinition rapidFire = Perk("rapid_fire");
            Assert.AreEqual(PerkEffect.RapidFire, rapidFire.Effect);
            Assert.AreEqual(2f, rapidFire.Magnitude, 1e-5f);
            Assert.AreEqual(10f, rapidFire.Duration, 1e-5f);

            PerkDefinition instaKill = Perk("insta_kill");
            Assert.AreEqual(PerkEffect.InstaKill, instaKill.Effect);
            Assert.AreEqual(10f, instaKill.Duration, 1e-5f);
        }

        [Test]
        public void DoubleDamageAndRapidFire_Combine_UntilExpiry()
        {
            int damage = _weapon.Damage;
            float rate = _weapon.FireRate;

            _perks.Apply(Perk("double_damage"));
            _perks.Apply(Perk("rapid_fire"));

            Assert.AreEqual(damage * 2, _weapon.Damage);
            Assert.AreEqual(rate * 2f, _weapon.FireRate, 1e-4f);

            _perks.Active.Tick(10f);

            Assert.AreEqual(damage, _weapon.Damage);
            Assert.AreEqual(rate, _weapon.FireRate, 1e-4f);
        }

        [UnityTest]
        public IEnumerator DoubleDamage_ShotsAreRedAndHitHarder()
        {
            yield return FireOneShot();
            Sprite normal = _shot.Sprite;
            Assert.AreEqual(_weapon.Config.Damage, _shot.Damage);

            _perks.Apply(Perk("double_damage"));
            yield return FireOneShot();

            Assert.AreNotSame(normal, _shot.Sprite, "double damage must change the projectile look");
            Assert.AreEqual(_weapon.Config.Damage * 2, _shot.Damage);
            Assert.IsFalse(_shot.Lethal);
        }

        [UnityTest]
        public IEnumerator InstaKill_ShotsAreLethal_UntilExpiry()
        {
            yield return FireOneShot();
            Sprite normal = _shot.Sprite;
            Assert.IsFalse(_shot.Lethal);

            _perks.Apply(Perk("insta_kill"));
            yield return FireOneShot();
            Assert.IsTrue(_shot.Lethal);
            Assert.AreNotSame(normal, _shot.Sprite, "insta-kill must change the projectile look");

            _perks.Active.Tick(10f);
            yield return FireOneShot();
            Assert.IsFalse(_shot.Lethal);
            Assert.AreSame(normal, _shot.Sprite);
        }

        [UnityTest]
        public IEnumerator LethalHit_KillsBruteAtOnce()
        {
            Enemy brute = EnemyPools.Of(EnemyPools.Brute).Spawn(new Vector2(5.5f, 5.5f));
            yield return null;
            Assert.Greater(brute.CurrentHealth, 1);

            Assert.IsTrue(brute.TakeHit(1, Vector2.right, true));

            Assert.IsTrue(brute.IsDead);
        }

        // Lets the weapon fire for one frame (the first shot after a pause leaves at once) and stores the new
        // projectile in _shot. Waits for the fire cooldown first, so the frame fires exactly one shot.
        private IEnumerator FireOneShot()
        {
            yield return new WaitForSeconds(0.3f);
            var before = new HashSet<Projectile>(Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None));

            _weapon.GetComponent<PlayerAim>().AimInput = Vector2.up;
            yield return null;
            _weapon.GetComponent<PlayerAim>().AimInput = Vector2.zero;

            _shot = null;
            foreach (Projectile projectile in Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            {
                if (!before.Contains(projectile))
                {
                    Assert.IsNull(_shot, "aiming for one frame fired more than one shot");
                    _shot = projectile;
                }
            }

            Assert.IsNotNull(_shot, "aiming for one frame fired no shot");
        }

        private PerkDefinition Perk(string id)
        {
            foreach (PerkDefinition perk in _pickups.Config.Perks)
            {
                if (perk.Id == id)
                {
                    return perk;
                }
            }

            Assert.Fail($"LootConfig has no perk {id}");
            return null;
        }
    }
}
