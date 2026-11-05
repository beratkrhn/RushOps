using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

namespace RushOps.Tests.PlayMode
{
    public class Level2_1SceneTests
    {
        private const string SceneName = "Level_2-1";

        // Body of the level design rules: every goal must be reachable for a 2 × 2 square.
        private const int BodySize = 2;

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

            Tilemap walls = Find<Tilemap>("Walls");
            Vector3Int start = walls.WorldToCell(GameObject.Find("Player").transform.position);
            HashSet<Vector2Int> reached = ReachableSquares(walls, (Vector2Int)start);
            Assert.IsNotEmpty(reached, "the player stands in a wall");

            var goals = new List<(string name, Vector3 position)>();
            goals.Add(("exit", Object.FindAnyObjectByType<LevelExit>().transform.position));
            foreach (Gate gate in Object.FindObjectsByType<Gate>(FindObjectsSortMode.None))
            {
                goals.Add((gate.name, gate.transform.position));
            }

            foreach (FloorSwitch plate in Object.FindObjectsByType<FloorSwitch>(FindObjectsSortMode.None))
            {
                goals.Add((plate.name, plate.transform.position));
            }

            foreach (HoldZonePuzzle hold in Object.FindObjectsByType<HoldZonePuzzle>(FindObjectsSortMode.None))
            {
                goals.Add((hold.name, hold.transform.position));
            }

            foreach (Generator generator in Object.FindObjectsByType<Generator>(FindObjectsSortMode.None))
            {
                goals.Add((generator.name, generator.transform.position));
            }

            foreach (EnergyCell cell in Object.FindObjectsByType<EnergyCell>(FindObjectsSortMode.None))
            {
                goals.Add((cell.name, cell.transform.position));
            }

            foreach (Zone zone in Object.FindObjectsByType<Zone>(FindObjectsSortMode.None))
            {
                foreach (SpawnPoint point in zone.SpawnPoints)
                {
                    goals.Add(($"{zone.name}/{point.name}", point.transform.position));
                }
            }

            Assert.Greater(goals.Count, 20);
            foreach ((string name, Vector3 position) in goals)
            {
                Vector2Int cell = (Vector2Int)walls.WorldToCell(position);
                Assert.IsTrue(Covers(reached, cell), $"{name} at {position} is not reachable for a 2 × 2 body");
            }
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

        // Lower left cells of all 2 × 2 squares free of wall tiles that a square from the start can slide to; gates
        // are not wall tiles, so they count as open.
        private static HashSet<Vector2Int> ReachableSquares(Tilemap walls, Vector2Int start)
        {
            var reached = new HashSet<Vector2Int>();
            var open = new Queue<Vector2Int>();
            for (int dy = -BodySize + 1; dy <= 0; dy++)
            {
                for (int dx = -BodySize + 1; dx <= 0; dx++)
                {
                    var corner = new Vector2Int(start.x + dx, start.y + dy);
                    if (IsFree(walls, corner) && reached.Add(corner))
                    {
                        open.Enqueue(corner);
                    }
                }
            }

            var steps = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            while (open.Count > 0)
            {
                Vector2Int corner = open.Dequeue();
                foreach (Vector2Int step in steps)
                {
                    Vector2Int next = corner + step;
                    if (!reached.Contains(next) && IsFree(walls, next))
                    {
                        reached.Add(next);
                        open.Enqueue(next);
                    }
                }
            }

            return reached;
        }

        private static bool IsFree(Tilemap walls, Vector2Int corner)
        {
            BoundsInt bounds = walls.cellBounds;
            for (int y = corner.y; y < corner.y + BodySize; y++)
            {
                for (int x = corner.x; x < corner.x + BodySize; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (!bounds.Contains(cell) || walls.HasTile(cell))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // True if a reachable square covers the cell.
        private static bool Covers(HashSet<Vector2Int> reached, Vector2Int cell)
        {
            for (int dy = -BodySize + 1; dy <= 0; dy++)
            {
                for (int dx = -BodySize + 1; dx <= 0; dx++)
                {
                    if (reached.Contains(new Vector2Int(cell.x + dx, cell.y + dy)))
                    {
                        return true;
                    }
                }
            }

            return false;
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
