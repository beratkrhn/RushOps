using System.Collections;
using NUnit.Framework;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class Level3_1SceneTests
    {
        private const string SceneName = "Level_3-1";

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
        public IEnumerator Gates_KillsThenSwitchesHoldAndSwitchesThenTransportWithZoneKills()
        {
            yield return null;

            var asche = Find<Gate>("Gate_1").Condition as KillGateCondition;
            Assert.IsNotNull(asche, "gate 1 is a kill gate");
            Assert.IsNull(asche.CountFromZone, "counts from the level start");
            Assert.IsInstanceOf<PuzzleSolvedCondition>(Find<Gate>("Gate_2").Condition);

            var schlot = Find<Gate>("Gate_3").GetComponent<PuzzleSolvedCondition>();
            CollectionAssert.AreEqual(
                new Puzzle[] { Find<HoldZonePuzzle>("HoldZone_Zone_3"), Find<TimedSwitchPuzzle>("Switches_Zone_3") },
                schlot.Puzzles,
                "decision L2: hold zone and switches at one gate");

            Gate grat = Find<Gate>("Gate_4");
            Assert.IsInstanceOf<GateConditionGroup>(grat.Condition);
            Assert.AreSame(Find<Zone>("Zone_4"), grat.GetComponent<KillGateCondition>().CountFromZone);
            Assert.AreEqual(4, Find<TimedSwitchPuzzle>("Switches_Zone_5").Switches.Length);
        }

        [UnityTest]
        public IEnumerator ExitAndAllGoals_AreReachableForATwoByTwoBody()
        {
            yield return null;

            LevelReachability.AssertAllGoalsReachable();
        }

        [UnityTest]
        public IEnumerator LevelDefinition_IsAschefeld_OnIsland3()
        {
            yield return null;

            LevelDefinition level = Object.FindAnyObjectByType<RushOps.UI.LevelResults>().Level;
            Assert.AreEqual("Level_3-1", level.SceneName);
            Assert.AreEqual("Aschefeld", level.DisplayName);
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
