using System.Collections;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Endless;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The wall weapons of the endless arena (issue #209): locked until round 5, 10 and 15, then picked up as usual.</summary>
    public class EndlessWallWeaponTests
    {
        private const string SceneName = "Endless_Arena";
        private const float Timeout = 20f;

        private EndlessRunController _controller;
        private EndlessConfig _config;
        private EndlessHud _hud;
        private PlayerWeapon _weapon;
        private WallWeapon[] _walls;

        private float _pause;
        private int _hearts;
        private int _first;
        private int _perRound;
        private EnemyDefinition _baseEnemy;
        private EndlessEnemyShare[] _shares;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            EndlessStart.Round = 1;
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<EndlessRunController>();
            _hud = Object.FindAnyObjectByType<EndlessHud>();
            _weapon = Object.FindAnyObjectByType<PlayerWeapon>();
            _weapon.GetComponent<PlayerMoveInput>().enabled = false;
            _weapon.GetComponent<PlayerAimInput>().enabled = false;
            _config = _controller.Rules.Config;
            _walls = Object.FindObjectsByType<WallWeapon>(FindObjectsSortMode.None);
            System.Array.Sort(_walls, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            _pause = _config.PauseBetweenRounds;
            _hearts = _config.HeartsPerRound;
            _first = _config.EnemiesFirstRound;
            _perRound = _config.EnemiesPerRound;
            _baseEnemy = _config.BaseEnemy;
            _shares = _config.EnemyShares;
            // Short breaks and a single walker per round; the player stands at the wall and is not in danger for long.
            _config.ConfigureRounds(0.2f, 1);
            _config.ConfigureEnemies(1, 0, _baseEnemy, new EndlessEnemyShare[0]);
        }

        [TearDown]
        public void RestoreConfig()
        {
            if (_config != null)
            {
                _config.ConfigureRounds(_pause, _hearts);
                _config.ConfigureEnemies(_first, _perRound, _baseEnemy, _shares);
            }

            Time.timeScale = 1f;
            EndlessStart.Round = 1;
            SaveGameIsolation.Reset();
        }

        [Test]
        public void Arena_HangsThreeWeaponsSideBySide_InTheOrderOfTheConfig()
        {
            Assert.AreEqual(3, _walls.Length);
            EndlessWeaponRound[] rounds = _config.WallWeapons;
            Assert.AreEqual(rounds.Length, _walls.Length);
            var cells = new System.Collections.Generic.List<float>();
            foreach (WallWeapon wall in _walls)
            {
                cells.Add(wall.transform.position.y);
            }

            Assert.AreEqual(cells[0], cells[1], 1e-4f);
            Assert.AreEqual(cells[1], cells[2], 1e-4f);
            for (int i = 0; i < rounds.Length; i++)
            {
                Assert.AreSame(rounds[i].Weapon, _walls[i].Weapon, $"slot {i + 1}");
            }
        }

        [UnityTest]
        public IEnumerator BeforeTheirRound_AllThreeAreLocked_AndTheRingDoesNotFill()
        {
            yield return null;
            foreach (WallWeapon wall in _walls)
            {
                Assert.IsTrue(wall.IsLocked, wall.Weapon.name);
            }

            Teleport(_walls[0].PickupPoint);
            yield return new WaitForSeconds(0.8f);

            Assert.AreEqual(0f, _walls[0].Progress, "the ring does not fill");
            Assert.IsFalse(_weapon.Slots.Holds(_walls[0].Weapon), "not picked up");
            Assert.IsFalse(_weapon.Slots.HasTwo);
        }

        [UnityTest]
        public IEnumerator FromRoundFive_TheShotgunUnlocks_WithNoticeAndCanBePickedUp()
        {
            _controller.BeginRun(5);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 5");
            yield return null;

            Assert.IsFalse(_walls[0].IsLocked, "shotgun");
            Assert.IsTrue(_walls[1].IsLocked, "assault rifle waits for round 10");
            Assert.IsTrue(_walls[2].IsLocked, "dual pistols wait for round 15");
            StringAssert.Contains(EndlessText.NewWeapon(_walls[0].Weapon.DisplayName), _hud.TrackerText);

            Teleport(_walls[0].PickupPoint);
            yield return new WaitForSeconds(0.9f);

            Assert.AreSame(_walls[0].Weapon, _weapon.ActiveWeapon, "picked up into the second slot");
            Assert.IsTrue(_weapon.Slots.HasTwo);
        }

        [UnityTest]
        public IEnumerator TheNoticeGoesAway_AfterAFewSeconds()
        {
            _controller.BeginRun(5);
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 5");
            yield return null;
            StringAssert.Contains("Neue Waffe", _hud.TrackerText);

            yield return WaitFor(() => !_hud.TrackerText.Contains("Neue Waffe") || _controller.Run.Round != 5,
                "the notice to disappear");
        }

        [UnityTest]
        public IEnumerator RunStartingAtTen_HasTheShotgunFromTheStart_AndTheRifleAtRoundTen()
        {
            _controller.BeginRun(10);
            yield return null;

            Assert.IsFalse(_walls[0].IsLocked, "round 5 is behind");
            Assert.IsTrue(_walls[1].IsLocked, "unlocks with round 10");
            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 10");
            Assert.IsFalse(_walls[1].IsLocked);
            Assert.IsTrue(_walls[2].IsLocked);
        }

        private void Teleport(Vector2 position)
        {
            var body = _weapon.GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
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
