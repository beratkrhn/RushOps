using System.Collections;
using NUnit.Framework;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class Level3_2SceneTests
    {
        private const string SceneName = "Level_3-2";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene_HasNineZones_EightClosedGates_FirstZoneActive()
        {
            yield return null;

            Assert.AreEqual(9, Object.FindObjectsByType<Zone>(FindObjectsSortMode.None).Length);
            Gate[] gates = Object.FindObjectsByType<Gate>(FindObjectsSortMode.None);
            Assert.AreEqual(8, gates.Length);
            foreach (Gate gate in gates)
            {
                Assert.AreEqual(GateState.Closed, gate.State, gate.name);
            }

            Assert.AreEqual(ZoneState.Active, Find<Zone>("Zone_1").State);
            for (int i = 1; i < 9; i++)
            {
                Zone zone = Find<Zone>($"Zone_{i}");
                Assert.IsNotEmpty(zone.SpawnPoints, $"{zone.name} has no spawn points");
                Assert.IsNotNull(zone.Waves, $"{zone.name} has no waves");
            }
        }

        [UnityTest]
        public IEnumerator Gates_CombineTwoPuzzles_TwiceAndZigzagSideToSide()
        {
            yield return null;

            var fall = Find<Gate>("Gate_2").GetComponent<PuzzleSolvedCondition>();
            CollectionAssert.AreEqual(
                new Puzzle[] { Find<HoldZonePuzzle>("HoldZone_Zone_2"), Find<TimedSwitchPuzzle>("Switches_Zone_2") },
                fall.Puzzles);
            var kammer = Find<Gate>("Gate_5").GetComponent<PuzzleSolvedCondition>();
            CollectionAssert.AreEqual(
                new Puzzle[] { Find<HoldZonePuzzle>("HoldZone_Zone_5"), Find<TransportPuzzle>("Transport_Zone_5") },
                kammer.Puzzles);

            Gate kessel = Find<Gate>("Gate_3");
            Assert.IsInstanceOf<GateConditionGroup>(kessel.Condition);
            Assert.AreSame(Find<Zone>("Zone_3"), kessel.GetComponent<KillGateCondition>().CountFromZone);

            // The gates alternate between the east and the west side.
            Assert.Greater(Find<Gate>("Gate_1").transform.position.x, 18f);
            Assert.Less(Find<Gate>("Gate_2").transform.position.x, 10f);
            Assert.Greater(Find<Gate>("Gate_3").transform.position.x, 18f);
            Assert.Less(Find<Gate>("Gate_4").transform.position.x, 10f);
        }

        [UnityTest]
        public IEnumerator ExitAndAllGoals_AreReachableForATwoByTwoBody()
        {
            yield return null;

            LevelReachability.AssertAllGoalsReachable();
        }

        [UnityTest]
        public IEnumerator LevelDefinition_IsLavastrom_OnIsland3()
        {
            yield return null;

            LevelDefinition level = Object.FindAnyObjectByType<RushOps.UI.LevelResults>().Level;
            Assert.AreEqual("Level_3-2", level.SceneName);
            Assert.AreEqual("Lavastrom", level.DisplayName);
            Assert.Greater(level.ThreeStarScore, level.TwoStarScore);
        }

        [UnityTest]
        public IEnumerator NewZones_RushThenZombiePuzzle_LevelEndsAtTheExit()
        {
            yield return null;
            PuzzleZoneFlow.PrepareRun();

            // Issue #291: the last rush still has to be beaten, then the new zone's puzzle opens the way to the exit.
            yield return PuzzleZoneFlow.BeatRush(6);
            yield return PuzzleZoneFlow.SolvePuzzleZone(7);
            yield return PuzzleZoneFlow.SolvePuzzleZone(8);
            yield return PuzzleZoneFlow.FinishAtExit();
        }

        [UnityTest]
        public IEnumerator Dogs_ComeOnlyInTheNewZones()
        {
            yield return null;

            PuzzleZoneFlow.AssertDogsOnlyIn(7, 8);
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
