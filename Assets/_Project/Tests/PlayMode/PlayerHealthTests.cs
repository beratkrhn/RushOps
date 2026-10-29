using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class PlayerHealthTests
    {
        private const string SceneName = "Arena_Test";

        private PlayerHealth _health;
        private PlayerMovement _movement;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _health = Object.FindFirstObjectByType<PlayerHealth>();
            Assert.IsNotNull(_health, "Arena_Test has no PlayerHealth");
            _movement = _health.GetComponent<PlayerMovement>();
            _health.GetComponent<PlayerMoveInput>().enabled = false;
            _health.GetComponent<PlayerAimInput>().enabled = false;
            _health.GetComponent<PlayerDashInput>().enabled = false;
        }

        [Test]
        public void Start_HasMaxHeartsFromConfig()
        {
            Assert.AreEqual(_movement.Config.MaxHearts, _health.Max);
            Assert.AreEqual(_health.Max, _health.Current);
        }

        [UnityTest]
        public IEnumerator TouchingWalker_LosesOneHeartThenBlinksAndIsInvulnerable()
        {
            Enemy walker = Object.FindFirstObjectByType<Enemy>();
            Assert.IsNotNull(walker, "Arena_Test has no walker with contact damage");
            int reported = -1;
            _health.HealthChanged += (current, max) => reported = current;

            // Right next to the walker; exactly on top of it the physics has no contact normal.
            _health.GetComponent<Rigidbody2D>().position = walker.transform.position + new Vector3(0.5f, 0f, 0f);
            float timeout = Time.time + 0.5f;
            while (Time.time < timeout && _health.Current == _health.Max)
            {
                yield return null;
            }

            Assert.AreEqual(_health.Max - walker.ContactDamage, _health.Current);
            Assert.AreEqual(_health.Current, reported);
            Assert.IsTrue(_health.IsInvulnerable);

            // Still touching: no further damage during the invulnerability, and the sprite blinks.
            var renderer = _health.GetComponent<SpriteRenderer>();
            bool seenOn = false;
            bool seenOff = false;
            float until = Time.time + _movement.Config.HitInvulnerability * 0.5f;
            while (Time.time < until)
            {
                seenOn |= renderer.enabled;
                seenOff |= !renderer.enabled;
                yield return null;
            }

            Assert.AreEqual(_health.Max - walker.ContactDamage, _health.Current);
            Assert.IsTrue(seenOn && seenOff, "blinks during the invulnerability");

            yield return new WaitForSeconds(_movement.Config.HitInvulnerability);
            Assert.AreEqual(_health.Max - 2 * walker.ContactDamage, _health.Current, "hurts again after the invulnerability");
        }

        [UnityTest]
        public IEnumerator DashIFrames_BlockDamage()
        {
            Assert.IsTrue(_health.GetComponent<PlayerDash>().TryDash());

            Assert.IsFalse(_health.TakeDamage(1));
            Assert.AreEqual(_health.Max, _health.Current);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Death_StopsPlayerThenRestartsLevel()
        {
            bool died = false;
            _health.Died += () => died = true;

            _health.TakeDamage(_health.Max);
            yield return null;

            Assert.IsTrue(died);
            Assert.IsTrue(_health.IsDead);
            Assert.IsFalse(_movement.enabled, "movement stops on death");
            Assert.IsFalse(_health.GetComponent<PlayerWeapon>().enabled, "weapon stops on death");

            yield return new WaitForSecondsRealtime(_movement.Config.DeathRestartDelay + 0.5f);

            PlayerHealth restarted = Object.FindFirstObjectByType<PlayerHealth>();
            Assert.IsNotNull(restarted);
            Assert.AreNotSame(_health, restarted, "level restarted");
            Assert.AreEqual(restarted.Max, restarted.Current);
        }
    }
}
