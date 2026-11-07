using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Floor plan of a test arena in cells: size, outer wall ring, inner walls and player start.
    /// Cell (x, y) covers the world square from (x, y) to (x + 1, y + 1).
    /// </summary>
    internal sealed class ArenaLayout
    {
        private readonly RectInt[] _walls;
        private readonly RectInt[] _outerWalls;

        /// <summary>
        /// Creates a layout of <paramref name="width"/> x <paramref name="height"/> cells with a wall ring around it,
        /// inner <paramref name="walls"/> as (x, y, width, height) rectangles and the player on <paramref name="startCell"/>.
        /// </summary>
        public ArenaLayout(int width, int height, Vector2Int startCell, RectInt[] walls)
            : this(width, height, startCell, walls, System.Array.Empty<RectInt>())
        {
        }

        /// <summary>
        /// Like the other constructor, plus <paramref name="outerWalls"/>: cells that belong to the outer wall although
        /// they are not on the border, e.g. the round wall of the endless arena (issue #206). They are drawn as outer
        /// wall, not as obstacles.
        /// </summary>
        public ArenaLayout(int width, int height, Vector2Int startCell, RectInt[] walls, RectInt[] outerWalls)
        {
            Width = width;
            Height = height;
            StartCell = startCell;
            _walls = walls;
            _outerWalls = outerWalls;
        }

        /// <summary>Number of columns, including the wall ring.</summary>
        public int Width { get; }

        /// <summary>Number of rows, including the wall ring.</summary>
        public int Height { get; }

        /// <summary>Cell the player starts on.</summary>
        public Vector2Int StartCell { get; }

        /// <summary>True if (x, y) is inside the arena and part of the wall ring or an inner wall.</summary>
        public bool IsWall(int x, int y)
        {
            bool inside = x >= 0 && y >= 0 && x < Width && y < Height;
            bool border = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;
            return inside && (border || Contains(_outerWalls, x, y) || Contains(_walls, x, y));
        }

        /// <summary>
        /// True if (x, y) is part of the outer wall ring or one of the extra outer walls; inner walls are obstacles
        /// (issue #104).
        /// </summary>
        public bool IsOuterWall(int x, int y)
        {
            bool inside = x >= 0 && y >= 0 && x < Width && y < Height;
            return inside && (x == 0 || y == 0 || x == Width - 1 || y == Height - 1 || Contains(_outerWalls, x, y));
        }

        private static bool Contains(RectInt[] walls, int x, int y)
        {
            foreach (RectInt wall in walls)
            {
                if (wall.Contains(new Vector2Int(x, y)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
