using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Navigation;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class ArenaZonenSceneTests
    {
        private const string SceneName = "Arena_Zonen";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene_HasThreeZonesAndTwoClosedGates()
        {
            yield return null;

            Assert.AreEqual(3, Object.FindObjectsByType<Zone>(FindObjectsSortMode.None).Length);
            Gate[] gates = Object.FindObjectsByType<Gate>(FindObjectsSortMode.None);
            Assert.AreEqual(2, gates.Length);
            foreach (Gate gate in gates)
            {
                Assert.AreEqual(GateState.Closed, gate.State, gate.name);
                Assert.IsNotNull(gate.Condition, $"{gate.name} has no condition");
            }
        }

        [UnityTest]
        public IEnumerator FirstZone_PlayerStartsInside_IsActive()
        {
            yield return null;

            Assert.AreEqual(ZoneState.Active, Find<Zone>("Zone_1").State);
            Assert.AreEqual(ZoneState.Inactive, Find<Zone>("Zone_2").State);
        }

        [UnityTest]
        public IEnumerator ClosedGate_BlocksPhysicsAndFlowField()
        {
            yield return null;
            Gate gate = Find<Gate>("Gate_1");
            FlowFieldNavigator navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();

            Assert.IsNotNull(Physics2D.OverlapPoint(gate.transform.position, LayerMask.GetMask("Walls")), "Gate does not block");
            Assert.IsFalse(navigator.Field.IsWalkable(navigator.CellOf(gate.transform.position)));
        }

        [UnityTest]
        public IEnumerator DebugGate_Forced_OpensFreesPassageAndCompletesZone()
        {
            yield return null;
            Gate gate = Find<Gate>("Gate_2");
            FlowFieldNavigator navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();
            bool opened = false;
            gate.Opened += _ => opened = true;

            gate.GetComponent<DebugGateCondition>().Force();
            yield return null;
            Assert.AreEqual(GateState.Opening, gate.State);
            yield return new WaitForSeconds(1.5f);

            Assert.IsTrue(opened);
            Assert.AreEqual(GateState.Open, gate.State);
            Assert.IsNull(Physics2D.OverlapPoint(gate.transform.position, LayerMask.GetMask("Walls")), "Open gate still blocks");
            Assert.IsTrue(navigator.Field.IsWalkable(navigator.CellOf(gate.transform.position)));
            Assert.AreEqual(ZoneState.Completed, Find<Zone>("Zone_2").State);
        }

        [UnityTest]
        public IEnumerator KillGate_OpensAfterRequiredKills_TrackerShowsProgress()
        {
            yield return null;
            Gate gate = Find<Gate>("Gate_1");
            var condition = gate.GetComponent<KillGateCondition>();
            Assert.IsNotNull(condition, "Gate_1 should be a kill gate");
            KillCounter counter = Object.FindFirstObjectByType<LevelKills>().Counter;
            MissionTracker tracker = Object.FindFirstObjectByType<MissionTracker>();
            Assert.AreEqual(MissionText.KillGate(0, condition.RequiredKills), tracker.Text);

            for (int i = 0; i < condition.RequiredKills - 1; i++)
            {
                counter.Register();
            }

            yield return null;
            Assert.AreEqual(GateState.Closed, gate.State);
            Assert.AreEqual(MissionText.KillGate(condition.RequiredKills - 1, condition.RequiredKills), tracker.Text);

            counter.Register();
            yield return null;
            yield return null;

            Assert.AreNotEqual(GateState.Closed, gate.State);
            Assert.AreEqual(MissionText.GateOpen, tracker.Text);
        }

        [UnityTest]
        public IEnumerator KilledEnemy_CountsTowardsLevelKills()
        {
            yield return null;
            KillCounter counter = Object.FindFirstObjectByType<LevelKills>().Counter;
            WaveDirector director = Object.FindFirstObjectByType<WaveDirector>();
            Enemy spawned = null;
            director.Spawned += (enemy, point) => spawned = enemy;
            float timeout = Time.time + 10f;
            while (Time.time < timeout && spawned == null)
            {
                yield return null;
            }

            Assert.IsNotNull(spawned, "no enemy spawned");
            ((RushOps.Combat.IDamageable)spawned).TakeHit(1000, Vector2.zero);

            Assert.AreEqual(1, counter.Count);
        }

        [UnityTest]
        public IEnumerator SecondZone_PlayerEnters_RaisesActivated()
        {
            Zone zone = Find<Zone>("Zone_2");
            Zone activated = null;
            zone.Activated += z => activated = z;

            var body = zone.Player.GetComponent<Rigidbody2D>();
            body.position = zone.transform.position;
            zone.Player.position = zone.transform.position;
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.AreSame(zone, activated);
            Assert.AreEqual(ZoneState.Active, zone.State);
        }

        [UnityTest]
        public IEnumerator SecondZone_AfterGateOpened_SpawnsOnlyFromItsOwnPoints()
        {
            yield return null;
            WaveDirector director = Object.FindFirstObjectByType<WaveDirector>();
            Zone first = Find<Zone>("Zone_1");
            Zone second = Find<Zone>("Zone_2");
            Gate gate = Find<Gate>("Gate_1");
            KillCounter counter = Object.FindFirstObjectByType<LevelKills>().Counter;
            for (int i = 0; i < gate.GetComponent<KillGateCondition>().RequiredKills; i++)
            {
                counter.Register();
            }

            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(WavePhase.Finished, director.SequenceOf(first).Phase, "waves of a completed zone stop");

            var points = new List<SpawnPoint>();
            director.Spawned += (enemy, point) => points.Add(point);
            second.Player.GetComponent<Rigidbody2D>().position = second.transform.position;
            float timeout = Time.time + 8f;
            while (Time.time < timeout && points.Count < 3)
            {
                yield return null;
            }

            Assert.GreaterOrEqual(points.Count, 3, "zone 2 did not spawn");
            CollectionAssert.IsSubsetOf(points, second.SpawnPoints);
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
