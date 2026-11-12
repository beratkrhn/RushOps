using System.Collections;
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
    /// <summary>The round flow of the endless arena (issue #207): rounds, hearts, cap of enemies and the end of a run.</summary>
    public class EndlessRoundFlowTests
    {
        private const string SceneName = "Endless_Arena";
        private const float Timeout = 20f;

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
        private int _maxLimit;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            EndlessStart.Round = 1;
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            FindParts();
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
            _maxLimit = _config.MaxActiveLimit;

            // Short breaks and two walkers per round, so a round is over in a few seconds.
            _config.ConfigureRounds(0.2f, 1);
            _config.ConfigureEnemies(2, 0, _baseEnemy, new EndlessEnemyShare[0]);
            _config.ConfigureSpawning(0.1f, 0f, 0.1f, _maxLimit);
        }

        [TearDown]
        public void RestoreConfig()
        {
            if (_config != null)
            {
                _config.ConfigureRounds(_pause, _hearts);
                _config.ConfigureEnemies(_first, _perRound, _baseEnemy, _shares);
                _config.ConfigureSpawning(_interval, _step, _min, _maxLimit);
            }

            Time.timeScale = 1f;
            EndlessStart.Round = 1;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Round1_Finishes_HeartComesBack_Round2Starts()
        {
            // One heart missing, so the heart after the round shows.
            _health.TakeDamage(1);
            int missing = _health.Current;
            _controller.BeginRun(1);

            Assert.AreEqual(EndlessPhase.Pause, _controller.Run.Phase);
            Assert.AreEqual("Runde 1", _hud.BannerText, "large display during the break");

            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 1 to start");
            Assert.AreEqual(string.Empty, _hud.BannerText);
            Assert.AreEqual(1, _controller.CurrentRound.Number);
            Assert.AreEqual(_controller.Rules.MaxActive, _director.MaxActive, "the endless mode has no cap");
            yield return WaitFor(() => HasEnemies(), "enemies of round 1");
            yield return null;
            StringAssert.StartsWith("Runde 1", _hud.TrackerText);
            StringAssert.Contains("Gegner", _hud.TrackerText);

            yield return KillUntil(() => _controller.Run.RoundsCleared >= 1);

            Assert.AreEqual(1, _controller.Run.RoundsCleared);
            Assert.AreEqual(missing + 1, _health.Current, "one heart after the cleared round");
            Assert.AreEqual(2, _controller.Run.Round, "the next round is announced");
            Assert.AreEqual("Runde 2", _hud.BannerText);

            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 2 to start");
            Assert.AreEqual(2, _controller.CurrentRound.Number);
            Assert.IsFalse(_hud.ShowsResult);
        }

        [UnityTest]
        public IEnumerator Death_EndsTheRun_AndShowsTheResultScreen()
        {
            _controller.BeginRun(1);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 1 to start");

            _health.TakeDamage(_health.Max);

            Assert.AreEqual(EndlessPhase.Ended, _controller.Run.Phase);
            Assert.AreEqual(1, _controller.Run.Round);
            yield return WaitFor(() => _hud.ShowsResult, "the result screen");
            Assert.AreEqual(string.Empty, _hud.TrackerText);
            Assert.AreEqual(0f, Time.timeScale, "the game is frozen behind the result screen");
            Assert.IsNotNull(GameObject.Find("RetryButton"), "Nochmal");
            Assert.IsNotNull(GameObject.Find("MenuButton"), "Hauptmenü");
        }

        [UnityTest]
        public IEnumerator Death_SavesTheRunInTheTopTen_AndUnlocksTheRoundTenSkin()
        {
            RushOps.Save.SaveGameService save = SaveGameIsolation.Service;
            Assume.That(save, Is.Not.Null, "needs the save game service of the editor");
            RushOps.Save.EndlessRecords.Record(save.Data.Endless, 99999, 30, "2026-01-01");
            var score = Object.FindAnyObjectByType<RushOps.Scoring.LevelScore>();
            _controller.BeginRun(10);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 10 to start");
            score.Keeper.AddPoints(1500);

            _health.TakeDamage(_health.Max);
            yield return WaitFor(() => _hud.ShowsResult, "the result screen");

            Assert.IsTrue(SaveGameIsolation.Files.Exists(System.IO.Path.Combine("playmode-tests", "save.json")), "autosave");
            var loaded = new RushOps.Save.SaveGameStore(SaveGameIsolation.Files, "playmode-tests").Load();
            Assert.AreEqual(2, loaded.Endless.Runs);
            Assert.AreEqual(30, loaded.Endless.HighestRound);
            Assert.AreEqual(2, loaded.Endless.Top.Count);
            Assert.AreEqual(1500, loaded.Endless.Top[1].Score);
            Assert.AreEqual(10, loaded.Endless.Top[1].Round);
            CollectionAssert.Contains(loaded.Skins.Unlocked, "Gladiator", "round 10 reached");
            Assert.AreEqual("Platz 2 der Top 10", _hud.RankText);
            Assert.IsFalse(_hud.ShowsNewBest, "99999 is still the best");
            StringAssert.Contains("1500", _hud.TopColumnText(1));
            StringAssert.Contains("99999", _hud.TopColumnText(1));
        }

        [UnityTest]
        public IEnumerator Endless_HasNoCap_TheCampaignKeepsIt()
        {
            _controller.BeginRun(25);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 25 to start");

            Assert.AreEqual(int.MaxValue, _controller.Rules.MaxActive);
            Assert.AreEqual(int.MaxValue, _director.MaxActive, "no limit in the endless mode (F-END-04)");
            Assert.AreEqual(10, _director.Config.MaxActive, "the campaign cap stays at 10");
        }

        [UnityTest]
        public IEnumerator Run_StartsWithThePistol_WhateverWasSelected()
        {
            var weapon = _health.GetComponent<PlayerWeapon>();
            Assert.IsNotNull(_config.StartWeapon);
            weapon.Slots.Reset(_config.WallWeapons[0].Weapon);
            Assert.AreNotSame(_config.StartWeapon, weapon.Slots.Selected);

            _controller.BeginRun(1);
            yield return null;

            Assert.AreSame(_config.StartWeapon, weapon.Slots.Selected, "first slot");
            Assert.AreSame(_config.StartWeapon, weapon.ActiveWeapon);
            Assert.IsFalse(weapon.Slots.HasTwo, "no second weapon at the start");
        }

        [UnityTest]
        public IEnumerator TestStart_BeginsAtTheGivenRound_AndForgetsIt()
        {
            EndlessStart.Round = 5;
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            FindParts();

            Assert.AreEqual(5, _controller.Run.Round);
            Assert.AreEqual("Runde 5", _hud.BannerText);
            Assert.AreEqual(1, EndlessStart.Round, "the next start is round 1 again");
        }

        private void FindParts()
        {
            _controller = Object.FindAnyObjectByType<EndlessRunController>();
            Assert.IsNotNull(_controller, "no EndlessRunController in the arena");
            _director = Object.FindAnyObjectByType<WaveDirector>();
            _health = Object.FindAnyObjectByType<PlayerHealth>();
            _hud = Object.FindAnyObjectByType<EndlessHud>();
            _config = _controller.Rules.Config;
        }

        private static bool HasEnemies()
        {
            foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                if (enemy.isActiveAndEnabled && !enemy.IsDead)
                {
                    return true;
                }
            }

            return false;
        }

        // Kills every living enemy as it appears, until the condition holds.
        private IEnumerator KillUntil(System.Func<bool> condition)
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
