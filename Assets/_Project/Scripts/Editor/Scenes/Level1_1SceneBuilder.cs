using System.Collections.Generic;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the prototype of level 1-1 (issue #36, F-LVL-06): five zones stacked from south to north, each one a
    /// step harder than the one before.
    /// <list type="number">
    /// <item>Ankunft: walkers only, kill gate.</item>
    /// <item>Halten: hold-the-line zone under its own faster waves; runners join.</item>
    /// <item>Schalter: two lanes split by a long wall, three timed switches; boomers join.</item>
    /// <item>Generator: two energy cells on both sides of a wall, carried to the generator; brutes join.</item>
    /// <item>Ausgang: a last rush of all types; the level exit opens once it is beaten.</item>
    /// </list>
    /// The layout is checked against <c>docs/level-design.md</c> before the scene is saved (<see cref="Validate"/>).
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level1_1SceneBuilder.Build</c>
    /// </summary>
    public static class Level1_1SceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Level_1-1.unity";
        private const string DisplayName = "Level 1-1";
        private const string WavesDir = "Assets/_Project/Data/Waves";
        private const string WavesPrefix = WavesDir + "/Level_1-1_";

        // Starting star thresholds for a new level definition; tuned in the asset after playtests.
        private static readonly Vector2Int StarScores = new Vector2Int(1500, 3000);

        private const int Width = 24;
        private const int Height = 118;
        // Rows of the walls between the zones; each has a gate in the middle.
        private static readonly int[] DividerRows = { 23, 48, 75, 102 };
        private const int GateX = 10;
        private const int GateWidth = 4;
        // Kills since level start that open the first gate (F-LVL-03).
        private const int KillGateKills = 15;
        private static readonly Vector2Int StartCell = new Vector2Int(12, 3);

        // Zone 2: hold zone in the open middle between two pillars.
        private static readonly Vector2 HoldZoneCenter = new Vector2(12f, 36f);

        // Zone 3: one switch in each lane and one past the end of the dividing wall.
        private static readonly Vector2[] SwitchPlates =
        {
            new Vector2(3.5f, 58.5f), new Vector2(19.5f, 63.5f), new Vector2(11.5f, 71.5f),
        };

        // Zone 4: the generator is in sight of the entry; one cell lies south, one north of the wall.
        private static readonly Vector2 GeneratorPosition = new Vector2(19.5f, 92.5f);
        private static readonly Vector2[] CellPositions = { new Vector2(3.5f, 80.5f), new Vector2(3.5f, 96.5f) };
        private const int RequiredCells = 2;

        // Zone 5: the exit lies on the last floor rows at the north end.
        private static readonly Vector2 ExitCenter = new Vector2(12f, Height - 2.25f);

        // Spawn points per zone: at the far end and along the sides, away from where the player comes in.
        private static readonly Vector2Int[][] ZoneSpawnCells =
        {
            new[] { new Vector2Int(2, 21), new Vector2Int(21, 21), new Vector2Int(12, 20), new Vector2Int(2, 13), new Vector2Int(21, 13) },
            new[] { new Vector2Int(2, 46), new Vector2Int(21, 46), new Vector2Int(12, 45), new Vector2Int(2, 33), new Vector2Int(21, 33) },
            new[] { new Vector2Int(2, 64), new Vector2Int(21, 58), new Vector2Int(2, 73), new Vector2Int(21, 73), new Vector2Int(16, 71) },
            new[] { new Vector2Int(2, 84), new Vector2Int(21, 84), new Vector2Int(2, 100), new Vector2Int(21, 100), new Vector2Int(12, 99) },
            new[] { new Vector2Int(2, 115), new Vector2Int(21, 115), new Vector2Int(2, 111), new Vector2Int(21, 111) },
        };

        private static readonly RectInt[] Obstacles =
        {
            // Zone 1: crates to shoot around.
            new RectInt(4, 8, 3, 2),
            new RectInt(17, 8, 3, 2),
            new RectInt(8, 15, 2, 2),
            new RectInt(14, 15, 2, 2),
            // Zone 2: two pillars beside the hold zone, a wall in front of the far spawn points.
            new RectInt(5, 30, 2, 5),
            new RectInt(17, 30, 2, 5),
            new RectInt(9, 42, 6, 1),
            // Zone 3: a long wall splits the zone into two lanes.
            new RectInt(11, 55, 2, 12),
            new RectInt(18, 51, 2, 2),
            new RectInt(4, 69, 2, 2),
            // Zone 4: a wall between the cells and the generator, one crate near the generator.
            new RectInt(8, 86, 8, 2),
            new RectInt(17, 96, 2, 2),
            // Zone 5: two crates in front of the exit.
            new RectInt(5, 108, 3, 2),
            new RectInt(16, 108, 3, 2),
        };

        internal static readonly ArenaLayout Layout = new ArenaLayout(Width, Height, StartCell, BuildWalls());

        [MenuItem("RushOps/Szenen/Level_1-1 erzeugen")]
        public static void Build()
        {
            List<string> problems = Validate();
            if (problems.Count > 0)
            {
                throw new System.InvalidOperationException("Level_1-1 breaks the level design rules:\n" + string.Join("\n", problems));
            }

            ArenaTestSceneBuilder.BuildScene(
                ScenePath, Layout, false, AddZones, DisplayName, StarScores, IslandThemes.StrandPath);
        }

        /// <summary>
        /// Checks the layout against <c>docs/level-design.md</c>; returns one line per broken rule, empty if all hold.
        /// </summary>
        internal static List<string> Validate()
        {
            var problems = new List<string>();
            int size = LevelLayoutCheck.MinPassage;
            bool[,] reached = LevelLayoutCheck.ReachableSquares(Layout, StartCell, size);
            float sight = ArenaTestSceneBuilder.ReferenceHeight / (float)Art.PixelArtImportRules.PixelsPerUnit;

            for (int zone = 0; zone < ZoneSpawnCells.Length; zone++)
            {
                foreach (Vector2Int cell in ZoneSpawnCells[zone])
                {
                    if (!LevelLayoutCheck.HasClearance(Layout, cell, 1))
                    {
                        problems.Add($"Zone {zone + 1}: spawn point {cell} has a wall within one cell");
                    }

                    if (!InZone(zone, cell.y))
                    {
                        problems.Add($"Zone {zone + 1}: spawn point {cell} lies outside the zone");
                    }

                    if (!LevelLayoutCheck.Covers(reached, size, cell))
                    {
                        problems.Add($"Zone {zone + 1}: spawn point {cell} is not reachable through passages of {size} cells");
                    }
                }
            }

            var targets = new List<(string name, Vector2 point)>
            {
                ("hold zone", HoldZoneCenter), ("generator", GeneratorPosition), ("exit", ExitCenter),
            };
            for (int i = 0; i < SwitchPlates.Length; i++)
            {
                targets.Add(($"switch {i + 1}", SwitchPlates[i]));
            }

            for (int i = 0; i < CellPositions.Length; i++)
            {
                targets.Add(($"energy cell {i + 1}", CellPositions[i]));
            }

            foreach (int row in DividerRows)
            {
                targets.Add(($"gate in row {row}", new Vector2(GateX + GateWidth * 0.5f, row + 0.5f)));
            }

            foreach ((string name, Vector2 point) in targets)
            {
                if (!LevelLayoutCheck.Covers(reached, size, LevelLayoutCheck.CellOf(point)))
                {
                    problems.Add($"{name} at {point} is not reachable through passages of {size} cells");
                }
            }

            // Each puzzle shows at least one of its parts right where the player enters its zone.
            CheckInSight(problems, "hold zone", 1, sight, HoldZoneCenter);
            CheckInSight(problems, "switches", 2, sight, SwitchPlates);
            CheckInSight(problems, "transport", 3, sight, GeneratorPosition);
            CheckInSight(problems, "exit", 4, sight, ExitCenter);
            return problems;
        }

        private static RectInt[] BuildWalls()
        {
            var walls = new List<RectInt>(Obstacles);
            foreach (int row in DividerRows)
            {
                walls.Add(new RectInt(1, row, GateX - 1, 1));
                walls.Add(new RectInt(GateX + GateWidth, row, Width - 1 - GateX - GateWidth, 1));
            }

            return walls.ToArray();
        }

        // Zone i spans the rows between divider i - 1 and divider i.
        private static bool InZone(int zone, int row)
        {
            int bottom = zone == 0 ? 0 : DividerRows[zone - 1];
            int top = zone < DividerRows.Length ? DividerRows[zone] : Height - 1;
            return row > bottom && row < top;
        }

        private static void CheckInSight(List<string> problems, string name, int zone, float sight, params Vector2[] parts)
        {
            var entry = new Vector2(GateX + GateWidth * 0.5f, DividerRows[zone - 1] + 1.5f);
            foreach (Vector2 part in parts)
            {
                if (Vector2.Distance(entry, part) <= sight && LevelLayoutCheck.HasLineOfSight(Layout, entry, part))
                {
                    return;
                }
            }

            problems.Add($"Zone {zone + 1}: no part of the {name} is in sight of the zone entry");
        }

        private static void AddZones(ArenaScene arena)
        {
            var gatesRoot = new GameObject("Gates").transform;
            var gates = new Gate[DividerRows.Length];
            for (int i = 0; i < gates.Length; i++)
            {
                var center = new Vector2(GateX + GateWidth * 0.5f, DividerRows[i] + 0.5f);
                gates[i] = LevelPieceBuilder.CreateGate(
                    $"Gate_{i + 1}", gatesRoot, center, GateWidth, arena, i == 0 ? KillGateKills : 0);
            }

            WaveDefinition[] waves = CreateWaves();
            var zonesRoot = new GameObject("Zones").transform;
            var zones = new Zone[gates.Length + 1];
            int bottom = 1;
            for (int i = 0; i < zones.Length; i++)
            {
                int top = i < gates.Length ? DividerRows[i] + 1 : Height - 1;
                Gate exit = i < gates.Length ? gates[i] : null;
                var area = new RectInt(1, bottom, Width - 2, top - bottom);
                zones[i] = ArenaTestSceneBuilder.CreateZone(arena, $"Zone_{i + 1}", zonesRoot, area, ZoneSpawnCells[i], waves[i], exit);
                bottom = top;
            }

            ArenaTestSceneBuilder.AssignZones(arena, zones);

            HoldZonePuzzle hold = PuzzleBuilder.CreateHoldZone("HoldZone_Zone_2", HoldZoneCenter, zones[1], arena);
            PuzzleBuilder.AddPressure(hold, arena, CreateHoldPressureWaves(), AfterPuzzle.EndWaves);
            PuzzleBuilder.SetPuzzles(gates[1].GetComponent<PuzzleSolvedCondition>(), hold);

            TimedSwitchPuzzle switches = PuzzleBuilder.CreateTimedSwitches("Switches_Zone_3", SwitchPlates, zones[2], arena);
            PuzzleBuilder.SetPuzzles(gates[2].GetComponent<PuzzleSolvedCondition>(), switches);

            TransportPuzzle transport = PuzzleBuilder.CreateTransport(
                "Transport_Zone_4", GeneratorPosition, CellPositions, RequiredCells, zones[3], arena);
            PuzzleBuilder.SetPuzzles(gates[3].GetComponent<PuzzleSolvedCondition>(), transport);

            PuzzleBuilder.SetLevelPuzzles(arena, hold, switches, transport);
            // The exit opens only after the last rush, so it cannot be skipped (playtest v0.3.0).
            LevelPieceBuilder.CreateExit(arena, ExitCenter, zones[4]);
        }

        // While the hold zone runs, zone 2 sends small waves at a fast pace; once it is held, the zone goes quiet.
        private static WaveDefinition CreateHoldPressureWaves()
        {
            var walker = ArenaTestSceneBuilder.LoadOrCreateConfig<EnemyDefinition>(ArenaTestSceneBuilder.WalkerDefinitionPath);
            EnemyDefinition runner = ArenaTestSceneBuilder.LoadOrCreateRunner();
            return ArenaTestSceneBuilder.LoadOrCreateWaves(WavesPrefix + "Zone2_Hold.asset", w => w.Configure(
                new[]
                {
                    new Wave(new WaveEntry(walker, 4), new WaveEntry(runner, 1)),
                    new Wave(new WaveEntry(walker, 4), new WaveEntry(runner, 2)),
                },
                0.5f, 2f, AfterLastWave.RepeatLast));
        }

        // Each zone adds one enemy type: walkers, runners, boomers, brutes. Zones with a gate repeat their last wave
        // until it opens; the last zone ends after its waves.
        private static WaveDefinition[] CreateWaves()
        {
            var walker = ArenaTestSceneBuilder.LoadOrCreateConfig<EnemyDefinition>(ArenaTestSceneBuilder.WalkerDefinitionPath);
            EnemyDefinition runner = ArenaTestSceneBuilder.LoadOrCreateRunner();
            EnemyDefinition brute = ArenaTestSceneBuilder.LoadOrCreateBrute();
            EnemyDefinition boomer = ArenaTestSceneBuilder.LoadOrCreateBoomer();
            return new[]
            {
                ArenaTestSceneBuilder.LoadOrCreateWaves(WavesPrefix + "Zone1.asset", w => w.Configure(
                    new[] { new Wave(new WaveEntry(walker, 4)), new Wave(new WaveEntry(walker, 6)), new Wave(new WaveEntry(walker, 8)) },
                    1f, 4f, AfterLastWave.RepeatLast)),
                ArenaTestSceneBuilder.LoadOrCreateWaves(WavesPrefix + "Zone2.asset", w => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(walker, 6), new WaveEntry(runner, 2)),
                        new Wave(new WaveEntry(walker, 6), new WaveEntry(runner, 3)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)),
                ArenaTestSceneBuilder.LoadOrCreateWaves(WavesPrefix + "Zone3.asset", w => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(walker, 6), new WaveEntry(runner, 2)),
                        new Wave(new WaveEntry(walker, 6), new WaveEntry(runner, 3), new WaveEntry(boomer, 1)),
                        new Wave(new WaveEntry(walker, 8), new WaveEntry(runner, 4), new WaveEntry(boomer, 2)),
                    },
                    0.7f, 3f, AfterLastWave.RepeatLast)),
                ArenaTestSceneBuilder.LoadOrCreateWaves(WavesPrefix + "Zone4.asset", w => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(walker, 6), new WaveEntry(runner, 2), new WaveEntry(brute, 1)),
                        new Wave(new WaveEntry(walker, 8), new WaveEntry(runner, 3), new WaveEntry(brute, 1), new WaveEntry(boomer, 1)),
                        new Wave(new WaveEntry(walker, 8), new WaveEntry(runner, 4), new WaveEntry(brute, 2), new WaveEntry(boomer, 2)),
                    },
                    0.6f, 3f, AfterLastWave.RepeatLast)),
                ArenaTestSceneBuilder.LoadOrCreateWaves(WavesPrefix + "Zone5.asset", w => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(walker, 4), new WaveEntry(runner, 4)),
                        new Wave(new WaveEntry(walker, 4), new WaveEntry(runner, 4), new WaveEntry(brute, 1), new WaveEntry(boomer, 2)),
                    },
                    0.5f, 2f, AfterLastWave.End)),
            };
        }
    }
}
