using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Checks an <see cref="ArenaLayout"/> against the level design rules in <c>docs/level-design.md</c> (issue #36):
    /// passages at least two cells wide, spawn points with free cells around them and puzzles in sight of the zone
    /// entry. Level builders run these checks before saving a scene.
    /// </summary>
    internal static class LevelLayoutCheck
    {
        /// <summary>Minimum width of passages in cells.</summary>
        public const int MinPassage = 2;

        /// <summary>True if <paramref name="cell"/> and all cells up to <paramref name="margin"/> around it are floor.</summary>
        public static bool HasClearance(ArenaLayout layout, Vector2Int cell, int margin)
        {
            for (int y = cell.y - margin; y <= cell.y + margin; y++)
            {
                for (int x = cell.x - margin; x <= cell.x + margin; x++)
                {
                    if (layout.IsWall(x, y))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Squares of <paramref name="size"/> x <paramref name="size"/> floor cells that a body of that size reaches from
        /// <paramref name="start"/>, moving one cell at a time. Index [x, y] is the square with its lower left cell at (x, y).
        /// </summary>
        public static bool[,] ReachableSquares(ArenaLayout layout, Vector2Int start, int size)
        {
            var reached = new bool[layout.Width, layout.Height];
            var open = new Queue<Vector2Int>();
            for (int y = start.y - size + 1; y <= start.y; y++)
            {
                for (int x = start.x - size + 1; x <= start.x; x++)
                {
                    TryReach(layout, size, reached, open, new Vector2Int(x, y));
                }
            }

            while (open.Count > 0)
            {
                Vector2Int square = open.Dequeue();
                TryReach(layout, size, reached, open, square + Vector2Int.right);
                TryReach(layout, size, reached, open, square + Vector2Int.left);
                TryReach(layout, size, reached, open, square + Vector2Int.up);
                TryReach(layout, size, reached, open, square + Vector2Int.down);
            }

            return reached;
        }

        /// <summary>True if one of the reached squares of <paramref name="size"/> covers <paramref name="cell"/>.</summary>
        public static bool Covers(bool[,] reached, int size, Vector2Int cell)
        {
            for (int y = cell.y - size + 1; y <= cell.y; y++)
            {
                for (int x = cell.x - size + 1; x <= cell.x; x++)
                {
                    if (x >= 0 && y >= 0 && x < reached.GetLength(0) && y < reached.GetLength(1) && reached[x, y])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>True if no wall cell lies on the straight line between the world points <paramref name="from"/> and <paramref name="to"/>.</summary>
        public static bool HasLineOfSight(ArenaLayout layout, Vector2 from, Vector2 to)
        {
            const float step = 0.1f;
            float length = Vector2.Distance(from, to);
            int steps = Mathf.Max(1, Mathf.CeilToInt(length / step));
            for (int i = 0; i <= steps; i++)
            {
                Vector2 point = Vector2.Lerp(from, to, i / (float)steps);
                if (layout.IsWall(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y)))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Cell that contains the world point <paramref name="point"/>.</summary>
        public static Vector2Int CellOf(Vector2 point)
        {
            return new Vector2Int(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y));
        }

        private static void TryReach(ArenaLayout layout, int size, bool[,] reached, Queue<Vector2Int> open, Vector2Int square)
        {
            if (square.x < 0 || square.y < 0 || square.x >= layout.Width || square.y >= layout.Height || reached[square.x, square.y])
            {
                return;
            }

            for (int y = square.y; y < square.y + size; y++)
            {
                for (int x = square.x; x < square.x + size; x++)
                {
                    if (layout.IsWall(x, y) || x >= layout.Width || y >= layout.Height)
                    {
                        return;
                    }
                }
            }

            reached[square.x, square.y] = true;
            open.Enqueue(square);
        }
    }
}
