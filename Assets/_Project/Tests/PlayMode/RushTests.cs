using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Rush;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The rush in the arena (issue #270, F-RSH-02 to F-RSH-06): slow motion, dash without cooldown, damage.</summary>
    public class RushTests
    {
        private PlayerRush _rush;
        private PlayerDash _dash;
        private PlayerWeapon _weapon;
        private PlayerMovement _player;

        private IEnumerator Load(string scene)
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;
            var director = Object.FindAnyObjectByType<WaveDirector>();
            if (director != null)
            {
                director.enabled = false;
            }

            var restarter = Object.FindAnyObjectByType<LevelRestarter>();
            if (restarter != null)
            {
                restarter.enabled = false;
            }

            _player = Object.FindAnyObjectByType<PlayerMovement>();
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
            _rush = _player.GetComponent<PlayerRush>();
            _dash = _player.GetComponent<PlayerDash>();
            _weapon = _player.GetComponent<PlayerWeapon>();
            Assert.IsNotNull(_rush, $"no rush on the player in {scene}");
        }

        [UnityTearDown]
        public IEnumerator Restore()
        {
            Time.timeScale = 1f;
            SaveGameIsolation.Reset();
            yield return null;
        }

        [UnityTest]
        public IEnumerator FullRush_SlowsEnemies_DashesWithoutCooldown_HitsHarder_ThenEnds()
        {
            yield return Load("Arena_Test");
            Vector2 center = _player.transform.position;
            Enemy before = EnemyPools.Of(EnemyPools.Walker).Spawn(center + new Vector2(0f, 7f));
            for (int i = 0; i < 6; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            float normalSpeed = before.GetComponent<EnemyMovement>().Velocity.magnitude;
            Assert.Greater(normalSpeed, 0.1f, "the walker does not walk");
            int normalDamage = _weapon.Damage;

            Assert.IsFalse(_rush.TryTrigger(), "an empty meter triggered");
            _rush.FillUp();
            Assert.IsTrue(_rush.TryTrigger());
            Assert.IsTrue(_rush.IsActive);
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(normalSpeed * 0.5f, before.GetComponent<EnemyMovement>().Velocity.magnitude, normalSpeed * 0.1f,
                "a walker of before the rush is not at half speed");
            Enemy during = EnemyPools.Of(EnemyPools.Walker).Spawn(center + new Vector2(0f, -7f));
            Assert.AreEqual(0.5f, during.TimeFactor, 1e-5f, "a walker spawned during the rush is not slowed");
            Assert.AreEqual(WeaponMath.ShotDamage(normalDamage, 1.5f), _weapon.Damage, "the rush does not raise the damage");
            Assert.IsTrue(Object.FindAnyObjectByType<RushOverlay>().IsVisible, "no rush tint");

            _player.MoveInput = Vector2.right;
            Assert.IsTrue(_dash.TryDash());
            yield return new WaitForSeconds(0.4f);
            Assert.IsTrue(_dash.TryDash(), "the second dash waited for a cooldown during the rush");

            float end = Time.time + 5.5f;
            while (Time.time < end && _rush.IsActive)
            {
                yield return null;
            }

            Assert.IsFalse(_rush.IsActive, "the rush did not end after 5 s");
            Assert.AreEqual(1f, before.TimeFactor, 1e-5f);
            Assert.AreEqual(1f, during.TimeFactor, 1e-5f);
            Assert.AreEqual(normalDamage, _weapon.Damage);
            Assert.IsFalse(Object.FindAnyObjectByType<RushOverlay>().IsVisible, "the tint stayed");
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(_dash.TryDash());
            yield return new WaitForSeconds(0.4f);
            Assert.IsFalse(_dash.TryDash(), "the dash has no cooldown after the rush");
        }

        [UnityTest]
        public IEnumerator Kills_FillTheMeter()
        {
            yield return Load("Arena_Test");
            Vector2 center = _player.transform.position;
            for (int i = 0; i < 3; i++)
            {
                Enemy enemy = EnemyPools.Of(EnemyPools.Walker).Spawn(center + new Vector2(-3f + 3f * i, 6f));
                enemy.Kill();
            }

            yield return null;

            Assert.GreaterOrEqual(_rush.Meter.Fill, 3, "kills did not fill the meter");
        }

        [UnityTest]
        public IEnumerator Pause_HoldsTheRushTimer()
        {
            yield return Load("Arena_Test");
            _rush.FillUp();
            _rush.TryTrigger();
            yield return null;
            Time.timeScale = 0f;
            float remaining = _rush.Meter.Remaining;

            yield return new WaitForSecondsRealtime(0.5f);

            Assert.AreEqual(remaining, _rush.Meter.Remaining, 1e-4f, "the rush ran on in the pause");
            Assert.IsTrue(_rush.IsActive);
        }

        [UnityTest]
        public IEnumerator Death_EndsTheRush()
        {
            yield return Load("Arena_Test");
            _rush.FillUp();
            _rush.TryTrigger();
            var health = _player.GetComponent<PlayerHealth>();

            while (!health.IsDead)
            {
                health.TakeDamage(5);
                yield return new WaitForSeconds(1.1f);
            }

            Assert.IsFalse(_rush.IsActive, "death did not end the rush");
        }

        [UnityTest]
        public IEnumerator Bosses_AreSlowedLess()
        {
            yield return Load("Boss_Test");
            var fight = Object.FindAnyObjectByType<BossFight>();
            Assert.IsNotNull(fight);

            _rush.FillUp();
            _rush.TryTrigger();
            yield return null;

            Assert.AreEqual(0.75f, fight.TimeFactor, 1e-5f, "the boss fight is not part of the rush");
            foreach (Boss body in fight.Bodies)
            {
                Assert.AreEqual(0.75f, body.TimeFactor, 1e-5f);
            }

            _rush.Meter.End();
            Assert.AreEqual(1f, fight.Bodies[0].TimeFactor, 1e-5f);
        }

        [UnityTest]
        public IEnumerator EveryGameScene_HasTheRush()
        {
            foreach (string scene in new[] { "Level_1-1", "Level_3-3", "Endless_Arena" })
            {
                yield return Load(scene);
            }
        }
    }
}
