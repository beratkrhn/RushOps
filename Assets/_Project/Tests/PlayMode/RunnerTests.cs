using System.Collections;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Navigation;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class RunnerTests
    {
        private const string SceneName = "Arena_Test";

        private EnemyPool _runners;
        private EnemyPool _walkers;
        private FlowFieldNavigator _navigator;
        private PlayerHealth _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _runners = EnemyPools.Of(EnemyPools.Runner);
            _walkers = EnemyPools.Of(EnemyPools.Walker);
            _navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            // These tests place runners themselves; touching the player must not reload the scene.
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
        }

        [Test]
        public void Definition_IsFastAndFragile()
        {
            EnemyDefinition runner = _runners.Definition;
            EnemyDefinition walker = _walkers.Definition;

            Assert.Greater(runner.MoveSpeed, walker.MoveSpeed, "runner must be faster than the walker");
            Assert.Less(runner.Health, walker.Health, "runner must be weaker than the walker");
            Assert.Greater(runner.Points, walker.Points, "runner is worth more than the walker");
            Assert.IsFalse(runner.IsBoss);
        }

        [Test]
        public void Definition_HasOwnSounds()
        {
            EnemyDefinition runner = _runners.Definition;

            Assert.IsNotNull(runner.SpawnClip);
            Assert.IsNotNull(runner.ApproachClip);
            Assert.AreNotEqual(runner.SpawnClip, runner.ApproachClip);
            Assert.Greater(runner.ApproachDistance, 0f);
        }

        [Test]
        public void Waves_UseRunners()
        {
            WaveDefinition waves = Object.FindFirstObjectByType<Zone>().Waves;
            int runners = 0;
            foreach (Wave wave in waves.Waves)
            {
                foreach (WaveEntry entry in wave.Entries)
                {
                    if (entry.Enemy == _runners.Definition)
                    {
                        runners += entry.Count;
                    }
                }
            }

            Assert.Greater(runners, 0, "Arena_Test waves bring no runners");
        }

        [UnityTest]
        public IEnumerator Runner_OneHit_Dies()
        {
            Enemy runner = _runners.Spawn(new Vector2(5.5f, 5.5f));
            yield return null;

            Assert.IsTrue(((IDamageable)runner).TakeHit(10, Vector2.right), "one pistol hit (ADR 0012)");

            Assert.IsTrue(runner.IsDead);
        }

        [UnityTest]
        public IEnumerator Runner_BehindObstacle_ReachesPlayerWithoutEnteringWalls()
        {
            // Player above the 8x2 obstacle at (16..23, 20..21), runner straight below it.
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(20.5f, 23.5f));
            yield return null;
            Enemy runner = _runners.Spawn(new Vector2(20.5f, 17.5f));

            yield return ChaseUntilClose(runner, 6f);
        }

        [UnityTest]
        public IEnumerator Runner_AroundWallEnd_ReachesPlayerWithoutEnteringWalls()
        {
            // The 2x8 obstacle at (8..9, 30..37) stands between them; the runner has to round its end.
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(11.5f, 33.5f));
            yield return null;
            Enemy runner = _runners.Spawn(new Vector2(6.5f, 33.5f));

            yield return ChaseUntilClose(runner, 6f);
        }

        [UnityTest]
        public IEnumerator Runner_Spawn_IsReported()
        {
            Enemy reported = null;
            _runners.EnemySpawned += enemy => reported = enemy;

            Enemy runner = _runners.Spawn(new Vector2(5.5f, 5.5f));
            yield return null;

            Assert.AreSame(runner, reported);
        }

        [UnityTest]
        public IEnumerator Runner_Approach_IsReportedOnce()
        {
            Teleport(_player.GetComponent<Rigidbody2D>(), new Vector2(20.5f, 10.5f));
            yield return null;
            int approached = 0;
            _runners.EnemyApproached += _ => approached++;
            Enemy runner = _runners.Spawn(new Vector2(20.5f, 3.5f));
            float distance = runner.Definition.ApproachDistance;
            yield return null;
            Assert.AreEqual(0, approached, "reported while still far away");

            float timeout = Time.time + 5f;
            while (Time.time < timeout && Vector2.Distance(runner.transform.position, _player.transform.position) > distance * 0.5f)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(1, approached);
        }

        // Fails if the runner's body center ever is inside a wall cell or it does not get within 1 unit in time.
        private IEnumerator ChaseUntilClose(Enemy runner, float seconds)
        {
            var body = runner.GetComponent<Collider2D>();
            float start = Vector2.Distance(runner.transform.position, _player.transform.position);
            float timeout = Time.time + seconds;
            while (Time.time < timeout && Vector2.Distance(runner.transform.position, _player.transform.position) > 1f)
            {
                Vector2 center = body.bounds.center;
                Assert.IsTrue(_navigator.Field.IsWalkable(_navigator.CellOf(center)), $"runner inside a wall at {center}");
                yield return null;
            }

            Assert.Less(Vector2.Distance(runner.transform.position, _player.transform.position), 1f,
                $"runner stuck at {runner.transform.position}, started {start} away");
        }

        private static void Teleport(Rigidbody2D body, Vector2 position)
        {
            body.position = position;
            body.transform.position = position;
        }
    }
}
