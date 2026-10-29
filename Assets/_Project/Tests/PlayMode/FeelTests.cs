using System.Collections;
using NUnit.Framework;
using RushOps.Cameras;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Feel;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class FeelTests
    {
        private const string SceneName = "Arena_Test";

        private EnemyPool _pool;
        private PlayerHealth _player;
        private Hitstop _hitstop;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            Object.FindFirstObjectByType<EnemySpawner>().enabled = false;
            _pool = Object.FindFirstObjectByType<EnemyPool>();
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            _hitstop = Object.FindFirstObjectByType<Hitstop>();
            Assert.IsNotNull(_hitstop, "Arena_Test has no Hitstop");
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator EnemyHit_FlashesWhiteAndIsKnockedBack()
        {
            Enemy walker = _pool.Spawn(new Vector2(5.5f, 5.5f));
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(((IDamageable)walker).TakeHit(1, Vector2.right));
            Assert.IsTrue(walker.IsFlashing);
            var movement = walker.GetComponent<EnemyMovement>();
            Assert.IsTrue(movement.IsKnockedBack);
            yield return new WaitForFixedUpdate();

            Assert.Greater(movement.Velocity.x, 0f, "pushed along the shot");
            var block = new MaterialPropertyBlock();
            walker.GetComponent<SpriteRenderer>().GetPropertyBlock(block);
            Assert.AreEqual(1f, block.GetFloat("_FlashAmount"));

            yield return new WaitForSeconds(0.3f);

            Assert.IsFalse(walker.IsFlashing);
            Assert.IsFalse(movement.IsKnockedBack);
            walker.GetComponent<SpriteRenderer>().GetPropertyBlock(block);
            Assert.AreEqual(0f, block.GetFloat("_FlashAmount"));
        }

        [UnityTest]
        public IEnumerator EnemyDeath_TriggersShortHitstop()
        {
            Enemy walker = _pool.Spawn(new Vector2(5.5f, 5.5f));
            yield return null;

            ((IDamageable)walker).TakeHit(1000, Vector2.right);

            Assert.IsTrue(_hitstop.IsActive);
            Assert.AreEqual(0f, Time.timeScale);
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsFalse(_hitstop.IsActive);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Hitstop_PauseDuringFreeze_StaysPaused()
        {
            var pause = Object.FindFirstObjectByType<PauseController>();

            _hitstop.Trigger(0.05f);
            pause.Pause();
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsFalse(_hitstop.IsActive);
            Assert.AreEqual(0f, Time.timeScale, "pause keeps the time stopped");
            pause.Resume();
        }

        [UnityTest]
        public IEnumerator PlayerDamage_ShakesCamera()
        {
            var camera = Object.FindFirstObjectByType<CameraFollow>();

            _player.TakeDamage(1);
            bool shook = false;
            float until = Time.realtimeSinceStartup + 0.15f;
            while (Time.realtimeSinceStartup < until)
            {
                shook |= camera.ShakeOffset.sqrMagnitude > 1e-6f;
                yield return null;
            }

            Assert.IsTrue(shook);
            yield return new WaitForSecondsRealtime(0.3f);

            Assert.AreEqual(Vector2.zero, camera.ShakeOffset);
        }

        [UnityTest]
        public IEnumerator Shot_PlaysSound()
        {
            var source = Object.FindFirstObjectByType<GameFeel>().GetComponent<AudioSource>();
            var aim = _player.GetComponent<PlayerAim>();

            aim.AimInput = Vector2.up;
            bool played = false;
            float until = Time.time + 0.3f;
            while (Time.time < until)
            {
                played |= source.isPlaying;
                yield return null;
            }

            Assert.IsTrue(played);
        }
    }
}
