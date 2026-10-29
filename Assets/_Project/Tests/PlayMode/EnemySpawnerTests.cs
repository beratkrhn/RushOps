using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class EnemySpawnerTests
    {
        private const string SceneName = "Arena_Test";
        // Faster test runs; physics and timers scale along.
        private const float TimeScale = 4f;

        private EnemySpawner _spawner;
        private EnemyPool _pool;
        private SpawnerConfig _config;
        private Transform _player;
        private readonly List<string> _violations = new List<string>();

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _spawner = Object.FindFirstObjectByType<EnemySpawner>();
            _pool = Object.FindFirstObjectByType<EnemyPool>();
            Assert.IsNotNull(_spawner, "Arena_Test has no EnemySpawner");
            _config = _spawner.Config;
            PlayerHealth health = Object.FindFirstObjectByType<PlayerHealth>();
            _player = health.transform;
            // Walkers reach the player during the long runs; dying must not reload the scene.
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            health.GetComponent<PlayerMoveInput>().enabled = false;
            health.GetComponent<PlayerAimInput>().enabled = false;
            _violations.Clear();
            Time.timeScale = TimeScale;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void Scene_HasSpawnPointsAndNoEnemiesYet()
        {
            Assert.Greater(Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None).Length, 1);
            Assert.AreEqual(0, _pool.CountActive, "enemies appear only after the marker");
        }

        [UnityTest]
        public IEnumerator FillsUpToMaxAndNeverExceedsIt()
        {
            float timeout = Time.time + _config.TelegraphDuration + _config.SpawnInterval * (_config.MaxActive + 4);
            while (Time.time < timeout && _pool.CountActive < _config.MaxActive)
            {
                Assert.LessOrEqual(_pool.CountActive + _spawner.AnnouncedCount, _config.MaxActive);
                yield return null;
            }

            Assert.AreEqual(_config.MaxActive, _pool.CountActive);

            for (int i = 0; i < 30; i++)
            {
                yield return null;
                Assert.LessOrEqual(_pool.CountActive, _config.MaxActive);
            }
        }

        [UnityTest]
        public IEnumerator KilledEnemy_IsReplaced()
        {
            yield return WaitForFull();
            Enemy victim = Object.FindFirstObjectByType<Enemy>();

            ((IDamageable)victim).TakeHit(1000, Vector2.zero);
            float timeout = Time.time + victim.Definition.DeathDuration + _config.TelegraphDuration + _config.SpawnInterval * 3f;
            yield return new WaitForSeconds(victim.Definition.DeathDuration + 0.05f);
            Assert.Less(_pool.CountActive, _config.MaxActive);
            while (Time.time < timeout && _pool.CountActive < _config.MaxActive)
            {
                yield return null;
            }

            Assert.AreEqual(_config.MaxActive, _pool.CountActive);
        }

        [UnityTest]
        public IEnumerator SpawnPointsInViewOrNearPlayer_AreSkipped()
        {
            // Stand next to the bottom-left spawn point, so several points are in view or close.
            _player.GetComponent<Rigidbody2D>().position = new Vector2(5.5f, 6.5f);
            yield return new WaitForSeconds(0.2f);
            Camera camera = Camera.main;
            _spawner.Spawned += (enemy, point) =>
            {
                var half = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
                Rect view = SpawnRules.View(camera.transform.position, half, 0f);
                if (view.Contains(point.Position))
                {
                    _violations.Add($"{point.name} in view {view}");
                }

                if (Vector2.Distance(point.Position, _player.position) < _config.MinPlayerDistance)
                {
                    _violations.Add($"{point.name} next to player");
                }
            };

            yield return WaitForFull();

            Assert.IsEmpty(_violations, string.Join(", ", _violations));
        }

        private IEnumerator WaitForFull()
        {
            float timeout = Time.time + _config.TelegraphDuration + _config.SpawnInterval * (_config.MaxActive + 4);
            while (Time.time < timeout && _pool.CountActive < _config.MaxActive)
            {
                yield return null;
            }

            Assert.AreEqual(_config.MaxActive, _pool.CountActive, "spawner did not fill up");
        }
    }
}
