using System.Collections;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class AimAssistTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerWeapon _weapon;
        private PlayerAim _aim;
        private AimAssist _assist;
        private EnemyPool _pool;
        private Vector2 _lastShot;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            Object.FindFirstObjectByType<EnemySpawner>().enabled = false;
            _pool = Object.FindFirstObjectByType<EnemyPool>();
            _weapon = Object.FindFirstObjectByType<PlayerWeapon>();
            _aim = _weapon.GetComponent<PlayerAim>();
            _assist = _weapon.GetComponent<AimAssist>();
            Assert.IsNotNull(_assist, "Player has no AimAssist");
            _weapon.GetComponent<PlayerMoveInput>().enabled = false;
            _weapon.GetComponent<PlayerAimInput>().enabled = false;
            _weapon.Fired += direction => _lastShot = direction;
        }

        [UnityTest]
        public IEnumerator EnemySlightlyOffAim_ShotGoesToEnemy()
        {
            Enemy walker = SpawnStill(new Vector2(0.6f, 4f));
            Vector2 expected = (walker.AimPoint - _weapon.MuzzlePosition(Vector2.up)).normalized;

            yield return FireUp();

            Assert.Greater(_lastShot.x, 0.05f, "corrected towards the enemy");
            Assert.AreEqual(expected.x, _lastShot.x, 0.05f);
        }

        [UnityTest]
        public IEnumerator EnemyFarOffAim_ShotStaysStraight()
        {
            SpawnStill(new Vector2(3f, 3f));

            yield return FireUp();

            Assert.AreEqual(Vector2.up, _lastShot);
        }

        [UnityTest]
        public IEnumerator Disabled_ShotStaysStraight()
        {
            SpawnStill(new Vector2(0.6f, 4f));
            _assist.Enabled = false;

            yield return FireUp();

            Assert.AreEqual(Vector2.up, _lastShot);
        }

        private Enemy SpawnStill(Vector2 offsetFromPlayer)
        {
            Enemy walker = _pool.Spawn((Vector2)_weapon.transform.position + offsetFromPlayer);
            walker.GetComponent<EnemyMovement>().Stop();
            return walker;
        }

        private IEnumerator FireUp()
        {
            _lastShot = Vector2.zero;
            _aim.AimInput = Vector2.up;
            float until = Time.time + 0.5f;
            while (Time.time < until && _lastShot == Vector2.zero)
            {
                yield return null;
            }

            _aim.AimInput = Vector2.zero;
            Assert.AreNotEqual(Vector2.zero, _lastShot, "weapon did not fire");
        }
    }
}
