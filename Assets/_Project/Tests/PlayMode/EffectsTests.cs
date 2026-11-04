using System.Collections;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Feel;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Combat effects and accent lights (issue #107).</summary>
    public class EffectsTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerAim _aim;
        private MuzzleFlash _muzzle;
        private SpriteEffectPool _sparks;
        private SpriteEffectPool _deaths;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            var weapon = Object.FindFirstObjectByType<PlayerWeapon>();
            weapon.GetComponent<PlayerMoveInput>().enabled = false;
            weapon.GetComponent<PlayerAimInput>().enabled = false;
            _aim = weapon.GetComponent<PlayerAim>();
            _muzzle = weapon.GetComponentInChildren<MuzzleFlash>();
            _sparks = Pool("HitSparkPool");
            _deaths = Pool("DeathSplatPool");
            Assert.IsNotNull(_muzzle, "player has no muzzle flash");
        }

        [UnityTest]
        public IEnumerator Firing_ShowsMuzzleFlashWithLight()
        {
            Assert.IsFalse(_muzzle.IsShowing);
            bool seen = false;
            bool lit = false;
            _aim.AimInput = Vector2.right;
            float timeout = Time.time + 0.5f;
            while (Time.time < timeout && !seen)
            {
                yield return null;
                seen = _muzzle.IsShowing;
                lit = _muzzle.GetComponentInChildren<Light2D>().enabled;
            }

            Assert.IsTrue(seen, "no muzzle flash while firing");
            Assert.IsTrue(lit, "muzzle flash has no light");
        }

        [UnityTest]
        public IEnumerator ShotHittingWall_ShowsSparks()
        {
            ProjectilePool projectiles = Object.FindFirstObjectByType<ProjectilePool>();
            projectiles.Launch(new Vector2(4f, 30.5f), Vector2.left, 30f, 20f, LayerMask.GetMask("Walls"));
            float timeout = Time.time + 0.5f;
            while (Time.time < timeout && _sparks.CountActive == 0)
            {
                yield return null;
            }

            Assert.Greater(_sparks.CountActive, 0, "no sparks at the wall");
        }

        [UnityTest]
        public IEnumerator EnemyDeath_ShowsSplatter_BoomerDoesNot()
        {
            Enemy walker = EnemyPools.Of(EnemyPools.Walker).Spawn(new Vector2(5.5f, 5.5f));
            yield return null;
            walker.Kill();
            yield return null;
            Assert.AreEqual(1, _deaths.CountActive, "walker death shows no splatter");

            Enemy boomer = EnemyPools.Of(EnemyPools.Boomer).Spawn(new Vector2(34.5f, 18.5f));
            yield return null;
            boomer.Kill();
            yield return null;
            Assert.LessOrEqual(_deaths.CountActive, 1, "boomer shows its blast, not a splatter");
        }

        [Test]
        public void PerkPickupPrefab_HasAccentLight()
        {
            PerkPickupPool perks = Object.FindFirstObjectByType<PerkPickupPool>();
            Assert.IsNotNull(perks);
            var prefabLight = Resources.FindObjectsOfTypeAll<PerkPickup>();
            bool found = false;
            foreach (PerkPickup pickup in prefabLight)
            {
                found |= pickup.GetComponentInChildren<Light2D>(true) != null;
            }

            Assert.IsTrue(found, "perk pickups have no light");
        }

        private static SpriteEffectPool Pool(string name)
        {
            GameObject go = GameObject.Find(name);
            Assert.IsNotNull(go, $"{SceneName} has no {name}");
            return go.GetComponent<SpriteEffectPool>();
        }
    }
}
