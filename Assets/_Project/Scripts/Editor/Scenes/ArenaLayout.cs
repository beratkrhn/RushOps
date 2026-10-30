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

        /// <summary>
        /// Creates a layout of <paramref name="width"/> x <paramref name="height"/> cells with a wall ring around it,
        /// inner <paramref name="walls"/> as (x, y, width, height) rectangles and the player on <paramref name="startCell"/>.
        /// </summary>
        public ArenaLayout(int width, int height, Vector2Int startCell, RectInt[] walls)
        {
            Width = width;
            Height = height;
            StartCell = startCell;
            _walls = walls;
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
            return inside && (border || IsInnerWall(x, y));
        }

        private bool IsInnerWall(int x, int y)
        {
            foreach (RectInt wall in _walls)
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
