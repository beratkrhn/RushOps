using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Endless;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Save;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The daily run (issue #280, F-END-13, F-END-14, N21, N22): same start all day, own list, not in the top 10.</summary>
    public class DailyRunTests
    {
        private const float Timeout = 30f;
        private const string Date = "2026-11-11";
        private const int ComparedSpawns = 4;

        private EndlessRunController _controller;
        private EndlessConfig _config;
        private WaveDirector _director;
        private PlayerHealth _health;
        private readonly List<Vector2> _spawns = new List<Vector2>();

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
            EndlessStart.Daily = false;
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<EndlessRunController>();
            _config = _controller.Rules.Config;
            _director = Object.FindAnyObjectByType<WaveDirector>();
            _health = Object.FindAnyObjectByType<PlayerHealth>();
            _health.GetComponent<PlayerMoveInput>().enabled = false;
            _health.GetComponent<PlayerAimInput>().enabled = false;
            _director.Spawned += OnSpawned;

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
            _config.ConfigureRounds(0.2f, 1);
            _config.ConfigureEnemies(2, 0, _baseEnemy, new EndlessEnemyShare[0]);
            _config.ConfigureSpawning(0.1f, 0f, 0.1f, _maxStart, _maxStep, _maxEvery, _maxCap);
        }

        [TearDown]
        public void TearDown()
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
            EndlessStart.Daily = false;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator SameDate_SameFirstSpawnsAndOffer_NormalRunDiffers()
        {
            var first = new Attempt();
            yield return Play(true, first);
            var second = new Attempt();
            yield return Play(true, second);
            var normal = new Attempt();
            yield return Play(false, normal);

            CollectionAssert.AreEqual(first.Spawns, second.Spawns, "the same date starts with the same spawn points");
            CollectionAssert.AreEqual(first.Offer, second.Offer, "the same date brings the same first offer");
            Assert.IsFalse(Same(first.Spawns, normal.Spawns) && Same(first.Offer, normal.Offer),
                "a normal run is not the daily run");
        }

        [UnityTest]
        public IEnumerator DailyBest_GoesIntoTheDailyList_NotIntoTheTop10()
        {
            _controller.BeginRun(1, true, Date);
            Assert.IsTrue(_controller.IsDaily);
            var hud = Object.FindAnyObjectByType<EndlessHud>();
            StringAssert.Contains("Tageslauf 11.11.26", hud.BannerSubtitleText, "the break names the daily run and its date");
            yield return KillUntil(() => _controller.Run.RoundsCleared >= 1);

            _health.TakeDamage(_health.Max);
            yield return WaitFor(() => hud.ShowsResult, "the result screen");

            SaveData data = SaveGameIsolation.Service.Data;
            Assert.IsNotNull(data);
            Assert.IsEmpty(data.Endless.Top, "a daily run never enters the top 10");
            Assert.AreEqual(1, data.Endless.Runs, "but it counts as a run");
            DailyScore day = DailyRecords.Find(data.Daily, Date);
            Assert.IsNotNull(day, "the daily best is saved");
            Assert.AreEqual(2, day.Round);
            Assert.IsTrue(hud.ShowsNewBest, "„Neuer Tagesbestwert!“ on the first try");
            StringAssert.StartsWith("Tagesbestwert:", hud.RankText);
        }

        [UnityTest]
        public IEnumerator MainMenu_EndlessArea_ShowsToday_AndStartsTheDailyRun()
        {
            SaveGameService save = SaveGameIsolation.Service;
            string today = DailySeed.Today();
            DailyRecords.Record(save.Data.Daily, 4321, 9, today);
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;

            var endless = Object.FindAnyObjectByType<EndlessMenu>();
            GameObject.Find("EndlessButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Assert.IsTrue(endless.IsOpen);
            Assert.AreEqual("Heute: 4321, Runde 9", endless.TodayText);
            Assert.AreEqual(EndlessText.Date(today), endless.ColumnText(0));
            Assert.AreEqual("4321", endless.ColumnText(1));
            Assert.AreEqual("9", endless.ColumnText(2));
            Assert.IsNotNull(GameObject.Find("StartButton"));

            GameObject.Find("DailyButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            yield return null;
            yield return null;
            var controller = Object.FindAnyObjectByType<EndlessRunController>();
            Assert.IsNotNull(controller);
            Assert.IsTrue(controller.IsDaily);
            Assert.AreEqual(today, controller.DailyDate);
        }

        private sealed class Attempt
        {
            public readonly List<Vector2> Spawns = new List<Vector2>();
            public readonly List<string> Offer = new List<string>();
        }

        // Plays rounds 1 to 3 of a run and keeps its first spawn points and its first upgrade offer.
        private IEnumerator Play(bool daily, Attempt attempt)
        {
            _spawns.Clear();
            _controller.BeginRun(1, daily, Date);
            yield return KillUntil(() => _controller.Run.Phase == EndlessPhase.Choice);
            for (int i = 0; i < ComparedSpawns && i < _spawns.Count; i++)
            {
                attempt.Spawns.Add(_spawns[i]);
            }

            foreach (UpgradeDefinition upgrade in _controller.CurrentOffer)
            {
                attempt.Offer.Add(upgrade.Id);
            }

            Assert.AreEqual(ComparedSpawns, attempt.Spawns.Count);
            Assert.AreEqual(3, attempt.Offer.Count);
        }

        private void OnSpawned(Enemy enemy, SpawnPoint point)
        {
            _spawns.Add(point.Position);
        }

        private static bool Same<T>(List<T> a, List<T> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerator KillUntil(System.Func<bool> condition)
        {
            float end = Time.realtimeSinceStartup + Timeout;
            while (!condition())
            {
                Assert.Less(Time.realtimeSinceStartup, end, "timeout while killing the enemies of the round");
                _health.Heal(_health.Max);
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

        private static IEnumerator WaitFor(System.Func<bool> condition, string what)
        {
            float end = Time.realtimeSinceStartup + Timeout;
            while (!condition())
            {
                Assert.Less(Time.realtimeSinceStartup, end, $"timeout waiting for {what}");
                yield return null;
            }
        }
    }
}
