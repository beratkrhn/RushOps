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
    /// <summary>The upgrade choice after every third round in the endless arena (issue #279, F-END-11, T2, N20).</summary>
    public class UpgradeChoiceTests
    {
        private const float Timeout = 20f;

        private EndlessRunController _controller;
        private EndlessConfig _config;
        private UpgradeChoiceView _view;
        private PauseController _pause;
        private DebugUpgradeKey _debug;
        private PlayerHealth _health;

        private float _pauseSeconds;
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
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<EndlessRunController>();
            _config = _controller.Rules.Config;
            _view = Object.FindAnyObjectByType<UpgradeChoiceView>();
            _pause = Object.FindAnyObjectByType<PauseController>();
            _debug = _controller.GetComponent<DebugUpgradeKey>();
            _health = Object.FindAnyObjectByType<PlayerHealth>();
            _health.GetComponent<PlayerMoveInput>().enabled = false;
            _health.GetComponent<PlayerAimInput>().enabled = false;
            Assert.IsNotNull(_view, "the arena has the upgrade choice");

            _pauseSeconds = _config.PauseBetweenRounds;
            _hearts = _config.HeartsPerRound;
            _first = _config.EnemiesFirstRound;
            _perRound = _config.EnemiesPerRound;
            _baseEnemy = _config.BaseEnemy;
            _shares = _config.EnemyShares;
            _interval = _config.SpawnIntervalFirstRound;
            _step = _config.SpawnIntervalStep;
            _min = _config.SpawnIntervalMin;
            _maxLimit = _config.MaxActiveLimit;

            // Short breaks and two walkers per round, so a round is over in a moment.
            _config.ConfigureRounds(0.2f, 1);
            _config.ConfigureEnemies(2, 0, _baseEnemy, new EndlessEnemyShare[0]);
            _config.ConfigureSpawning(0.1f, 0f, 0.1f, _maxLimit);
        }

        [TearDown]
        public void RestoreConfig()
        {
            if (_config != null)
            {
                _config.ConfigureRounds(_pauseSeconds, _hearts);
                _config.ConfigureEnemies(_first, _perRound, _baseEnemy, _shares);
                _config.ConfigureSpawning(_interval, _step, _min, _maxLimit);
            }

            Time.timeScale = 1f;
            EndlessStart.Round = 1;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Round3Cleared_ThreeCards_TapOne_LevelRises_Round4Follows()
        {
            _controller.BeginRun(3);
            yield return KillUntil(() => _controller.Run.Phase == EndlessPhase.Choice);

            Assert.AreEqual(3, _controller.Run.Round);
            Assert.IsTrue(_view.IsShowing);
            Assert.AreEqual(3, _view.ShownCards);
            Assert.AreEqual(0f, Time.timeScale, "the game holds during the choice");
            Assert.IsTrue(_pause.IsHeld);
            Assert.IsFalse(_pause.IsPaused, "held, not paused: no pause overlay");
            var shown = new HashSet<UpgradeDefinition>();
            for (int i = 0; i < 3; i++)
            {
                UpgradeCardView card = _view.Cards[i];
                Assert.IsTrue(shown.Add(card.Upgrade), "three different upgrades");
                Assert.AreEqual(EndlessText.UpgradeLevel(1, card.Upgrade.MaxLevel), card.LevelText);
                Assert.IsNotNull(card.Upgrade.Icon, $"{card.Upgrade.Id} has an icon");
            }

            UpgradeDefinition picked = _view.Cards[1].Upgrade;
            _view.Cards[1].Button.onClick.Invoke();

            Assert.AreEqual(1, _controller.Upgrades.Level(picked));
            Assert.IsFalse(_view.IsShowing);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(EndlessPhase.Pause, _controller.Run.Phase, "the round pause follows");
            Assert.AreEqual(4, _controller.Run.Round);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 4 to start");
            Assert.AreEqual(4, _controller.Run.Round);
        }

        [UnityTest]
        public IEnumerator Rounds1And2_BringNoChoice()
        {
            _controller.BeginRun(1);
            yield return KillUntil(() => _controller.Run.RoundsCleared >= 2);

            Assert.AreNotEqual(EndlessPhase.Choice, _controller.Run.Phase);
            Assert.IsFalse(_view.IsShowing);
            Assert.AreEqual(0, _controller.Upgrades.TotalLevels);
        }

        [UnityTest]
        public IEnumerator PauseButton_WorksDuringTheChoice_AndListsTheUpgrades()
        {
            _controller.BeginRun(3);
            yield return KillUntil(() => _controller.Run.Phase == EndlessPhase.Choice);
            var list = Object.FindAnyObjectByType<UpgradeListView>(FindObjectsInactive.Include);

            _pause.Pause();
            Assert.IsTrue(_pause.IsPaused);
            Assert.AreEqual(EndlessText.NoUpgrades, list.Text);
            _pause.Resume();
            Assert.AreEqual(0f, Time.timeScale, "resuming keeps the choice holding the game");
            Assert.IsTrue(_view.IsShowing);

            UpgradeDefinition picked = _view.Cards[0].Upgrade;
            _view.Cards[0].Button.onClick.Invoke();
            yield return null;
            _pause.Pause();
            StringAssert.Contains($"{picked.DisplayName} 1/{picked.MaxLevel}", list.Text);
            _pause.Resume();
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator TwoLeft_TwoCards_NoneLeft_NoChoice()
        {
            IReadOnlyList<UpgradeDefinition> all = _controller.Upgrades.Definitions;
            _controller.BeginRun(3);
            foreach (UpgradeDefinition upgrade in all)
            {
                if (upgrade.Id == "bounce" || upgrade.Id == "damage")
                {
                    continue;
                }

                while (_debug.Grant(upgrade.Id))
                {
                }
            }

            yield return KillUntil(() => _controller.Run.Phase == EndlessPhase.Choice);
            Assert.AreEqual(2, _view.ShownCards);
            Assert.IsFalse(_view.Cards[2].gameObject.activeSelf);

            // Max the last two, then clear round 6: no upgrade is left, so the break follows at once.
            _view.Cards[0].Button.onClick.Invoke();
            while (_debug.Grant("bounce") || _debug.Grant("damage"))
            {
            }

            Assert.IsTrue(_controller.Upgrades.AllMaxed);
            yield return KillUntil(() => _controller.Run.RoundsCleared >= 4);
            Assert.AreNotEqual(EndlessPhase.Choice, _controller.Run.Phase);
            Assert.IsFalse(_view.IsShowing);
            Assert.IsFalse(_pause.IsHeld);
        }

        [UnityTest]
        public IEnumerator NewRunDuringAChoice_ClosesItAndTheTimeRuns()
        {
            _controller.BeginRun(3);
            yield return KillUntil(() => _controller.Run.Phase == EndlessPhase.Choice);

            _controller.BeginRun(1);

            Assert.IsFalse(_view.IsShowing);
            Assert.IsFalse(_pause.IsHeld);
            Assert.AreEqual(1f, Time.timeScale);
        }

        // Kills every living enemy as it appears, until the condition holds.
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
