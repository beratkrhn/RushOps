using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// One of the four gates of the endless arena (issue #206, decision E1): the cells of its sprite (ring wall cells
    /// plus the tunnel through the stands), the ring wall cells it replaces and the spawn points in front of it. The
    /// gate stays closed; zombies appear at the spawn points.
    /// </summary>
    internal sealed class ColosseumGate
    {
        public ColosseumGate(string name, RectInt area, Vector2Int ringCellA, Vector2Int ringCellB, params Vector2Int[] spawnCells)
        {
            Name = name;
            Area = area;
            RingCells = new[] { ringCellA, ringCellB };
            SpawnCells = spawnCells;
        }

        /// <summary>Direction letter: N, E, S or W.</summary>
        public string Name { get; }

        /// <summary>Cells covered by the gate sprite, from the ring wall out to the edge of the stands.</summary>
        public RectInt Area { get; }

        /// <summary>The two ring wall cells of the gate opening.</summary>
        public Vector2Int[] RingCells { get; }

        /// <summary>Spawn points in front of the gate.</summary>
        public Vector2Int[] SpawnCells { get; }

        /// <summary>Center of <see cref="Area"/> in world units, where the gate sprite sits.</summary>
        public Vector2 Center => Area.center;

        /// <summary>Sprite asset name, e.g. <c>Gate_N</c>.</summary>
        public string SpriteName => $"Gate_{Name}";
    }
}
