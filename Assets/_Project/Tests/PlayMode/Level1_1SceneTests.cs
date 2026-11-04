using System.Collections;
using NUnit.Framework;
using RushOps.Levels;
using RushOps.Puzzles;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class Level1_1SceneTests
    {
        private const string SceneName = "Level_1-1";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene_HasFiveZones_FourClosedGates_FirstZoneActive()
        {
            yield return null;

            Assert.AreEqual(5, Object.FindObjectsByType<Zone>(FindObjectsSortMode.None).Length);
            Gate[] gates = Object.FindObjectsByType<Gate>(FindObjectsSortMode.None);
            Assert.AreEqual(4, gates.Length);
            foreach (Gate gate in gates)
            {
                Assert.AreEqual(GateState.Closed, gate.State, gate.name);
            }

            Assert.AreEqual(ZoneState.Active, Find<Zone>("Zone_1").State);
            for (int i = 2; i <= 5; i++)
            {
                Zone zone = Find<Zone>($"Zone_{i}");
                Assert.AreEqual(ZoneState.Inactive, zone.State, zone.name);
                Assert.IsNotEmpty(zone.SpawnPoints, $"{zone.name} has no spawn points");
                Assert.IsNotNull(zone.Waves, $"{zone.name} has no waves");
            }
        }

        [UnityTest]
        public IEnumerator Gates_KillGateFirst_ThenOnePuzzleTypeEach()
        {
            yield return null;

            Assert.IsInstanceOf<KillGateCondition>(Find<Gate>("Gate_1").Condition);
            AssertPuzzleGate<HoldZonePuzzle>("Gate_2");
            AssertPuzzleGate<TimedSwitchPuzzle>("Gate_3");
            AssertPuzzleGate<TransportPuzzle>("Gate_4");
            Assert.IsNull(Find<Zone>("Zone_5").ExitGate, "the last zone ends at the exit, not at a gate");
        }

        [UnityTest]
        public IEnumerator Level_HasExitAndDefinition()
        {
            yield return null;

            Assert.IsNotNull(Object.FindFirstObjectByType<LevelExit>(), "no level exit");
            LevelResults results = Object.FindFirstObjectByType<LevelResults>();
            Assert.IsNotNull(results);
            Assert.AreEqual(SceneName, results.Level.SceneName);
        }

        private static void AssertPuzzleGate<T>(string gateName) where T : Puzzle
        {
            var condition = Find<Gate>(gateName).Condition as PuzzleSolvedCondition;
            Assert.IsNotNull(condition, $"{gateName} should be a puzzle gate");
            Assert.AreEqual(1, condition.Puzzles.Count, $"{gateName} should wait for one puzzle");
            Assert.IsInstanceOf<T>(condition.Puzzles[0], gateName);
        }

        private static T Find<T>(string name) where T : Component
        {
            GameObject go = GameObject.Find(name);
            Assert.IsNotNull(go, $"{name} not found in {SceneName}");
            T component = go.GetComponent<T>();
            Assert.IsNotNull(component, $"{name} has no {typeof(T).Name}");
            return component;
        }
    }
}
