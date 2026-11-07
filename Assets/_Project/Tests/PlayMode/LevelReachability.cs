using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Checks a loaded level scene against the level design rules: exit, gates, puzzle parts and spawn points must be
    /// reachable from the player for a 2 × 2 body, with all gates open.
    /// </summary>
    internal static class LevelReachability
    {
        private const int BodySize = 2;

        /// <summary>Fails the test if a goal of the loaded scene cannot be reached.</summary>
        public static void AssertAllGoalsReachable()
        {
            Tilemap walls = GameObject.Find("Walls").GetComponent<Tilemap>();
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
            AssertCovered(walls, reached, goals);
        }

        /// <summary>Fails the test if one of <paramref name="goals"/> cannot be reached from the player.</summary>
        public static void AssertReachable(IEnumerable<(string name, Vector3 position)> goals)
        {
            Tilemap walls = GameObject.Find("Walls").GetComponent<Tilemap>();
            Vector3Int start = walls.WorldToCell(GameObject.Find("Player").transform.position);
            HashSet<Vector2Int> reached = ReachableSquares(walls, (Vector2Int)start);
            Assert.IsNotEmpty(reached, "the player stands in a wall");
            AssertCovered(walls, reached, goals);
        }

        private static void AssertCovered(
            Tilemap walls, HashSet<Vector2Int> reached, IEnumerable<(string name, Vector3 position)> goals)
        {
            foreach ((string name, Vector3 position) in goals)
            {
                Vector2Int cell = (Vector2Int)walls.WorldToCell(position);
                Assert.IsTrue(Covers(reached, cell), $"{name} at {position} is not reachable for a 2 × 2 body");
            }
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
    }
}
