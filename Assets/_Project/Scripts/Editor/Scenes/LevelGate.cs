using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// A gate in a <see cref="LevelBlueprint"/>: a gap in a divider wall, horizontal (zones above each other) or
    /// vertical (side branch, decision L3). It opens after <see cref="Kills"/> kills, after its <see cref="Puzzles"/>
    /// or, with both, after all of them (decision L2).
    /// </summary>
    internal sealed class LevelGate
    {
        public LevelGate(string name, Vector2Int cell, int length, bool vertical)
        {
            Name = name;
            Cell = cell;
            Length = length;
            Vertical = vertical;
        }

        public string Name { get; }

        /// <summary>Lowest, leftmost cell of the gap.</summary>
        public Vector2Int Cell { get; }

        /// <summary>Length of the gap in cells.</summary>
        public int Length { get; }

        /// <summary>True for a gate in a vertical wall (passage to the east or west).</summary>
        public bool Vertical { get; }

        /// <summary>Kills that open the gate; 0 for none.</summary>
        public int Kills { get; set; }

        /// <summary>Zone index from whose entry the kills count (decision P1); -1 counts from level start.</summary>
        public int CountKillsFromZone { get; set; } = -1;

        /// <summary>Indices of the blueprint's puzzles that must be solved.</summary>
        public List<int> Puzzles { get; } = new List<int>();

        /// <summary>World center of the gap.</summary>
        public Vector2 Center => Vertical
            ? new Vector2(Cell.x + 0.5f, Cell.y + Length * 0.5f)
            : new Vector2(Cell.x + Length * 0.5f, Cell.y + 0.5f);

        /// <summary>Cells of the gap.</summary>
        public RectInt Area => Vertical ? new RectInt(Cell.x, Cell.y, 1, Length) : new RectInt(Cell.x, Cell.y, Length, 1);
    }
}
