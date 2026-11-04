using System.Collections;
using NUnit.Framework;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class Level1_2SceneTests
    {
        private const string SceneName = "Level_1-2";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
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
            for (int i = 1; i <= 5; i++)
            {
                Zone zone = Find<Zone>($"Zone_{i}");
                Assert.IsNotEmpty(zone.SpawnPoints, $"{zone.name} has no spawn points");
                Assert.IsNotNull(zone.Waves, $"{zone.name} has no waves");
            }
        }

        [UnityTest]
        public IEnumerator Gates_HoldSwitchesZoneKillsTransport()
        {
            yield return null;

            Assert.IsInstanceOf<PuzzleSolvedCondition>(Find<Gate>("Gate_1").Condition);
            Assert.IsInstanceOf<PuzzleSolvedCondition>(Find<Gate>("Gate_2").Condition);
            var kills = Find<Gate>("Gate_3").Condition as KillGateCondition;
            Assert.IsNotNull(kills, "gate 3 is a kill gate");
            Assert.AreSame(Find<Zone>("Zone_3"), kills.CountFromZone, "counts from entering the Strandbar (P1)");
            Assert.AreEqual(0, kills.CurrentKills, "nothing counts before the player enters zone 3");
            Assert.IsInstanceOf<PuzzleSolvedCondition>(Find<Gate>("Gate_4").Condition);
            Assert.AreEqual(4, Find<TimedSwitchPuzzle>("Switches_Zone_2").Switches.Length);
            Assert.IsNotNull(Find<HoldZonePuzzle>("HoldZone_Zone_1"));
            Assert.IsNotNull(Find<TransportPuzzle>("Transport_Zone_4"));
        }

        [UnityTest]
        public IEnumerator LevelDefinition_IsStrandpromenade()
        {
            yield return null;

            LevelDefinition level = Object.FindAnyObjectByType<RushOps.UI.LevelResults>().Level;
            Assert.AreEqual("Level_1-2", level.SceneName);
            Assert.AreEqual("Strandpromenade", level.DisplayName);
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
