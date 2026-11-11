using System.Collections;
using NUnit.Framework;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class Level2_2SceneTests
    {
        private const string SceneName = "Level_2-2";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene_HasEightZones_SevenClosedGates_FirstZoneActive()
        {
            yield return null;

            Assert.AreEqual(8, Object.FindObjectsByType<Zone>(FindObjectsSortMode.None).Length);
            Gate[] gates = Object.FindObjectsByType<Gate>(FindObjectsSortMode.None);
            Assert.AreEqual(7, gates.Length);
            foreach (Gate gate in gates)
            {
                Assert.AreEqual(GateState.Closed, gate.State, gate.name);
            }

            Assert.AreEqual(ZoneState.Active, Find<Zone>("Zone_1").State);
            for (int i = 1; i < 8; i++)
            {
                Zone zone = Find<Zone>($"Zone_{i}");
                Assert.IsNotEmpty(zone.SpawnPoints, $"{zone.name} has no spawn points");
                Assert.IsNotNull(zone.Waves, $"{zone.name} has no waves");
            }
        }

        [UnityTest]
        public IEnumerator TunnelGate_NeedsTheGenerator_AndKillsFromEnteringTheTunnel()
        {
            yield return null;

            var hall = Find<Gate>("Gate_1").Condition as KillGateCondition;
            Assert.IsNotNull(hall, "gate 1 is a kill gate");
            Assert.IsNull(hall.CountFromZone, "counts from the level start");
            Assert.IsInstanceOf<PuzzleSolvedCondition>(Find<Gate>("Gate_2").Condition);

            // Decisions L1, L2: carry the cells while the kill gate counts (P1: from entering the tunnel).
            Gate tunnel = Find<Gate>("Gate_3");
            Assert.IsInstanceOf<GateConditionGroup>(tunnel.Condition);
            var kills = tunnel.GetComponent<KillGateCondition>();
            Assert.IsNotNull(kills, "gate 3 counts kills");
            Assert.AreSame(Find<Zone>("Zone_3"), kills.CountFromZone);
            Assert.AreEqual(0, kills.CurrentKills, "nothing counts before the player enters the tunnel");
            CollectionAssert.AreEqual(
                new Puzzle[] { Find<TransportPuzzle>("Transport_Zone_3") },
                tunnel.GetComponent<PuzzleSolvedCondition>().Puzzles);
            Assert.IsFalse(tunnel.Condition.IsMet);

            Assert.IsInstanceOf<PuzzleSolvedCondition>(Find<Gate>("Gate_4").Condition);
            Assert.IsNotNull(Find<HoldZonePuzzle>("HoldZone_Zone_4"));
            Assert.AreEqual(4, Find<TimedSwitchPuzzle>("Switches_Zone_5").Switches.Length);
            Assert.IsInstanceOf<PuzzleSolvedCondition>(Find<Gate>("Gate_5").Condition);
        }

        [UnityTest]
        public IEnumerator Gates_2And4_LeadSideways()
        {
            yield return null;

            // Zones 3 and 4 lie west of the main lane (decision L3).
            Assert.Less(Find<Zone>("Zone_3").Area.center.x, Find<Zone>("Zone_2").Area.center.x - 10f);
            Assert.Less(Find<Zone>("Zone_4").Area.center.x, Find<Zone>("Zone_5").Area.center.x - 10f);
            Assert.Less(Find<Gate>("Gate_2").transform.position.x, Find<Zone>("Zone_2").Area.xMin + 1f);
            Assert.Less(Find<Gate>("Gate_4").transform.position.x, Find<Zone>("Zone_5").Area.xMin + 1f);
        }

        [UnityTest]
        public IEnumerator ExitAndAllGoals_AreReachableForATwoByTwoBody()
        {
            yield return null;

            LevelReachability.AssertAllGoalsReachable();
        }

        [UnityTest]
        public IEnumerator LevelDefinition_IsUBahn_OnIsland2()
        {
            yield return null;

            LevelDefinition level = Object.FindAnyObjectByType<RushOps.UI.LevelResults>().Level;
            Assert.AreEqual("Level_2-2", level.SceneName);
            Assert.AreEqual("U-Bahn", level.DisplayName);
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
            yield return PuzzleZoneFlow.FinishAtExit();
        }

        [UnityTest]
        public IEnumerator Dogs_ComeOnlyInTheNewZones()
        {
            yield return null;

            PuzzleZoneFlow.AssertDogsOnlyIn(7);
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
