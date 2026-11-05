using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Short description of a zone level (issue #148): floor plan, gates, zones, puzzles, exit, campaign slot and
    /// difficulty. <see cref="LevelBlueprintBuilder"/> checks it against <c>docs/level-design.md</c> and builds the
    /// scene from it, so a new level is a list of numbers instead of its own builder.
    /// </summary>
    internal sealed class LevelBlueprint
    {
        private ArenaLayout _layout;

        public LevelBlueprint(string sceneName, string displayName, int width, int height, Vector2Int startCell)
        {
            SceneName = sceneName;
            DisplayName = displayName;
            Width = width;
            Height = height;
            StartCell = startCell;
        }

        /// <summary>Scene and level id, e.g. <c>Level_1-2</c>.</summary>
        public string SceneName { get; }

        /// <summary>Short name on the map (decision C11), e.g. „Strandpromenade“.</summary>
        public string DisplayName { get; }

        public int Width { get; }

        public int Height { get; }

        public Vector2Int StartCell { get; }

        /// <summary>Island index (0-based) in the campaign; -1 keeps the level out of the campaign.</summary>
        public int Island { get; set; } = -1;

        /// <summary>Slot on the island (0-based).</summary>
        public int Slot { get; set; } = -1;

        /// <summary>Island theme asset path.</summary>
        public string ThemePath { get; set; } = IslandThemes.DefaultPath;

        /// <summary>Scores for two and three stars of a new level definition.</summary>
        public Vector2Int StarScores { get; set; } = new Vector2Int(1500, 3000);

        /// <summary>Extra line on „Level geschafft!“, e.g. after the last boss (decision C12); null = none.</summary>
        public string CompletionMessage { get; set; }

        /// <summary>Difficulty asset path; null uses the values of the waves and puzzle configs unchanged.</summary>
        public string DifficultyPath { get; set; }

        /// <summary>Inner walls and obstacles as cell rectangles; divider walls are added by the divider helpers.</summary>
        public List<RectInt> Walls { get; } = new List<RectInt>();

        public List<LevelGate> Gates { get; } = new List<LevelGate>();

        public List<LevelZone> Zones { get; } = new List<LevelZone>();

        public List<LevelPuzzle> Puzzles { get; } = new List<LevelPuzzle>();

        /// <summary>Center of the level exit.</summary>
        public Vector2 ExitCenter { get; set; }

        /// <summary>Zone whose waves must be beaten before the exit opens; -1 for an exit that is always open.</summary>
        public int ExitLockZone { get; set; } = -1;

        /// <summary>Boss of a boss level, or null. A boss level has no exit; beating the boss finishes it.</summary>
        public LevelBoss Boss { get; set; }

        /// <summary>Weapons hanging on walls (issue #175).</summary>
        public List<LevelWallWeapon> WallWeapons { get; } = new List<LevelWallWeapon>();

        /// <summary>Path of the scene asset.</summary>
        public string ScenePath => $"Assets/_Project/Scenes/{SceneName}.unity";

        /// <summary>Floor plan from size, start and walls; built on first use, so add all walls before.</summary>
        public ArenaLayout Layout => _layout ??= new ArenaLayout(Width, Height, StartCell, Walls.ToArray());

        /// <summary>
        /// A horizontal wall in <paramref name="row"/> from column <paramref name="fromX"/> to <paramref name="toX"/>
        /// (inclusive) with a gate of <paramref name="gateLength"/> cells starting at <paramref name="gateX"/>.
        /// Returns the index of the gate.
        /// </summary>
        public int AddHorizontalDivider(string gateName, int row, int fromX, int toX, int gateX, int gateLength)
        {
            AddWall(new RectInt(fromX, row, gateX - fromX, 1));
            AddWall(new RectInt(gateX + gateLength, row, toX + 1 - gateX - gateLength, 1));
            Gates.Add(new LevelGate(gateName, new Vector2Int(gateX, row), gateLength, false));
            return Gates.Count - 1;
        }

        /// <summary>
        /// A vertical wall in <paramref name="column"/> from row <paramref name="fromY"/> to <paramref name="toY"/>
        /// (inclusive) with a gate of <paramref name="gateLength"/> cells starting at <paramref name="gateY"/>, for a side
        /// branch (decision L3). Returns the index of the gate.
        /// </summary>
        public int AddVerticalDivider(string gateName, int column, int fromY, int toY, int gateY, int gateLength)
        {
            AddWall(new RectInt(column, fromY, 1, gateY - fromY));
            AddWall(new RectInt(column, gateY + gateLength, 1, toY + 1 - gateY - gateLength));
            Gates.Add(new LevelGate(gateName, new Vector2Int(column, gateY), gateLength, true));
            return Gates.Count - 1;
        }

        /// <summary>Adds a zone and returns its index.</summary>
        public int AddZone(LevelZone zone)
        {
            Zones.Add(zone);
            return Zones.Count - 1;
        }

        /// <summary>Adds a puzzle and returns its index.</summary>
        public int AddPuzzle(LevelPuzzle puzzle)
        {
            Puzzles.Add(puzzle);
            return Puzzles.Count - 1;
        }

        private void AddWall(RectInt wall)
        {
            if (wall.width > 0 && wall.height > 0)
            {
                Walls.Add(wall);
            }
        }
    }
}
