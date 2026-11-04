using System.Collections;
using NUnit.Framework;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Player;
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

        [UnityTest]
        public IEnumerator Exit_LockedUntilLastWavesBeaten_TrackerLeadsToIt()
        {
            yield return null;
            LevelExit exit = Object.FindFirstObjectByType<LevelExit>();
            LevelResults results = Object.FindFirstObjectByType<LevelResults>();
            MissionTracker tracker = Object.FindFirstObjectByType<MissionTracker>();
            WaveDirector director = Object.FindFirstObjectByType<WaveDirector>();
            Zone last = Find<Zone>("Zone_5");
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            Assert.AreSame(last, exit.Zone);
            Assert.IsFalse(exit.IsOpen, "exit is open before the last zone was played");
            SpriteRenderer exitRenderer = exit.GetComponent<SpriteRenderer>();
            Assert.AreEqual("Exit_Pad_Locked", exitRenderer.sprite.name, "locked exit does not show its locked sprite");

            Teleport(exit.transform.position);
            yield return new WaitForFixedUpdate();
            yield return null;
            yield return null;

            Assert.AreEqual(ZoneState.Active, last.State);
            Assert.IsFalse(results.ShowsDone, "locked exit finished the level");
            Assert.AreEqual(MissionText.ClearZone, tracker.Text);

            director.StopWaves(last);
            foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                ((IDamageable)enemy).TakeHit(1000, Vector2.zero);
            }

            float timeout = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < timeout && !results.ShowsDone)
            {
                yield return null;
            }

            Assert.IsTrue(exit.IsOpen, "exit did not open after the last waves");
            Assert.AreEqual("Exit_Pad", exitRenderer.sprite.name, "open exit still shows the locked sprite");
            Assert.IsTrue(results.ShowsDone, "player standing on the exit did not finish once it opened");
        }

        [UnityTest]
        public IEnumerator Tracker_InLastZone_ShowsToExitOnceOpen()
        {
            yield return null;
            LevelExit exit = Object.FindFirstObjectByType<LevelExit>();
            MissionTracker tracker = Object.FindFirstObjectByType<MissionTracker>();
            WaveDirector director = Object.FindFirstObjectByType<WaveDirector>();
            Zone last = Find<Zone>("Zone_5");
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;

            Teleport(last.transform.position);
            yield return new WaitForFixedUpdate();
            yield return null;
            director.StopWaves(last);
            foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                ((IDamageable)enemy).TakeHit(1000, Vector2.zero);
            }

            yield return null;
            yield return null;

            Assert.IsTrue(exit.IsOpen);
            Assert.AreEqual(MissionText.ToExit, tracker.Text);
        }

        private static void Teleport(Vector2 position)
        {
            var body = Object.FindFirstObjectByType<PlayerHealth>().GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
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
