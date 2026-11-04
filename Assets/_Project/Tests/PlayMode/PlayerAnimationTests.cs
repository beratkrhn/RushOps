using System.Collections;
using NUnit.Framework;
using RushOps.Characters;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The player's walk, hit and death animations (issue #102).</summary>
    public class PlayerAnimationTests
    {
        private PlayerHealth _health;
        private PlayerMovement _movement;
        private DirectionalSpriteAnimator _view;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;

            Object.FindFirstObjectByType<RushOps.Enemies.WaveDirector>().enabled = false;
            _health = Object.FindFirstObjectByType<PlayerHealth>();
            _movement = _health.GetComponent<PlayerMovement>();
            _view = _health.GetComponent<DirectionalSpriteAnimator>();
            _health.GetComponent<PlayerMoveInput>().enabled = false;
            _health.GetComponent<PlayerAimInput>().enabled = false;
            _health.GetComponent<PlayerDashInput>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator Moving_ShowsWalkFrames_StandingShowsIdle()
        {
            yield return null;
            StringAssert.StartsWith("Player_Idle", _health.GetComponent<SpriteRenderer>().sprite.name);

            _movement.MoveInput = Vector2.right;
            yield return new WaitForSeconds(0.3f);
            StringAssert.StartsWith("Player_Walk", _health.GetComponent<SpriteRenderer>().sprite.name);

            _movement.MoveInput = Vector2.zero;
            yield return new WaitForSeconds(0.5f);
            StringAssert.StartsWith("Player_Idle", _health.GetComponent<SpriteRenderer>().sprite.name);
        }

        [UnityTest]
        public IEnumerator LosingAHeart_PlaysHit()
        {
            yield return null;
            Assert.IsTrue(_health.TakeDamage(1));

            Assert.AreEqual(SpriteAnimationMode.Hit, _view.Mode);
        }

        [UnityTest]
        public IEnumerator Death_PlaysToTheEndBeforeFailedScreen()
        {
            yield return null;
            var results = Object.FindFirstObjectByType<LevelResults>();
            _health.TakeDamage(_health.Current);

            Assert.AreEqual(SpriteAnimationMode.Death, _view.Mode);
            Assert.Greater(_view.DeathDuration, 0f);
            yield return new WaitForSeconds(_view.DeathDuration * 0.8f);
            Assert.IsFalse(results.ShowsFailed, "„Gescheitert“ covers the death animation");
            StringAssert.StartsWith("Player_Death", _health.GetComponent<SpriteRenderer>().sprite.name);
        }
    }
}
