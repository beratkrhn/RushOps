using System;
using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Endless;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// The upgrades with new effects in the endless arena (issue #278, F-END-12): bouncing shots, the fire trail of the
    /// dash and the permanent drone.
    /// </summary>
    public class EndlessEffectUpgradeTests
    {
        private const float Timeout = 20f;

        private EndlessRunController _controller;
        private EndlessConfig _config;
        private DebugUpgradeKey _debug;
        private PlayerHealth _health;
        private PlayerAim _aim;
        private float _pause;
        private int _hearts;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            EndlessStart.Round = 1;
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            _controller = Object.FindAnyObjectByType<EndlessRunController>();
            _config = _controller.Rules.Config;
            _debug = _controller.GetComponent<DebugUpgradeKey>();
            _health = Object.FindAnyObjectByType<PlayerHealth>();
            _aim = _health.GetComponent<PlayerAim>();
            _health.GetComponent<PlayerMoveInput>().enabled = false;
            _health.GetComponent<PlayerAimInput>().enabled = false;
            _pause = _config.PauseBetweenRounds;
            _hearts = _config.HeartsPerRound;
        }

        [TearDown]
        public void TearDown()
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
        public IEnumerator Bounce_ShotAtAWall_ComesBackAndHitsAnEnemyBehindThePlayer()
        {
            // Stand 2.5 tiles east of the west wall, a held enemy 2 tiles behind (east of) the player.
            var body = _health.GetComponent<Rigidbody2D>();
            Vector2 from = body.position + Vector2.up * 0.5f;
            RaycastHit2D wall = Physics2D.Raycast(from, Vector2.left, 40f, LayerMask.GetMask("Walls"));
            Assert.IsNotNull(wall.collider, "no wall west of the player");
            body.position = new Vector2(wall.point.x + 2.5f, body.position.y);
            yield return new WaitForFixedUpdate();
            Enemy enemy = EnemyPools.Of(EnemyPools.Brute).Spawn(body.position + Vector2.right * 2f);
            enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            int before = enemy.CurrentHealth;

            // Without the upgrade the wall stops every shot.
            _aim.AimInput = Vector2.left;
            yield return new WaitForSeconds(0.8f);
            _aim.AimInput = Vector2.zero;
            Assert.AreEqual(before, enemy.CurrentHealth, "no bounce without the upgrade");
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(_debug.Grant("bounce"));
            Assert.AreEqual(1, _health.GetComponent<PlayerWeapon>().Bounces);
            _aim.AimInput = Vector2.left;
            yield return WaitFor(() => enemy.CurrentHealth < before || enemy.IsDead, "a bounced shot to hit the enemy");
            _aim.AimInput = Vector2.zero;

            Assert.AreEqual(_health.Max, _health.Current, "the player is never hit by own shots");
        }

        [UnityTest]
        public IEnumerator FireTrail_HurtsAnEnemyInIt_NeverThePlayer()
        {
            Assert.IsTrue(_debug.Grant("fire_trail"));
            var trail = Object.FindAnyObjectByType<FireTrail>();
            Assert.IsTrue(trail.Active);

            Vector2 player = _health.transform.position;
            Enemy enemy = EnemyPools.Of(EnemyPools.Brute).Spawn(player + Vector2.right * 3.5f);
            enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            int before = enemy.CurrentHealth;
            // The player stands in the fire too; a row of tiles runs from the held enemy towards the player.
            trail.Lay(player);
            trail.Tiles.BreakChain();
            for (int i = 0; i < 4; i++)
            {
                trail.Lay((Vector2)enemy.transform.position + Vector2.left * (0.6f * i));
            }

            Assert.AreEqual(5, trail.Tiles.Count);
            yield return new WaitForSeconds(1f);

            Assert.Less(enemy.CurrentHealth, before, "the enemy burns");
            Assert.AreEqual(_health.Max, _health.Current, "the player never burns");
            yield return new WaitForSeconds(1f);
            Assert.AreEqual(0, trail.Tiles.Count, "the tiles go out after 1.5 s");
        }

        [UnityTest]
        public IEnumerator FireTrail_DashLaysTiles_OnlyWithTheUpgrade()
        {
            var trail = Object.FindAnyObjectByType<FireTrail>();
            var movement = _health.GetComponent<PlayerMovement>();
            var dash = _health.GetComponent<PlayerDash>();
            movement.MoveInput = Vector2.right;
            Assert.IsTrue(dash.TryDash());
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(0, trail.Tiles.Count, "no trail without the upgrade");

            Assert.IsTrue(_debug.Grant("fire_trail"));
            yield return new WaitForSeconds(dash.CooldownRemaining + 0.05f);
            movement.MoveInput = Vector2.left;
            Assert.IsTrue(dash.TryDash());
            yield return new WaitForSeconds(0.4f);
            movement.MoveInput = Vector2.zero;

            Assert.GreaterOrEqual(trail.Tiles.Count, 2, "the dash leaves burning tiles");
        }

        [UnityTest]
        public IEnumerator PermanentCompanion_StaysAfter30sAndABossRound_AndItsPerkDoesNotDrop()
        {
            _config.ConfigureRounds(0.2f, _hearts);
            _controller.BeginRun(10);
            Assert.IsTrue(_debug.Grant("permanent_companion"));
            var drone = Object.FindAnyObjectByType<CompanionDrone>();
            var dropper = Object.FindAnyObjectByType<PerkDropper>();
            Assert.IsTrue(drone.IsActive);
            Assert.IsTrue(drone.Permanent);
            Assert.AreEqual(PerkEffect.Companion, dropper.Excluded);
            for (int i = 0; i < 200; i++)
            {
                Assert.AreNotEqual(PerkEffect.Companion, dropper.Pool.Pick().Effect, "the companion perk does not drop");
            }

            yield return WaitFor(() => _controller.Run.Phase == EndlessPhase.Running, "round 10 to start");
            BossFight fight = _controller.CurrentFight;
            Assert.IsNotNull(fight);
            Boss boss = fight.Bodies[0];
            boss.TakeHit(boss.CurrentHealth, Vector2.down);
            yield return WaitFor(() => _controller.Run.RoundsCleared == 1, "round 10 to end");
            Assert.IsTrue(drone.IsActive, "the drone stays after the boss round");

            Time.timeScale = 6f;
            float end = Time.time + 30f;
            while (Time.time < end)
            {
                _health.Heal(_health.Max);
                yield return null;
            }

            Time.timeScale = 1f;
            Assert.IsTrue(drone.IsActive, "the drone stays after 30 s");

            _controller.BeginRun(1);
            Assert.IsFalse(drone.IsActive, "a new run starts without the drone");
            Assert.IsNull(dropper.Excluded);
        }

        [UnityTest]
        public IEnumerator NewRun_DropsBouncesAndTrail()
        {
            Assert.IsTrue(_debug.Grant("bounce"));
            Assert.IsTrue(_debug.Grant("fire_trail"));
            var trail = Object.FindAnyObjectByType<FireTrail>();
            trail.Lay(_health.transform.position);
            yield return null;

            _controller.BeginRun(1);

            Assert.AreEqual(0, _health.GetComponent<PlayerWeapon>().Bounces);
            Assert.IsFalse(trail.Active);
            Assert.AreEqual(0, trail.Tiles.Count);
        }

        private static IEnumerator WaitFor(Func<bool> condition, string what)
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
