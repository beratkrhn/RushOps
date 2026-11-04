using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Player;
using RushOps.Scoring;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class LevelResultsTests
    {
        private const string SceneName = "Arena_Zonen";

        private LevelResults _results;
        private LevelExit _exit;
        private PauseController _pause;
        private Rigidbody2D _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _results = Object.FindFirstObjectByType<LevelResults>();
            _exit = Object.FindFirstObjectByType<LevelExit>();
            _pause = Object.FindFirstObjectByType<PauseController>();
            Assert.IsNotNull(_results, "Arena_Zonen has no LevelResults");
            Assert.IsNotNull(_exit, "Arena_Zonen has no LevelExit");
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            _player = Object.FindFirstObjectByType<PlayerHealth>().GetComponent<Rigidbody2D>();
            _player.GetComponent<PlayerMoveInput>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void Level_HasDefinitionWithSceneAndStars()
        {
            LevelDefinition level = _results.Level;
            Assert.IsNotNull(level);
            Assert.AreEqual(SceneName, level.SceneName);
            Assert.IsNotEmpty(level.DisplayName);
            Assert.LessOrEqual(level.TwoStarScore, level.ThreeStarScore);
            Assert.IsFalse(_results.ShowsDone);
            Assert.IsFalse(_results.ShowsFailed);
        }

        [UnityTest]
        public IEnumerator ReachingExit_ShowsDoneScreen_AndFreezesGame()
        {
            var score = Object.FindFirstObjectByType<LevelScore>();
            score.Keeper.RegisterKill(10);

            Teleport(_exit.transform.position);
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsTrue(_exit.IsReached);
            Assert.IsTrue(_results.ShowsDone, "no „Level geschafft!“ screen");
            Assert.AreEqual(0f, Time.timeScale, "game runs on behind the screen");
            Assert.That(_results.StarsShown, Is.InRange(1, 3));
            Assert.AreEqual(1, _results.StarsShown, "10 points give only the star for finishing");

            // Coming back from the background must not unfreeze the game.
            _pause.Pause();
            _pause.Resume();
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(_pause.IsFrozen);
        }

        [UnityTest]
        public IEnumerator Continue_LoadsTestMenu()
        {
            _results.ShowDone();
            _results.Continue();
            yield return null;
            yield return null;

            Assert.AreEqual("TestMenu", SceneManager.GetActiveScene().name);
            var menu = Object.FindFirstObjectByType<TestMenu>();
            Assert.IsNotNull(menu, "test menu has no TestMenu");
            Assert.GreaterOrEqual(menu.Levels.Length, 2, "test menu lists no levels");
            Assert.AreEqual(1f, Time.timeScale);
        }

        private void Teleport(Vector2 position)
        {
            _player.position = position;
            _player.transform.position = position;
        }
    }
}
