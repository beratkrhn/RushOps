using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Levels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class Level1_3SceneTests
    {
        private const string SceneName = "Level_1-3";

        private Colossus _koloss;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _koloss = Object.FindAnyObjectByType<Colossus>();
            Assert.IsNotNull(_koloss, "no Koloss in level 1-3");
        }

        [UnityTearDown]
        public IEnumerator Unfreeze()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene_HasTwoZonesBeforeTheArena_AndTheKoloss()
        {
            yield return null;

            Assert.AreEqual(3, Object.FindObjectsByType<Zone>(FindObjectsSortMode.None).Length);
            Assert.AreEqual(2, Object.FindObjectsByType<Gate>(FindObjectsSortMode.None).Length);
            Assert.IsNull(Object.FindAnyObjectByType<LevelExit>(), "a boss level ends with the boss");
            Assert.IsInstanceOf<ColossusDefinition>(_koloss.Definition);
            Assert.AreEqual("Koloss", _koloss.Definition.DisplayName);
            Assert.AreEqual(1000, _koloss.Definition.Points);
            Assert.AreEqual(1, _koloss.Definition.ContactDamage, "decision C7: the charge costs 1 heart");
            Assert.AreEqual("Leuchtturm", Object.FindAnyObjectByType<RushOps.UI.LevelResults>().Level.DisplayName);
        }

        [UnityTest]
        public IEnumerator Koloss_ChargesUpWithLine_RushesIntoTheWall_AndIsDazed()
        {
            // Stand at the west wall level with the Koloss, so its rush goes west into the outer wall.
            Teleport(new Vector2(4f, _koloss.transform.position.y));
            yield return null;
            yield return null;
            var fight = Object.FindAnyObjectByType<BossFight>();
            Assert.IsTrue(fight.IsRunning);

            yield return WaitFor(() => _koloss.Phase == ColossusPhase.Windup, 5f, "no charge up");
            Assert.IsTrue(_koloss.ShowsLine, "decision L7: line on the floor while charging up");
            Assert.Less(_koloss.ChargeDirection.x, -0.8f, "charges at the player");
            Teleport(new Vector2(4f, _koloss.transform.position.y + 6f));

            yield return WaitFor(() => _koloss.Phase == ColossusPhase.Charge, 2f, "no rush");
            Assert.IsFalse(_koloss.ShowsLine);
            yield return WaitFor(() => _koloss.Phase == ColossusPhase.Stunned, 3f, "the wall did not stop the rush");
            Assert.Less(_koloss.transform.position.x, 6f, "it rushed to the west wall");
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

        private static void Teleport(Vector2 position)
        {
            var body = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
        }
    }
}
