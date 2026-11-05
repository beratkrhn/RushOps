using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Scoring;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class Level2_3SceneTests
    {
        private const string SceneName = "Level_2-3";
        private static readonly Vector2 ArenaSpot = new Vector2(14f, 45f);

        private BroodMother _mother;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _mother = Object.FindAnyObjectByType<BroodMother>();
            Assert.IsNotNull(_mother, "no Brutmutter in level 2-3");
        }

        [UnityTearDown]
        public IEnumerator Unfreeze()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene_HasTwoZonesBeforeTheArena_AndTheBrutmutter()
        {
            yield return null;

            Assert.AreEqual(3, Object.FindObjectsByType<Zone>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(2, Object.FindObjectsByType<Gate>(FindObjectsSortMode.None).Length);
            Assert.IsNull(Object.FindAnyObjectByType<LevelExit>(), "a boss level ends with the boss");
            Assert.IsInstanceOf<BroodMotherDefinition>(_mother.Definition);
            Assert.AreEqual("Brutmutter", _mother.Definition.DisplayName);
            Assert.AreEqual(1000, _mother.Definition.Points, "decision C10");
            var values = (BroodMotherDefinition)_mother.Definition;
            Assert.AreEqual(3f, values.SpawnInterval, "decision C8");
            Assert.AreEqual(2f, values.EnragedSpawnInterval, "decision C8");
            Assert.AreEqual("Kanalisation", Object.FindAnyObjectByType<RushOps.UI.LevelResults>().Level.DisplayName);
        }

        [UnityTest]
        public IEnumerator Brutmutter_GivesBirthToRunners_ThatGivePoints()
        {
            Enemy born = null;
            _mother.Birthed += (_, runner) => born = runner;
            yield return EnterArena();

            yield return WaitFor(() => born != null, 4f, "no runner after the spawn interval");
            Assert.AreEqual("Runner", born.Definition.name);
            Assert.Less(Vector2.Distance(born.transform.position, _mother.transform.position), 2f, "comes out of her");

            ScoreKeeper score = Object.FindAnyObjectByType<LevelScore>().Keeper;
            int before = score.Score;
            born.TakeHit(99, Vector2.up);
            yield return null;
            Assert.Greater(score.Score, before, "her runners give points");
        }

        [UnityTest]
        public IEnumerator Brutmutter_Waits_WhileTenEnemiesAreAlive()
        {
            var director = Object.FindAnyObjectByType<WaveDirector>();
            yield return EnterArena();

            // F-GEG-03, decision C8: her runners count towards the cap of 10. Fill it far behind the closed gates.
            EnemyPool runners = GameObject.Find("EnemyPool_Runner").GetComponent<EnemyPool>();
            while (director.FreeSlots > 0)
            {
                Assert.IsNotNull(runners.Spawn(new Vector2(14f, 5f)));
            }

            yield return new WaitForSeconds(4f);
            Assert.AreEqual(0, _mother.BornCount, "no birth while the level is full");
            Assert.LessOrEqual(director.AliveCount, director.Config.MaxActive);
        }

        private IEnumerator EnterArena()
        {
            var body = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            body.position = ArenaSpot;
            body.transform.position = ArenaSpot;
            yield return null;
            yield return null;
            Assert.IsTrue(Object.FindAnyObjectByType<BossFight>().IsRunning, "the fight did not start");
        }

        private static IEnumerator WaitFor(System.Func<bool> condition, float seconds, string message)
        {
            float waited = 0f;
            while (!condition() && waited < seconds)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(condition(), message);
        }
    }
}
