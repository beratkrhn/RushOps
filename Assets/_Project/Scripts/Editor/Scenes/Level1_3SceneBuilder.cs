using System.Collections.Generic;
using RushOps.Bosses;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Level 1-3 „Leuchtturm“ (issue #151): two short zones before the arena (decision C6), then the fight against the
    /// Koloss.
    /// <list type="number">
    /// <item>Steg: kill gate, walkers and runners.</item>
    /// <item>Klippe: three timed switches around a wall.</item>
    /// <item>Leuchtturm: open arena; the door closes behind the player and the Koloss attacks.</item>
    /// </list>
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level1_3SceneBuilder.Build</c>
    /// </summary>
    public static class Level1_3SceneBuilder
    {
        private const string WavesPrefix = "Level_1-3_";

        private const int Width = 24;
        private const int Height = 76;
        private const int GateX = 10;
        private const int GateWidth = 4;
        private static readonly int[] DividerRows = { 16, 34 };
        // Two rows behind the second gate still belong to zone 2, so the door closes behind the player.
        private const int ArenaBottom = 37;
        private const int StegKills = 12;
        private static readonly Vector2Int StartCell = new Vector2Int(12, 3);
        private static readonly Vector2 BossStart = new Vector2(12f, 62f);

        private static readonly Vector2[] SwitchPlates =
        {
            new Vector2(4.5f, 22.5f), new Vector2(19.5f, 26.5f), new Vector2(5.5f, 31.5f),
        };

        private static readonly Vector2Int[][] ZoneSpawnCells =
        {
            new[] { new Vector2Int(2, 14), new Vector2Int(21, 14), new Vector2Int(12, 13), new Vector2Int(2, 9), new Vector2Int(21, 9) },
            new[] { new Vector2Int(2, 27), new Vector2Int(21, 31), new Vector2Int(21, 20), new Vector2Int(12, 30) },
        };

        private static readonly RectInt[] Obstacles =
        {
            // Zone 1: crates on the jetty.
            new RectInt(5, 8, 2, 2),
            new RectInt(17, 8, 2, 2),
            // Zone 2: a rock wall between the switches.
            new RectInt(9, 24, 6, 1),
        };

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Level_1-3 erzeugen")]
        public static void Build()
        {
            LevelBlueprintBuilder.Build(Blueprint);
        }

        /// <summary>Checks the layout against <c>docs/level-design.md</c>; empty if all rules hold.</summary>
        internal static List<string> Validate()
        {
            return LevelBlueprintBuilder.Validate(Blueprint);
        }

        private static LevelBlueprint CreateBlueprint()
        {
            var level = new LevelBlueprint("Level_1-3", "Leuchtturm", Width, Height, StartCell)
            {
                Island = 0,
                Slot = 2,
                ThemePath = IslandThemes.StrandPath,
                StarScores = new Vector2Int(1500, 2200),
                DifficultyPath = LevelDifficulties.Island1Path,
            };
            level.Walls.AddRange(Obstacles);
            int steg = level.AddHorizontalDivider("Gate_1", DividerRows[0], 1, Width - 2, GateX, GateWidth);
            int klippe = level.AddHorizontalDivider("Gate_2", DividerRows[1], 1, Width - 2, GateX, GateWidth);
            level.Gates[steg].Kills = StegKills;

            level.AddZone(new LevelZone("Zone_1", new RectInt(1, 1, Width - 2, DividerRows[0]), ZoneSpawnCells[0],
                new LevelWaves(WavesPrefix + "Zone1.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 1)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 2)),
                    },
                    0.9f, 3f, AfterLastWave.RepeatLast)), steg));
            int zone2 = level.AddZone(new LevelZone("Zone_2",
                new RectInt(1, DividerRows[0] + 1, Width - 2, ArenaBottom - DividerRows[0] - 1), ZoneSpawnCells[1],
                new LevelWaves(WavesPrefix + "Zone2.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 2)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 2), new WaveEntry(e.Boomer, 1)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)), klippe));
            int arena = level.AddZone(new LevelZone(
                "Zone_Arena", new RectInt(1, ArenaBottom, Width - 2, Height - 1 - ArenaBottom), null, null, -1));

            int switches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_2", zone2, SwitchPlates));
            level.Gates[klippe].Puzzles.Add(switches);
            level.Boss = new LevelBoss(ColossusBuilder.DefinitionPath, BossStart, arena, klippe, ColossusBuilder.Configure)
            {
                DefinitionType = typeof(ColossusDefinition),
                Create = ColossusBuilder.Create,
            };
            return level;
        }
    }
}
