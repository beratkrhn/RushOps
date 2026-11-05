using System.Collections;
using NUnit.Framework;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class Level2_1SceneTests
    {
        private const string SceneName = "Level_2-1";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene_HasSixZones_FiveClosedGates_FirstZoneActive()
        {
            yield return null;

            Assert.AreEqual(6, Object.FindObjectsByType<Zone>(FindObjectsSortMode.None).Length);
            Gate[] gates = Object.FindObjectsByType<Gate>(FindObjectsSortMode.None);
            Assert.AreEqual(5, gates.Length);
            foreach (Gate gate in gates)
            {
                Assert.AreEqual(GateState.Closed, gate.State, gate.name);
            }

            Assert.AreEqual(ZoneState.Active, Find<Zone>("Zone_1").State);
            for (int i = 1; i <= 6; i++)
            {
                Zone zone = Find<Zone>($"Zone_{i}");
                Assert.IsNotEmpty(zone.SpawnPoints, $"{zone.name} has no spawn points");
                Assert.IsNotNull(zone.Waves, $"{zone.name} has no waves");
            }
        }

        [UnityTest]
        public IEnumerator Gates_KillsSwitchesHoldAndSwitchesTransportZoneKills()
        {
            yield return null;

            var hof = Find<Gate>("Gate_1").Condition as KillGateCondition;
            Assert.IsNotNull(hof, "gate 1 is a kill gate");
            Assert.IsNull(hof.CountFromZone, "counts from the level start");
            Assert.IsInstanceOf<PuzzleSolvedCondition>(Find<Gate>("Gate_2").Condition);

            var garage = Find<Gate>("Gate_3").GetComponent<PuzzleSolvedCondition>();
            CollectionAssert.AreEqual(
                new Puzzle[] { Find<HoldZonePuzzle>("HoldZone_Zone_3"), Find<TimedSwitchPuzzle>("Switches_Zone_3") },
                garage.Puzzles,
                "decisions L1, L2: hold zone and switches at one gate");
            Assert.AreSame(Find<Zone>("Zone_3"), garage.Puzzles[0].Zone);
            Assert.AreSame(Find<Zone>("Zone_3"), garage.Puzzles[1].Zone);

            Assert.IsInstanceOf<PuzzleSolvedCondition>(Find<Gate>("Gate_4").Condition);
            Assert.IsNotNull(Find<TransportPuzzle>("Transport_Zone_4"));
            var hinterhaus = Find<Gate>("Gate_5").Condition as KillGateCondition;
            Assert.IsNotNull(hinterhaus, "gate 5 is a kill gate");
            Assert.AreSame(Find<Zone>("Zone_5"), hinterhaus.CountFromZone, "counts from entering the Hinterhaus (P1)");
        }

        [UnityTest]
        public IEnumerator Gates_2And4_LeadSideways()
        {
            yield return null;

            // Zones 3 and 4 lie east of the main lane (decision L3).
            Vector2 zone2 = Find<Zone>("Zone_2").Area.center;
            Vector2 zone3 = Find<Zone>("Zone_3").Area.center;
            Vector2 zone4 = Find<Zone>("Zone_4").Area.center;
            Vector2 zone5 = Find<Zone>("Zone_5").Area.center;
            Assert.Greater(zone3.x, zone2.x + 10f);
            Assert.Greater(zone4.x, zone5.x + 10f);
            Assert.Greater(Find<Gate>("Gate_2").transform.position.x, Find<Zone>("Zone_2").Area.xMax - 1f);
            Assert.Greater(Find<Gate>("Gate_4").transform.position.x, Find<Zone>("Zone_5").Area.xMax - 1f);
        }

        [UnityTest]
        public IEnumerator ExitAndAllGoals_AreReachableForATwoByTwoBody()
        {
            yield return null;

            LevelReachability.AssertAllGoalsReachable();
        }

        [UnityTest]
        public IEnumerator LevelDefinition_IsHinterhoefe_OnIsland2()
        {
            yield return null;

            LevelDefinition level = Object.FindAnyObjectByType<RushOps.UI.LevelResults>().Level;
            Assert.AreEqual("Level_2-1", level.SceneName);
            Assert.AreEqual("Hinterhöfe", level.DisplayName);
            Assert.Greater(level.ThreeStarScore, level.TwoStarScore);
        }

        private static T Find<T>(string name) where T : Component
        {
            GameObject go = GameObject.Find(name);
            Assert.IsNotNull(go, $"{name} not found");
            T component = go.GetComponent<T>();
            Assert.IsNotNull(component, $"{name} has no {typeof(T).Name}");
            return component;
        }
    }
}
