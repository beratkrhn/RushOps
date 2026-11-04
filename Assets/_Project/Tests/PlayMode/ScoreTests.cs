using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Scoring;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class ScoreTests
    {
        private const string SceneName = "Arena_Test";
        // Free floor along the east wall of Arena_Test, far from the player's start.
        private static readonly Vector2 Spot = new Vector2(34.5f, 18.5f);

        private LevelScore _score;
        private ScoreView _view;
        private PlayerHealth _player;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _score = Object.FindFirstObjectByType<LevelScore>();
            _view = Object.FindFirstObjectByType<ScoreView>();
            Assert.IsNotNull(_score, "Arena_Test has no LevelScore");
            Assert.IsNotNull(_view, "Arena_Test has no ScoreView");
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
            _player.GetComponent<PlayerWeapon>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void Hud_StartsAtZeroTimesOne()
        {
            Assert.AreEqual("0", _view.ScoreTextShown);
            Assert.AreEqual("×1", _view.MultiplierTextShown);
        }

        [UnityTest]
        public IEnumerator Kill_AddsPointsOfEnemyType_AndHudShowsThem()
        {
            Enemy brute = Place(EnemyPools.Brute, Spot);
            yield return null;

            brute.Kill();
            yield return null;

            int points = EnemyPools.Of(EnemyPools.Brute).Definition.Points;
            Assert.Greater(points, 0);
            Assert.AreEqual(points, _score.Keeper.Score);
            Assert.AreEqual(points.ToString(), _view.ScoreTextShown);
        }

        [UnityTest]
        public IEnumerator FiveQuickKills_RaiseMultiplier_HudLightsUp()
        {
            for (int i = 0; i < 5; i++)
            {
                Place(EnemyPools.Walker, Spot + new Vector2(0f, i)).Kill();
                yield return null;
            }

            Assert.AreEqual(2, _score.Keeper.Multiplier);
            Assert.AreEqual("×2", _view.MultiplierTextShown);
            Assert.IsTrue(_view.IsFlashing, "multiplier did not light up on its rise");
        }

        [UnityTest]
        public IEnumerator PlayerHit_ResetsMultiplier()
        {
            for (int i = 0; i < 5; i++)
            {
                Place(EnemyPools.Walker, Spot + new Vector2(0f, i)).Kill();
            }

            yield return null;
            Assert.AreEqual(2, _score.Keeper.Multiplier);

            Assert.IsTrue(_player.TakeDamage(1));
            yield return null;

            Assert.AreEqual(1, _score.Keeper.Multiplier);
            Assert.AreEqual("×1", _view.MultiplierTextShown);
        }

        [UnityTest]
        public IEnumerator ComboWindow_RunsOut_ResetsMultiplier()
        {
            for (int i = 0; i < 5; i++)
            {
                Place(EnemyPools.Walker, Spot + new Vector2(0f, i)).Kill();
            }

            yield return null;
            Assert.AreEqual(2, _score.Keeper.Multiplier);

            yield return new WaitForSeconds(2.2f);

            Assert.AreEqual(1, _score.Keeper.Multiplier);
        }

        private static Enemy Place(string type, Vector2 position)
        {
            Enemy enemy = EnemyPools.Of(type).Spawn(position);
            enemy.GetComponent<EnemyMovement>().Stop();
            return enemy;
        }
    }
}
