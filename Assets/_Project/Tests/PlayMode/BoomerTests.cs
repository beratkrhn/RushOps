using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Cameras;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Feel;
using RushOps.Levels;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class BoomerTests
    {
        private const string SceneName = "Arena_Test";
        // Free floor along the east wall of Arena_Test, far from the player's start.
        private static readonly Vector2 BlastSpot = new Vector2(34.5f, 18.5f);

        private EnemyPool _boomers;
        private EnemyPool _walkers;
        private EnemyPool _runners;
        private PlayerHealth _player;
        private ExplosionSystem _explosions;
        private LevelKills _kills;
        private readonly List<int> _explosionFrames = new List<int>();

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _boomers = EnemyPools.Of(EnemyPools.Boomer);
            _walkers = EnemyPools.Of(EnemyPools.Walker);
            _runners = EnemyPools.Of(EnemyPools.Runner);
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            _explosions = Object.FindFirstObjectByType<ExplosionSystem>();
            _kills = Object.FindFirstObjectByType<LevelKills>();
            Assert.IsNotNull(_explosions, "Arena_Test has no ExplosionSystem");
            _explosions.Exploded += OnExploded;
            _explosionFrames.Clear();
            // These tests place enemies themselves; losing hearts must not reload the scene.
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void Definition_HasStartValuesAndExplosion()
        {
            EnemyDefinition boomer = _boomers.Definition;

            Assert.AreEqual(4, boomer.Health);
            Assert.AreEqual(2f, boomer.MoveSpeed, 1e-4f);
            Assert.AreEqual(20, boomer.Points);
            Assert.AreEqual(0, boomer.ContactDamage, "the boomer hurts by its explosion, not by touch");
            Assert.IsNotNull(boomer.Explosion);
            Assert.AreEqual(2.5f, boomer.Explosion.Radius, 1e-4f);
            Assert.AreEqual(2, boomer.Explosion.PlayerDamage);
            Assert.Greater(boomer.Explosion.ChainDelay, 0f);
            Assert.IsNotEmpty(boomer.Explosion.EffectFrames);
            Assert.IsNotNull(boomer.Explosion.Clip);
        }

        [Test]
        public void WarningSound_DiffersFromOtherEnemies()
        {
            AudioClip warning = _boomers.Definition.ApproachClip;
            Assert.IsNotNull(warning, "boomer has no warning sound");
            foreach (EnemyPool pool in Object.FindObjectsByType<EnemyPool>(FindObjectsSortMode.None))
            {
                if (pool != _boomers)
                {
                    Assert.AreNotEqual(warning, pool.Definition.ApproachClip, $"{pool.Definition.name} uses the boomer's warning");
                    Assert.AreNotEqual(warning, pool.Definition.SpawnClip);
                }
            }
        }

        [Test]
        public void Waves_UseBoomers()
        {
            int boomers = 0;
            foreach (Wave wave in Object.FindFirstObjectByType<Zone>().Waves.Waves)
            {
                foreach (WaveEntry entry in wave.Entries)
                {
                    if (entry.Enemy == _boomers.Definition)
                    {
                        boomers += entry.Count;
                    }
                }
            }

            Assert.Greater(boomers, 0, "Arena_Test waves bring no boomers");
        }

        [UnityTest]
        public IEnumerator ShotDead_BlastKillsNearbyZombiesAndTheyCountAsKills()
        {
            Enemy boomer = Place(_boomers, BlastSpot);
            Enemy near = Place(_walkers, BlastSpot + new Vector2(1f, 0f));
            Enemy alsoNear = Place(_runners, BlastSpot + new Vector2(-1.2f, 1.2f));
            Enemy far = Place(_walkers, BlastSpot + new Vector2(0f, 6f));
            yield return new WaitForFixedUpdate();
            int killsBefore = _kills.Counter.Count;

            boomer.TakeHit(boomer.CurrentHealth, Vector2.right);
            yield return WaitForExplosions(1);

            Assert.IsTrue(near.IsDead, "walker next to the boomer survived");
            Assert.IsTrue(alsoNear.IsDead, "runner next to the boomer survived");
            Assert.IsFalse(far.IsDead, "walker outside the radius was hit");
            Assert.AreEqual(killsBefore + 3, _kills.Counter.Count, "boomer and both blast victims must count as kills");
        }

        [UnityTest]
        public IEnumerator Blast_HurtsPlayerInRadiusByTwoHearts()
        {
            Teleport(BlastSpot + new Vector2(0f, 4f));
            yield return new WaitForFixedUpdate();
            Enemy boomer = Place(_boomers, BlastSpot + new Vector2(0f, 2f));
            int before = _player.Current;

            boomer.Kill();
            yield return WaitForExplosions(1);

            Assert.AreEqual(before - 2, _player.Current);
        }

        [UnityTest]
        public IEnumerator Blast_RespectsPlayerInvulnerability()
        {
            Teleport(BlastSpot + new Vector2(0f, 4f));
            yield return new WaitForFixedUpdate();
            Enemy boomer = Place(_boomers, BlastSpot + new Vector2(0f, 2f));
            Assert.IsTrue(_player.TakeDamage(1));
            int before = _player.Current;

            boomer.Kill();
            yield return WaitForExplosions(1);

            Assert.AreEqual(before, _player.Current, "the blast hurt the player during the invulnerability after a hit");
        }

        [UnityTest]
        public IEnumerator Blast_OutsideRadius_SparesPlayer()
        {
            Teleport(BlastSpot + new Vector2(0f, 6f));
            yield return new WaitForFixedUpdate();
            Enemy boomer = Place(_boomers, BlastSpot);
            int before = _player.Current;

            boomer.Kill();
            yield return WaitForExplosions(1);

            Assert.AreEqual(before, _player.Current);
        }

        [UnityTest]
        public IEnumerator TouchingPlayer_Explodes()
        {
            Teleport(new Vector2(35.5f, 24.5f));
            yield return new WaitForFixedUpdate();
            int before = _player.Current;
            Enemy boomer = _boomers.Spawn(new Vector2(35.5f, 21.5f));

            float timeout = Time.realtimeSinceStartup + 6f;
            while (Time.realtimeSinceStartup < timeout && _explosionFrames.Count == 0)
            {
                yield return null;
            }

            Assert.IsTrue(boomer.IsDead || !boomer.gameObject.activeSelf, "boomer did not blow up on touch");
            Assert.AreEqual(1, _explosionFrames.Count);
            Assert.AreEqual(before - 2, _player.Current, "touch blast must cost exactly the explosion's 2 hearts");
        }

        [UnityTest]
        public IEnumerator ChainReaction_GoesOffAFewFramesLater()
        {
            Enemy first = Place(_boomers, BlastSpot);
            Enemy second = Place(_boomers, BlastSpot + new Vector2(0f, 2f));
            Enemy third = Place(_boomers, BlastSpot + new Vector2(0f, 4f));
            yield return new WaitForFixedUpdate();

            first.Kill();
            yield return WaitForExplosions(3);

            Assert.AreEqual(3, _explosionFrames.Count, "chain did not reach all boomers in range of each other");
            Assert.IsTrue(second.IsDead || !second.gameObject.activeSelf);
            Assert.IsTrue(third.IsDead || !third.gameObject.activeSelf);
            Assert.GreaterOrEqual(_explosionFrames[1] - _explosionFrames[0], 2, "chain explosions must not go off in the same frame");
            Assert.GreaterOrEqual(_explosionFrames[2] - _explosionFrames[1], 2);
        }

        [UnityTest]
        public IEnumerator Blast_ShakesCameraAndShowsEffect()
        {
            var camera = Object.FindFirstObjectByType<CameraFollow>();
            var effects = Object.FindFirstObjectByType<ExplosionEffectPool>();
            Assert.IsNotNull(effects, "Arena_Test has no ExplosionEffectPool");
            Enemy boomer = Place(_boomers, BlastSpot);

            boomer.Kill();
            yield return WaitForExplosions(1);

            Assert.Greater(effects.CountActive, 0, "no blast effect");
            bool shook = false;
            float timeout = Time.realtimeSinceStartup + 0.3f;
            while (Time.realtimeSinceStartup < timeout && !shook)
            {
                shook = camera.ShakeOffset.sqrMagnitude > 1e-6f;
                yield return null;
            }

            Assert.IsTrue(shook, "camera did not shake");
        }

        private void OnExploded(Vector2 center, ExplosionDefinition definition)
        {
            _explosionFrames.Add(Time.frameCount);
        }

        // Real time, because the enemy deaths trigger hitstops.
        private IEnumerator WaitForExplosions(int count)
        {
            float timeout = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < timeout && _explosionFrames.Count < count)
            {
                yield return null;
            }

            Assert.GreaterOrEqual(_explosionFrames.Count, count, "explosion did not go off");
        }

        private static Enemy Place(EnemyPool pool, Vector2 position)
        {
            Enemy enemy = pool.Spawn(position);
            enemy.GetComponent<EnemyMovement>().Stop();
            return enemy;
        }

        private void Teleport(Vector2 position)
        {
            var body = _player.GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
        }
    }
}
