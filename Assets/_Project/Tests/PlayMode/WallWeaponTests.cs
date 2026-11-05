using System.Collections;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class WallWeaponTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerWeapon _weapon;
        private PlayerAim _aim;
        private WallWeapon _wallWeapon;
        private ProjectilePool _pool;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _weapon = Object.FindFirstObjectByType<PlayerWeapon>();
            _aim = _weapon.GetComponent<PlayerAim>();
            _pool = Object.FindFirstObjectByType<ProjectilePool>();
            _wallWeapon = Object.FindFirstObjectByType<WallWeapon>();
            Assert.IsNotNull(_wallWeapon, "issue #175: the test arena has a wall weapon");
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            // Without a touchscreen the touch controls stay hidden; show them to check the swap button.
            Object.FindFirstObjectByType<TouchControlsVisibility>().GetComponentInChildren<SafeAreaFitter>(true)
                .gameObject.SetActive(true);
            _weapon.GetComponent<PlayerMoveInput>().enabled = false;
            _weapon.GetComponent<PlayerAimInput>().enabled = false;
        }

        [UnityTest]
        public IEnumerator StandingInFront_PicksItUp_SwapGoesBack_AndBothFire()
        {
            WeaponDefinition start = _weapon.ActiveWeapon;
            var swap = Object.FindFirstObjectByType<WeaponSwapButton>(FindObjectsInactive.Include);
            var hud = Object.FindFirstObjectByType<ActiveWeaponIcon>();
            Assert.IsNotNull(swap, "no swap button");
            Assert.IsNotNull(hud, "no HUD weapon icon");
            Assert.IsFalse(swap.IsShown, "W10: hidden with one weapon");
            Assert.AreSame(start.Icon, hud.IconShown);

            Teleport(_wallWeapon.PickupPoint);
            yield return new WaitForSeconds(0.25f);
            Assert.Greater(_wallWeapon.Progress, 0f, "the ring fills");
            Assert.AreSame(start, _weapon.ActiveWeapon, "not yet");
            yield return new WaitForSeconds(0.4f);

            Assert.AreSame(_wallWeapon.Weapon, _weapon.ActiveWeapon, "W8, W9: picked up into the second slot");
            Assert.AreSame(start, _weapon.Slots.Selected);
            Assert.IsTrue(swap.IsShown);
            Assert.AreSame(start.Icon, swap.IconShown, "W10: shows the other weapon");
            Assert.AreSame(_wallWeapon.Weapon.Icon, hud.IconShown);
            Assert.IsTrue(_wallWeapon.gameObject.activeSelf, "the silhouette stays on the wall");

            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(0f, _wallWeapon.Progress, "with the weapon in hand nothing happens");

            yield return Fire(_wallWeapon.Weapon.ProjectilesPerShot, "the picked-up weapon fires");

            swap.Swap();
            Assert.AreSame(start, _weapon.ActiveWeapon);
            Assert.AreSame(_wallWeapon.Weapon.Icon, swap.IconShown);
            Assert.AreSame(start.Icon, hud.IconShown);
            yield return new WaitForSeconds(_wallWeapon.Weapon.Range / _wallWeapon.Weapon.ProjectileSpeed + 0.2f);
            yield return Fire(1, "the start weapon fires again");
        }

        [UnityTest]
        public IEnumerator SteppingAway_StopsThePickup()
        {
            WeaponDefinition start = _weapon.ActiveWeapon;
            Teleport(_wallWeapon.PickupPoint);
            yield return new WaitForSeconds(0.3f);

            Teleport(_wallWeapon.PickupPoint + new Vector2(0f, -3f));
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(0f, _wallWeapon.Progress);
            Assert.AreSame(start, _weapon.ActiveWeapon);
        }

        private IEnumerator Fire(int expected, string message)
        {
            // Step away: right at the wall the muzzle (at hand height) lies inside it.
            Teleport(_wallWeapon.PickupPoint + new Vector2(0f, -3f));
            yield return null;
            // Aim away from the wall, into the open floor south of it.
            _aim.AimInput = Vector2.down;
            float waited = 0f;
            while (_pool.CountActive < expected && waited < 1.5f)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            _aim.AimInput = Vector2.zero;
            Assert.GreaterOrEqual(_pool.CountActive, expected, message);
        }

        private void Teleport(Vector2 position)
        {
            var body = _weapon.GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
        }
    }
}
