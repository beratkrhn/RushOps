using System.Collections;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Navigation;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class WalkerTests
    {
        private const string SceneName = "Arena_Test";

        private EnemyPool _pool;
        private FlowFieldNavigator _navigator;
        private PlayerHealth _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _pool = Object.FindFirstObjectByType<EnemyPool>();
            _navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            Assert.IsNotNull(_pool, "Arena_Test has no EnemyPool");
            Assert.IsNotNull(_navigator, "Arena_Test has no FlowFieldNavigator");
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
        }

        [Test]
        public void Start_TenWalkersActive()
        {
            Assert.AreEqual(10, _pool.CountActive);
        }

        [Test]
        public void Field_CoversArenaAndBlocksWalls()
        {
            FlowField field = _navigator.Field;
            Assert.IsNotNull(field);

            Assert.IsTrue(field.IsReachable(_navigator.CellOf(new Vector2(5.5f, 5.5f))));
            Assert.IsFalse(field.IsWalkable(_navigator.CellOf(new Vector2(0.5f, 0.5f))), "outer wall");
            Assert.IsFalse(field.IsWalkable(_navigator.CellOf(new Vector2(17.5f, 20.5f))), "obstacle");
        }

        [UnityTest]
        public IEnumerator Walker_BehindObstacle_ReachesPlayer()
        {
            // Player above the 8x2 obstacle at (16..23, 20..21), walker straight below it.
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(20.5f, 23.5f));
            KillAllExcept(null);
            yield return null;
            Enemy walker = _pool.Spawn(new Vector2(20.5f, 17.5f));
            float start = Vector2.Distance(walker.transform.position, _player.transform.position);

            float timeout = Time.time + 12f;
            while (Time.time < timeout && Vector2.Distance(walker.transform.position, _player.transform.position) > 1f)
            {
                yield return null;
            }

            Assert.Less(Vector2.Distance(walker.transform.position, _player.transform.position), 1f,
                $"walker stuck at {walker.transform.position}, started {start} away");
        }

        [UnityTest]
        public IEnumerator Walkers_SpawnedOnSameSpot_DoNotStack()
        {
            KillAllExcept(null);
            yield return null;
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(35.5f, 55.5f));
            Enemy first = _pool.Spawn(new Vector2(5.5f, 5.5f));
            Enemy second = _pool.Spawn(new Vector2(5.5f, 5.5f));

            yield return new WaitForSeconds(1f);

            Assert.Greater(Vector2.Distance(first.transform.position, second.transform.position), 0.4f);
        }

        [UnityTest]
        public IEnumerator Walker_Shot_DiesAndReturnsToPool()
        {
            Enemy walker = Object.FindFirstObjectByType<Enemy>();
            KillAllExcept(walker);
            yield return new WaitForSeconds(walker.Definition.DeathDuration + 0.1f);
            Assert.AreEqual(1, _pool.CountActive, "killed walkers returned to the pool");
            bool died = false;
            walker.Died += _ => died = true;

            for (int i = 0; i < walker.Definition.Health; i++)
            {
                Assert.IsTrue(((IDamageable)walker).TakeHit(1, Vector2.right));
            }

            Assert.IsTrue(died);
            Assert.IsTrue(walker.IsDead);
            Assert.AreEqual(0, walker.ContactDamage, "dying walkers do not hurt");
            yield return new WaitForSeconds(walker.Definition.DeathDuration + 0.1f);

            Assert.IsFalse(walker.gameObject.activeSelf);
            Assert.AreEqual(0, _pool.CountActive);
        }

        [UnityTest]
        public IEnumerator Projectile_HitsWalker()
        {
            Enemy walker = Object.FindFirstObjectByType<Enemy>();
            KillAllExcept(walker);
            yield return null;
            walker.GetComponent<EnemyMovement>().Stop();
            Vector2 target = walker.transform.position + new Vector3(0f, 0.3f, 0f);
            ProjectilePool projectiles = Object.FindFirstObjectByType<ProjectilePool>();
            int mask = LayerMask.GetMask("Walls", "Enemies");

            projectiles.Launch(target + Vector2.left * 2f, Vector2.right, 20f, 5f, mask, 1);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(walker.Definition.Health - 1, walker.CurrentHealth);
        }

        private void KillAllExcept(Enemy keep)
        {
            foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                if (enemy != keep && !enemy.IsDead)
                {
                    ((IDamageable)enemy).TakeHit(1000, Vector2.zero);
                }
            }
        }

        private static void Teleport(Rigidbody2D body, Vector2 position)
        {
            body.position = position;
            body.transform.position = position;
        }
    }
}
