using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Endless;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Scoring;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Bosses in the endless arena (issue #210): boss rounds, health per appearance, the fountain and the win.</summary>
    public class EndlessBossTests
    {
        private const string SceneName = "Endless_Arena";
        private const float Timeout = 30f;

        private EndlessRunController _controller;
        private EndlessConfig _config;
        private PlayerHealth _health;
        private EndlessHud _hud;
        private float _pause;
        private int _hearts;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            EndlessStart.Round = 10;
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<EndlessRunController>();
            Assert.IsNotNull(_controller, "no EndlessRunController in the arena");
            _health = Object.FindAnyObjectByType<PlayerHealth>();
            _hud = Object.FindAnyObjectByType<EndlessHud>();
            _config = _controller.Rules.Config;
            _health.GetComponent<PlayerMoveInput>().enabled = false;
            _health.GetComponent<PlayerAimInput>().enabled = false;
            _pause = _config.PauseBetweenRounds;
            _hearts = _config.HeartsPerRound;
            _config.ConfigureRounds(0.2f, _hearts);
        }

        [TearDown]
        public void RestoreConfig()
        {
            if (_config != null)
            {
                _config.ConfigureRounds(_pause, _hearts);
            }

            Time.timeScale = 1f;
            EndlessStart.Round = 1;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Round10_FromTheTestStart_KolossWalksAroundTheFountain_AndItsDeathEndsTheRound()
        {
            // The test start set round 10 before the scene loaded; the run waits in the break before it.
            Assert.AreEqual(10, _controller.Run.Round);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 10 to start");

            BossFight fight = _controller.CurrentFight;
            Assert.IsNotNull(fight, "round 10 is a boss round");
            Assert.IsTrue(fight.IsRunning);
            Boss boss = fight.Bodies[0];
            Assert.AreSame(_config.Bosses[0], boss.Definition, "the Koloss comes first");
            Assert.AreEqual(boss.Definition.Health, boss.MaxHealth, "first appearance: base health");
            var bar = Object.FindAnyObjectByType<BossHealthBar>();
            Assert.IsTrue(bar.IsVisible, "boss health bar as in the campaign");
            Assert.AreEqual(boss.Definition.DisplayName, bar.Name);
            yield return null;
            // Round 10 also brings the rifle; its notice takes the second line for a while, two lines at most.
            StringAssert.StartsWith("Runde 10\n", _hud.TrackerText);
            Assert.AreEqual(1, _hud.TrackerText.Split('\n').Length - 1, "two lines, clear of the boss bar");

            // It comes from the north; the player waits in the south, the fountain in between.
            var fountain = GameObject.Find("Fountain").GetComponent<CircleCollider2D>();
            Vector2 center = fountain.bounds.center;
            Assert.Greater(boss.BodyCenter.y, center.y);
            float closest = float.MaxValue;
            Time.timeScale = 3f;
            yield return WaitFor(() =>
            {
                _health.Heal(_health.Max);
                closest = Mathf.Min(closest, (boss.BodyCenter - center).magnitude);
                return boss.BodyCenter.y < center.y - 1f;
            }, "the Koloss to get past the fountain");
            Time.timeScale = 1f;
            Assert.GreaterOrEqual(closest, fountain.radius + boss.BodyRadius - 0.15f, "never inside the fountain");

            // Immune to insta-kill: a lethal hit deals its damage only.
            int before = boss.CurrentHealth;
            Assert.IsTrue(boss.TakeHit(1, Vector2.down, true));
            Assert.AreEqual(before - 1, boss.CurrentHealth);

            int score = Object.FindAnyObjectByType<LevelScore>().Keeper.Score;
            Assert.IsTrue(boss.TakeHit(boss.CurrentHealth, Vector2.down));
            Assert.IsTrue(fight.IsWon);
            Assert.AreEqual(score + 1000, Object.FindAnyObjectByType<LevelScore>().Keeper.Score, "the win gives 1000 points");

            yield return WaitFor(() => _controller.Run.RoundsCleared == 1, "round 10 to end");
            Assert.AreEqual(EndlessPhase.Pause, _controller.Run.Phase, "the normal break follows");
            Assert.AreEqual(11, _controller.Run.Round);
            Assert.AreEqual("Runde 11", _hud.BannerText);
            Assert.IsFalse(bar.IsVisible, "the bar goes with the boss");
            Assert.IsNull(_controller.CurrentFight);
        }

        [UnityTest]
        public IEnumerator Round20_BringsTheBroodMother_WithAQuarterMoreHealth_AndHerRunnersCountTowardsTheCap()
        {
            _controller.BeginRun(20);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 20 to start");

            Boss boss = _controller.CurrentFight.Bodies[0];
            Assert.IsInstanceOf<BroodMother>(boss);
            Assert.AreEqual(Mathf.RoundToInt(boss.Definition.Health * 1.25f), boss.MaxHealth, "second boss appearance: +25 %");

            var director = Object.FindAnyObjectByType<WaveDirector>();
            Assert.AreEqual(_controller.Rules.MaxActive(20), director.MaxActive);
            var mother = (BroodMother)boss;
            yield return WaitFor(() =>
            {
                _health.Heal(_health.Max);
                return mother.BornCount > 0;
            }, "a runner");
            Assert.Less(director.FreeSlots, director.MaxActive, "her runners take slots of the cap");

            boss.TakeHit(boss.CurrentHealth, Vector2.down);
            yield return WaitFor(() => _controller.Run.RoundsCleared == 1, "round 20 to end");
            Assert.AreEqual(21, _controller.Run.Round);
            foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                Assert.IsTrue(!enemy.isActiveAndEnabled || enemy.IsDead, "her brood dies with her");
            }
        }

        [UnityTest]
        public IEnumerator Round30_BringsTheMegaBoomer_AndBothHalvesMustFall()
        {
            _controller.BeginRun(30);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 30 to start");

            BossFight fight = _controller.CurrentFight;
            var mega = (MegaBoomer)fight.Bodies[0];
            Assert.AreEqual(Mathf.RoundToInt(mega.Definition.Health * 1.5f), mega.MaxHealth, "third boss appearance");

            mega.TakeHit(mega.CurrentHealth - 10, Vector2.down);
            yield return WaitFor(() =>
            {
                _health.Heal(_health.Max);
                return fight.Bodies.Count == 3;
            }, "the split");
            Assert.IsFalse(fight.IsWon);
            Assert.AreEqual(EndlessPhase.Running, _controller.Run.Phase);
            foreach (Boss half in fight.Bodies)
            {
                if (half.IsInFight)
                {
                    half.TakeHit(half.CurrentHealth, Vector2.down);
                }
            }

            yield return WaitFor(() => _controller.Run.RoundsCleared == 1, "round 30 to end");
            Assert.AreEqual(31, _controller.Run.Round);
        }

        [UnityTest]
        public IEnumerator NextBossRound_GetsAFreshBoss()
        {
            _controller.BeginRun(10);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 10 to start");
            BossFight first = _controller.CurrentFight;
            first.Bodies[0].TakeHit(first.Bodies[0].CurrentHealth, Vector2.down);
            yield return WaitFor(() => _controller.Run.RoundsCleared == 1, "round 10 to end");

            _controller.BeginRun(40);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 40 to start");
            BossFight again = _controller.CurrentFight;
            Assert.AreNotSame(first, again);
            Assert.IsTrue(first == null, "the beaten boss is gone");
            Assert.AreSame(_config.Bosses[0], again.Bodies[0].Definition, "the order starts over with the Koloss");
            Assert.AreEqual(Mathf.RoundToInt(again.Bodies[0].Definition.Health * 1.75f), again.Bodies[0].MaxHealth);
            Assert.IsTrue(again.Bodies[0].IsActive);
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
