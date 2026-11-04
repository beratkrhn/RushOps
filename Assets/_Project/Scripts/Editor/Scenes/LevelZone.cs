using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>A zone in a <see cref="LevelBlueprint"/>: its cells, spawn points, waves and the gate that leads out.</summary>
    internal sealed class LevelZone
    {
        public LevelZone(string name, RectInt area, Vector2Int[] spawnCells, LevelWaves waves, int exitGate)
        {
            Name = name;
            Area = area;
            SpawnCells = spawnCells ?? System.Array.Empty<Vector2Int>();
            Waves = waves;
            ExitGate = exitGate;
        }

        public string Name { get; }

        /// <summary>Cells of the zone; the player activates it by stepping in.</summary>
        public RectInt Area { get; }

        public Vector2Int[] SpawnCells { get; }

        /// <summary>Waves while the zone is active, or null.</summary>
        public LevelWaves Waves { get; }

        /// <summary>Index of the blueprint gate that leads out of the zone; -1 for none.</summary>
        public int ExitGate { get; }
    }
}
