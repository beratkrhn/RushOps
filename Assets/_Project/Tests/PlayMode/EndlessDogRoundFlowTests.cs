using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Endless;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The dog round of the endless arena (issue #264): every 6th round sends dogs only, in packs of three.</summary>
    public class EndlessDogRoundFlowTests
    {
        private const string SceneName = "Endless_Arena";
        private const float Timeout = 20f;

        private readonly List<(int Frame, SpawnPoint Point, EnemyDefinition Enemy)> _spawns =
            new List<(int, SpawnPoint, EnemyDefinition)>();

        private EndlessRunController _controller;
        private EndlessConfig _config;
        private WaveDirector _director;
        private PlayerHealth _health;
        private EndlessHud _hud;

        // Values of the shared config asset, put back after every test.
        private float _pause;
        private int _hearts;
        private int _first;
        private int _perRound;
        private EnemyDefinition _baseEnemy;
        private EndlessEnemyShare[] _shares;
        private float _interval;
        private float _step;
        private float _min;
        private int _maxStart;
        private int _maxStep;
        private int _maxEvery;
        private int _maxCap;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            EndlessStart.Round = 1;
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<EndlessRunController>();
            Assert.IsNotNull(_controller, "no EndlessRunController in the arena");
            _director = Object.FindAnyObjectByType<WaveDirector>();
            _health = Object.FindAnyObjectByType<PlayerHealth>();
            _hud = Object.FindAnyObjectByType<EndlessHud>();
            _config = _controller.Rules.Config;
            _health.GetComponent<PlayerMoveInput>().enabled = false;
            _health.GetComponent<PlayerAimInput>().enabled = false;

            _pause = _config.PauseBetweenRounds;
            _hearts = _config.HeartsPerRound;
            _first = _config.EnemiesFirstRound;
            _perRound = _config.EnemiesPerRound;
            _baseEnemy = _config.BaseEnemy;
            _shares = _config.EnemyShares;
            _interval = _config.SpawnIntervalFirstRound;
            _step = _config.SpawnIntervalStep;
            _min = _config.SpawnIntervalMin;
            _maxStart = _config.MaxActiveStart;
            _maxStep = _config.MaxActiveStep;
            _maxEvery = _config.MaxActiveEveryRounds;
            _maxCap = _config.MaxActiveCap;

            // Short breaks and four enemies per round: the dog round rounds them up to two packs of three.
            _config.ConfigureRounds(0.2f, 1);
            _config.ConfigureEnemies(4, 0, _baseEnemy, new EndlessEnemyShare[0]);
            _config.ConfigureSpawning(0.1f, 0f, 0.1f, Mathf.Max(6, _maxStart), _maxStep, _maxEvery, Mathf.Max(6, _maxCap));
            _director.Spawned += OnSpawned;
        }

        [TearDown]
        public void RestoreConfig()
        {
            if (_director != null)
            {
                _director.Spawned -= OnSpawned;
            }

            if (_config != null)
            {
                _config.ConfigureRounds(_pause, _hearts);
                _config.ConfigureEnemies(_first, _perRound, _baseEnemy, _shares);
                _config.ConfigureSpawning(_interval, _step, _min, _maxStart, _maxStep, _maxEvery, _maxCap);
            }

            Time.timeScale = 1f;
            EndlessStart.Round = 1;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Round6_SendsDogsInPacksOfThree_Round7_SendsNormalEnemies()
        {
            EnemyDefinition dog = _config.DogEnemy;
            Assert.IsNotNull(dog, "the config has no dog");
            Assert.IsTrue(_controller.Rules.IsDogRound(6));
            _controller.BeginRun(6);

            Assert.AreEqual("Runde 6", _hud.BannerText);
            Assert.AreEqual("Hunderunde!", _hud.BannerSubtitleText, "the break announces the dog round");

            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 6 to start");
            Assert.IsTrue(_controller.CurrentRound.IsDogRound);
            Assert.AreEqual(string.Empty, _hud.BannerSubtitleText);
            yield return WaitFor(() => _spawns.Count > 0, "the first pack");
            yield return null;
            StringAssert.StartsWith("Runde 6\nNoch ", _hud.TrackerText);
            StringAssert.Contains("Hund", _hud.TrackerText);

            yield return KillUntil(() => _controller.Run.RoundsCleared >= 1);
            PickFirstUpgrade();

            Assert.AreEqual(6, _spawns.Count, "four enemies round up to two full packs");
            var packs = new Dictionary<(int, SpawnPoint), int>();
            foreach ((int frame, SpawnPoint point, EnemyDefinition enemy) in _spawns)
            {
                Assert.AreSame(dog, enemy, "a dog round has dogs only");
                packs.TryGetValue((frame, point), out int members);
                packs[(frame, point)] = members + 1;
            }

            foreach (int members in packs.Values)
            {
                Assert.AreEqual(3, members, "a pack comes out of one spawn point together");
            }

            Assert.AreEqual(7, _controller.Run.Round);
            Assert.AreEqual("Runde 7", _hud.BannerText);
            Assert.AreEqual(string.Empty, _hud.BannerSubtitleText, "round 7 is a normal round");

            _spawns.Clear();
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 7 to start");
            Assert.IsFalse(_controller.CurrentRound.IsDogRound);
            yield return WaitFor(() => _spawns.Count > 0, "the enemies of round 7");
            yield return null;
            StringAssert.Contains("Gegner", _hud.TrackerText);
            foreach ((int _, SpawnPoint _, EnemyDefinition enemy) in _spawns)
            {
                Assert.AreNotSame(dog, enemy, "no dogs in a normal round");
            }
        }

        private void OnSpawned(Enemy enemy, SpawnPoint point)
        {
            _spawns.Add((Time.frameCount, point, enemy.Definition));
        }

        // Kills every living enemy as it appears, until the condition holds.
        private static IEnumerator KillUntil(System.Func<bool> condition)
        {
            float end = Time.realtimeSinceStartup + Timeout;
            while (!condition())
            {
                Assert.Less(Time.realtimeSinceStartup, end, "timeout while killing the enemies of the round");
                foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                {
                    if (enemy.isActiveAndEnabled && !enemy.IsDead)
                    {
                        enemy.Kill();
                    }
                }

                yield return null;
            }
        }

        // Since issue #279 every third round and every boss round end with the upgrade choice: take the first card.
        private void PickFirstUpgrade()
        {
            Assert.AreEqual(EndlessPhase.Choice, _controller.Run.Phase, "the cleared round brings an upgrade choice");
            Assert.IsTrue(_controller.Choose(_controller.CurrentOffer[0]));
        }

        private static IEnumerator WaitFor(System.Func<bool> condition, string what)
        {
            float end = Time.realtimeSinceStartup + Timeout;
            while (!condition())
            {
                Assert.Less(Time.realtimeSinceStartup, end, "timeout waiting for " + what);
                yield return null;
            }
        }
    }
}
