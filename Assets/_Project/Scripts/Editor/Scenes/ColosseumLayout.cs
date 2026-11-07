using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Floor plan of the endless arena <c>Endless_Arena</c> (issue #206, decisions E1, N2, N4): a round sand floor of
    /// about 24 cells across inside a ring wall, empty stands behind it, a round fountain in the middle, a barred gate
    /// in the north, east, south and west with spawn points in front of each, three weapon slots side by side on the
    /// north wall and the player start in the south. <c>Tools/gen_pixelart.py</c> mirrors this geometry for its preview.
    /// </summary>
    internal static class ColosseumLayout
    {
        /// <summary>Columns and rows of the scene grid, stands included.</summary>
        public const int Size = 32;

        /// <summary>Cells whose center is closer than this to <see cref="Center"/> are arena floor.</summary>
        public const float FloorRadius = 12.5f;

        /// <summary>Radius of the fountain in cells (decision N2); its cells are blocked for the flow field.</summary>
        public const float FountainRadius = 2f;

        /// <summary>Center of the arena and of the fountain in world units.</summary>
        public static readonly Vector2 Center = new Vector2(Size * 0.5f, Size * 0.5f);

        /// <summary>The player starts in the south, between the fountain and the south gate.</summary>
        public static readonly Vector2Int StartCell = new Vector2Int(15, 10);

        /// <summary>
        /// Wall cells of the three weapon slots, side by side on the straight part of the north wall, east of the gate
        /// (decision N4). The weapon is picked up from the floor cell below each.
        /// </summary>
        public static readonly Vector2Int[] WeaponSlotCells =
        {
            new Vector2Int(18, 28), new Vector2Int(19, 28), new Vector2Int(20, 28),
        };

        /// <summary>The four gates in the order north, east, south, west (decision E1).</summary>
        public static readonly ColosseumGate[] Gates =
        {
            new ColosseumGate("N", new RectInt(15, 28, 2, 4), new Vector2Int(15, 28), new Vector2Int(16, 28),
                new Vector2Int(12, 25), new Vector2Int(15, 25), new Vector2Int(19, 24)),
            new ColosseumGate("E", new RectInt(28, 15, 4, 2), new Vector2Int(28, 15), new Vector2Int(28, 16),
                new Vector2Int(25, 12), new Vector2Int(25, 16), new Vector2Int(25, 19)),
            new ColosseumGate("S", new RectInt(15, 0, 2, 4), new Vector2Int(15, 3), new Vector2Int(16, 3),
                new Vector2Int(12, 6), new Vector2Int(16, 6), new Vector2Int(19, 6)),
            new ColosseumGate("W", new RectInt(0, 15, 4, 2), new Vector2Int(3, 15), new Vector2Int(3, 16),
                new Vector2Int(6, 12), new Vector2Int(6, 15), new Vector2Int(6, 19)),
        };

        private static ArenaLayout _layout;

        /// <summary>
        /// The floor plan for <see cref="ArenaTestSceneBuilder.BuildScene"/>: ring wall and stands are outer walls, the
        /// fountain is the only obstacle.
        /// </summary>
        public static ArenaLayout Layout => _layout ??= new ArenaLayout(
            Size, Size, StartCell, Runs(ColosseumCell.Fountain), Runs(ColosseumCell.Wall, ColosseumCell.Stands));

        /// <summary>Smallest cell rectangle around the floor.</summary>
        public static RectInt FloorBounds
        {
            get
            {
                int min = Size, max = -1;
                for (int i = 0; i < Size; i++)
                {
                    if (CellAt(i, Size / 2) != ColosseumCell.Stands && CellAt(i, Size / 2) != ColosseumCell.Wall)
                    {
                        min = Mathf.Min(min, i);
                        max = Mathf.Max(max, i);
                    }
                }

                return new RectInt(min, min, max - min + 1, max - min + 1);
            }
        }

        /// <summary>What the cell (x, y) is.</summary>
        public static ColosseumCell CellAt(int x, int y)
        {
            float distance = DistanceToCenter(x, y);
            if (distance < FountainRadius)
            {
                return ColosseumCell.Fountain;
            }

            if (distance < FloorRadius)
            {
                return ColosseumCell.Floor;
            }

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (DistanceToCenter(x + dx, y + dy) < FloorRadius)
                    {
                        return ColosseumCell.Wall;
                    }
                }
            }

            return ColosseumCell.Stands;
        }

        /// <summary>Distance of the center of cell (x, y) to the arena center.</summary>
        public static float DistanceToCenter(int x, int y)
        {
            return Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), Center);
        }

        /// <summary>All spawn points of the four gates, in gate order.</summary>
        public static Vector2Int[] AllSpawnCells()
        {
            var cells = new List<Vector2Int>();
            foreach (ColosseumGate gate in Gates)
            {
                cells.AddRange(gate.SpawnCells);
            }

            return cells.ToArray();
        }

        /// <summary>
        /// Checks the floor plan against the level design rules (issue #206): spawn points with free cells around
        /// them, gates in the ring wall, three weapon slots side by side on a wall face, and spawn points, weapon slots
        /// and the whole floor around the fountain reachable for a 2 × 2 body from the start. Returns one line per
        /// broken rule, empty if all hold.
        /// </summary>
        public static List<string> Validate()
        {
            var problems = new List<string>();
            ArenaLayout layout = Layout;
            int size = LevelLayoutCheck.MinPassage;
            bool[,] reached = LevelLayoutCheck.ReachableSquares(layout, StartCell, size);

            if (CellAt(StartCell.x, StartCell.y) != ColosseumCell.Floor || StartCell.y + 0.5f >= Center.y - FountainRadius)
            {
                problems.Add($"start {StartCell} is not on the floor south of the fountain");
            }

            foreach (ColosseumGate gate in Gates)
            {
                if (gate.SpawnCells.Length < 2 || gate.SpawnCells.Length > 3)
                {
                    problems.Add($"gate {gate.Name} has {gate.SpawnCells.Length} spawn points, 2 to 3 needed");
                }

                foreach (Vector2Int cell in gate.RingCells)
                {
                    if (CellAt(cell.x, cell.y) != ColosseumCell.Wall || !gate.Area.Contains(cell))
                    {
                        problems.Add($"gate {gate.Name}: {cell} is not a ring wall cell of the gate");
                    }
                }

                foreach (Vector2Int cell in gate.SpawnCells)
                {
                    if (!LevelLayoutCheck.HasClearance(layout, cell, 1))
                    {
                        problems.Add($"gate {gate.Name}: spawn point {cell} has a wall within one cell");
                    }

                    if (!LevelLayoutCheck.Covers(reached, size, cell))
                    {
                        problems.Add($"gate {gate.Name}: spawn point {cell} is not reachable for a {size} × {size} body");
                    }

                    if (Vector2.Distance(cell + Vector2.one * 0.5f, gate.Center) > Vector2.Distance(cell + Vector2.one * 0.5f, Center))
                    {
                        problems.Add($"gate {gate.Name}: spawn point {cell} is nearer to the center than to its gate");
                    }
                }
            }

            for (int i = 0; i < WeaponSlotCells.Length; i++)
            {
                Vector2Int cell = WeaponSlotCells[i];
                var slot = new LevelWallWeapon(cell, $"slot_{i + 1}");
                LevelBlueprintBuilder.ValidateWallWeapon(problems, layout, reached, size, slot);
                if (i > 0 && cell != WeaponSlotCells[i - 1] + Vector2Int.right)
                {
                    problems.Add($"weapon slot {cell} is not right next to {WeaponSlotCells[i - 1]}");
                }

                foreach (ColosseumGate gate in Gates)
                {
                    if (gate.Area.Contains(cell))
                    {
                        problems.Add($"weapon slot {cell} lies in gate {gate.Name}");
                    }
                }
            }

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (CellAt(x, y) == ColosseumCell.Floor && !LevelLayoutCheck.Covers(reached, size, new Vector2Int(x, y)))
                    {
                        problems.Add($"floor cell ({x}, {y}) is not reachable for a {size} × {size} body");
                    }
                }
            }

            return problems;
        }

        // One rectangle per row and run of neighbouring cells of the given kinds.
        private static RectInt[] Runs(params ColosseumCell[] kinds)
        {
            var runs = new List<RectInt>();
            for (int y = 0; y < Size; y++)
            {
                int start = -1;
                for (int x = 0; x <= Size; x++)
                {
                    bool match = x < Size && System.Array.IndexOf(kinds, CellAt(x, y)) >= 0;
                    if (match && start < 0)
                    {
                        start = x;
                    }
                    else if (!match && start >= 0)
                    {
                        runs.Add(new RectInt(start, y, x - start, 1));
                        start = -1;
                    }
                }
            }

            return runs.ToArray();
        }
    }
}
