using System.Collections;
using NUnit.Framework;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class PlayerWeaponTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerMovement _movement;
        private PlayerAim _aim;
        private PlayerWeapon _weapon;
        private ProjectilePool _pool;
        private LayerMask _walls;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _weapon = Object.FindFirstObjectByType<PlayerWeapon>();
            Assert.IsNotNull(_weapon, "Arena_Test has no PlayerWeapon");
            Assert.IsNotNull(_weapon.Config, "PlayerWeapon has no WeaponConfig");
            _aim = _weapon.GetComponent<PlayerAim>();
            _movement = _weapon.GetComponent<PlayerMovement>();
            _pool = Object.FindFirstObjectByType<ProjectilePool>();
            Assert.IsNotNull(_pool, "Arena_Test has no ProjectilePool");
            _walls = LayerMask.GetMask("Walls");

            // Drive the player directly instead of through devices.
            _weapon.GetComponent<PlayerMoveInput>().enabled = false;
            _weapon.GetComponent<PlayerAimInput>().enabled = false;
        }

        [UnityTest]
        public IEnumerator Aim_BeyondDeadzone_FiresProjectiles()
        {
            _aim.AimInput = Vector2.right;
            yield return new WaitForSeconds(0.3f);

            Assert.Greater(_pool.CountActive, 0);
        }

        [UnityTest]
        public IEnumerator Aim_InsideDeadzone_DoesNotFire()
        {
            _aim.AimInput = new Vector2(_movement.Config.AimDeadzone * 0.5f, 0f);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(0, _pool.CountActive);
        }

        [UnityTest]
        public IEnumerator Aim_Released_AllProjectilesExpire()
        {
            _aim.AimInput = Vector2.up;
            yield return new WaitForSeconds(0.5f);

            _aim.AimInput = Vector2.zero;
            float lifetime = _weapon.Config.MaxRange / _weapon.Config.ProjectileSpeed;
            yield return new WaitForSeconds(lifetime + 0.2f);

            Assert.AreEqual(0, _pool.CountActive);
        }

        [UnityTest]
        public IEnumerator Aim_WhileMovingOtherWay_FacesAimDirection()
        {
            var view = _weapon.GetComponent<DirectionalSpriteAnimator>();
            _movement.MoveInput = Vector2.right;
            _aim.AimInput = Vector2.up;
            yield return null;
            yield return null;

            Assert.AreEqual(Direction8.N, view.Direction);
        }

        [UnityTest]
        public IEnumerator NotAiming_WhileMoving_FacesMoveDirection()
        {
            var view = _weapon.GetComponent<DirectionalSpriteAnimator>();
            _movement.MoveInput = Vector2.left;
            yield return null;
            yield return null;

            Assert.AreEqual(Direction8.W, view.Direction);
        }

        [UnityTest]
        public IEnumerator Projectile_HitsWall_ExpiresAtWall()
        {
            // The west wall occupies x 0..1; fire at it from open floor.
            Projectile projectile = _pool.Launch(new Vector2(4f, 30.5f), Vector2.left, 30f, 20f, _walls);
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(projectile.gameObject.activeSelf);
            Assert.GreaterOrEqual(projectile.transform.position.x, 1f - 1e-3f);
        }

        [UnityTest]
        public IEnumerator Projectile_OpenFloor_ExpiresAfterMaxRange()
        {
            // Column x = 20.5 is free between y 30 and 46.
            var origin = new Vector2(20.5f, 31f);
            Projectile projectile = _pool.Launch(origin, Vector2.up, 10f, 5f, _walls);

            yield return new WaitForSeconds(0.25f);
            Assert.IsTrue(projectile.gameObject.activeSelf, "Projectile expired before its range");

            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(projectile.gameObject.activeSelf, "Projectile flew past its range");
            Assert.AreEqual(origin.y + 5f, projectile.transform.position.y, 1e-3f);
        }

        [UnityTest]
        public IEnumerator Pool_AfterExpiry_ReusesProjectile()
        {
            Projectile first = _pool.Launch(new Vector2(20.5f, 31f), Vector2.up, 50f, 1f, _walls);
            yield return new WaitForSeconds(0.2f);

            Projectile second = _pool.Launch(new Vector2(20.5f, 31f), Vector2.up, 50f, 1f, _walls);

            Assert.AreSame(first, second);
        }
    }
}
