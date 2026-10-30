using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class WaveDirectorTests
    {
        private const string SceneName = "Arena_Test";
        // Faster test runs; physics and timers scale along.
        private const float TimeScale = 4f;

        private WaveDirector _director;
        private EnemyPool _pool;
        private SpawnerConfig _config;
        private Zone _zone;
        private Transform _player;
        private readonly List<string> _violations = new List<string>();

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _director = Object.FindFirstObjectByType<WaveDirector>();
            _pool = Object.FindFirstObjectByType<EnemyPool>();
            _zone = Object.FindFirstObjectByType<Zone>();
            Assert.IsNotNull(_director, "Arena_Test has no WaveDirector");
            Assert.IsNotNull(_zone, "Arena_Test has no Zone");
            _config = _director.Config;
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
        public void Scene_HasZoneWithWavesAndSpawnPoints()
        {
            Assert.Greater(_zone.SpawnPoints.Length, 1);
            Assert.IsNotNull(_zone.Waves);
            Assert.Greater(_zone.Waves.Waves[0].TotalCount, _config.MaxActive, "first wave should exceed the limit to use the queue");
            Assert.AreEqual(ZoneState.Active, _zone.State, "player starts inside the zone");
        }

        [UnityTest]
        public IEnumerator FirstWave_FillsUpToMaxAndQueuesTheRest()
        {
            yield return WaitForFull();
            WaveSequence sequence = _director.SequenceOf(_zone);
            float timeout = Time.time + _zone.Waves.SpawnInterval * (_zone.Waves.Waves[0].TotalCount + 2);

            // Wait until the wave has asked for all its enemies.
            while (Time.time < timeout && sequence.Phase == WavePhase.Spawning)
            {
                yield return null;
                Assert.LessOrEqual(_pool.CountActive + _director.AnnouncedCount, _config.MaxActive);
            }

            Assert.AreEqual(WavePhase.Clearing, sequence.Phase);

            int requested = _zone.Waves.Waves[0].TotalCount;
            Assert.AreEqual(requested - _config.MaxActive, _director.QueuedCount, "overflow waits in the queue");
        }

        [UnityTest]
        public IEnumerator KilledEnemy_IsReplacedFromQueue()
        {
            yield return WaitForFull();
            yield return new WaitForSeconds(_zone.Waves.SpawnInterval * 4f);
            int queued = _director.QueuedCount;
            Assert.Greater(queued, 0);
            Enemy victim = _pool.Active[0];

            ((IDamageable)victim).TakeHit(1000, Vector2.zero);
            yield return new WaitForSeconds(victim.Definition.DeathDuration + 0.05f);
            Assert.LessOrEqual(_pool.CountActive + _director.AnnouncedCount, _config.MaxActive);
            yield return WaitForFull();

            Assert.AreEqual(queued - 1, _director.QueuedCount);
        }

        [UnityTest]
        public IEnumerator SpawnPointsInViewOrNearPlayer_AreSkipped()
        {
            // Stand next to the bottom-left spawn point, so several points are in view or close.
            _player.GetComponent<Rigidbody2D>().position = new Vector2(5.5f, 6.5f);
            yield return new WaitForSeconds(0.2f);
            Camera camera = Camera.main;
            _director.Spawned += (enemy, point) =>
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

        [UnityTest]
        public IEnumerator WaveCleared_NextWaveStarts()
        {
            var started = new List<int>();
            _director.WaveStarted += (zone, wave) => started.Add(wave);
            WaveDefinition waves = _zone.Waves;
            float timeout = Time.time + waves.Waves[0].TotalCount * (waves.SpawnInterval + _config.TelegraphDuration)
                + waves.PauseBetweenWaves + 5f;

            while (Time.time < timeout && !started.Contains(1))
            {
                KillAll();
                yield return null;
            }

            CollectionAssert.Contains(started, 1, "second wave did not start after the first was cleared");
        }

        private void KillAll()
        {
            for (int i = _pool.Active.Count - 1; i >= 0; i--)
            {
                Enemy enemy = _pool.Active[i];
                if (!enemy.IsDead)
                {
                    ((IDamageable)enemy).TakeHit(1000, Vector2.zero);
                }
            }
        }

        private IEnumerator WaitForFull()
        {
            float timeout = Time.time + _config.TelegraphDuration + _zone.Waves.SpawnInterval * (_config.MaxActive + 4);
            while (Time.time < timeout && _pool.CountActive < _config.MaxActive)
            {
                Assert.LessOrEqual(_pool.CountActive + _director.AnnouncedCount, _config.MaxActive);
                yield return null;
            }

            Assert.AreEqual(_config.MaxActive, _pool.CountActive, "director did not fill up");
        }
    }
}
