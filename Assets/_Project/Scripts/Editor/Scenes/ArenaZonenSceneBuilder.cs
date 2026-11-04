using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the <c>Arena_Zonen</c> test level (issues #17, #18, #19): the arena of <see cref="ArenaTestSceneBuilder"/>
    /// in a tall layout with three zones stacked from south to north, separated by two gates. Each zone has its own
    /// spawn points and waves. The first gate is a kill gate; the second is a puzzle gate that opens once the player
    /// held the <see cref="HoldZonePuzzle"/> in zone 2 (issues #30, #31). Zone 3 has timed switches without a gate
    /// (issue #32) and an energy cell to bring to a generator (issue #33); the level exit at its north end finishes the
    /// level (issue #35).
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.ArenaZonenSceneBuilder.Build</c>
    /// </summary>
    public static class ArenaZonenSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Arena_Zonen.unity";
        private const int Width = 24;
        private const int Height = 84;
        // Rows of the walls between the zones; each has a gate in the middle.
        private static readonly int[] DividerRows = { 27, 55 };
        private const int GateX = 10;
        private const int GateWidth = 4;
        // Hold zone in zone 2: free floor between the pillars, away from the gates.
        private static readonly Vector2 HoldZoneCenter = new Vector2(12f, 38.5f);
        // Timed switches in zone 3: two sides and the far end, around the pillars.
        private static readonly Vector2[] SwitchPlates =
        {
            new Vector2(3.5f, 60.5f), new Vector2(20.5f, 66.5f), new Vector2(12.5f, 78.5f),
        };

        // Object transport in zone 3: the cell on the left, the generator on the right, the pillars in between.
        private static readonly Vector2 GeneratorPosition = new Vector2(18.5f, 76.5f);
        private static readonly Vector2[] CellPositions = { new Vector2(4.5f, 67.5f) };

        // Level exit: the last floor rows at the north end of zone 3.
        private static readonly Vector2 ExitCenter = new Vector2(12f, 81.75f);
        private const string WavesDir = "Assets/_Project/Data/Waves";
        // Kills since level start that open the first gate.
        private const int KillGateKills = 10;

        // Spawn points per zone, at the far end and the sides, away from where the player comes in.
        private static readonly Vector2Int[][] ZoneSpawnCells =
        {
            new[] { new Vector2Int(2, 25), new Vector2Int(21, 25), new Vector2Int(12, 24), new Vector2Int(2, 19), new Vector2Int(21, 19) },
            new[] { new Vector2Int(2, 53), new Vector2Int(21, 53), new Vector2Int(12, 52), new Vector2Int(2, 41), new Vector2Int(21, 41) },
            new[] { new Vector2Int(2, 81), new Vector2Int(21, 81), new Vector2Int(12, 80), new Vector2Int(2, 70), new Vector2Int(21, 70) },
        };

        private static readonly ArenaLayout Layout = new ArenaLayout(
            Width, Height, new Vector2Int(12, 4),
            new[]
            {
                // Zone 1
                new RectInt(5, 8, 3, 2),
                new RectInt(16, 12, 3, 3),
                new RectInt(9, 19, 6, 1),
                // Divider with gate 1
                new RectInt(1, DividerRows[0], GateX - 1, 1),
                new RectInt(GateX + GateWidth, DividerRows[0], Width - 1 - GateX - GateWidth, 1),
                // Zone 2
                new RectInt(4, 34, 2, 6),
                new RectInt(18, 33, 2, 6),
                new RectInt(10, 44, 4, 2),
                // Divider with gate 2
                new RectInt(1, DividerRows[1], GateX - 1, 1),
                new RectInt(GateX + GateWidth, DividerRows[1], Width - 1 - GateX - GateWidth, 1),
                // Zone 3
                new RectInt(6, 62, 3, 3),
                new RectInt(15, 62, 3, 3),
                new RectInt(8, 72, 8, 2),
            });

        [MenuItem("RushOps/Szenen/Arena_Zonen erzeugen")]
        public static void Build()
        {
            ArenaTestSceneBuilder.BuildScene(ScenePath, Layout, false, AddZones, "Zonen-Test");
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

            // Zones span the inner columns, from the row above the previous divider up to their own divider.
            WaveDefinition[] waves = CreateWaves();
            var zonesRoot = new GameObject("Zones").transform;
            var zones = new Zone[gates.Length + 1];
            HoldZonePuzzle hold = null;
            int bottom = 1;
            for (int i = 0; i < zones.Length; i++)
            {
                int top = i < gates.Length ? DividerRows[i] + 1 : Height - 1;
                Gate exit = i < gates.Length ? gates[i] : null;
                var area = new RectInt(1, bottom, Width - 2, top - bottom);
                Zone zone = ArenaTestSceneBuilder.CreateZone(arena, $"Zone_{i + 1}", zonesRoot, area, ZoneSpawnCells[i], waves[i], exit);
                zones[i] = zone;
                if (exit != null && exit.GetComponent<PuzzleSolvedCondition>() != null)
                {
                    hold = PuzzleBuilder.CreateHoldZone($"HoldZone_{zone.name}", HoldZoneCenter, zone, arena);
                    PuzzleBuilder.AddPressure(hold, arena, CreateHoldPressureWaves(), AfterPuzzle.ResumeZoneWaves);
                    PuzzleBuilder.SetPuzzles(exit.GetComponent<PuzzleSolvedCondition>(), hold);
                }

                bottom = top;
            }

            ArenaTestSceneBuilder.AssignZones(arena, zones);
            TimedSwitchPuzzle switches = PuzzleBuilder.CreateTimedSwitches("Switches_Zone_3", SwitchPlates, zones[2], arena);
            TransportPuzzle transport = PuzzleBuilder.CreateTransport(
                "Transport_Zone_3", GeneratorPosition, CellPositions, 1, zones[2], arena);
            PuzzleBuilder.SetLevelPuzzles(arena, hold, switches, transport);
            LevelPieceBuilder.CreateExit(arena, ExitCenter);
        }

        // While the hold zone runs, zone 2 sends smaller waves at a much faster pace, so standing still is never safe.
        private static WaveDefinition CreateHoldPressureWaves()
        {
            var walker = ArenaTestSceneBuilder.LoadOrCreateConfig<EnemyDefinition>(ArenaTestSceneBuilder.WalkerDefinitionPath);
            EnemyDefinition runner = ArenaTestSceneBuilder.LoadOrCreateRunner();
            return ArenaTestSceneBuilder.LoadOrCreateWaves($"{WavesDir}/Arena_Zonen_Zone2_Hold.asset", w => w.Configure(
                new[]
                {
                    new Wave(new WaveEntry(walker, 4), new WaveEntry(runner, 2)),
                    new Wave(new WaveEntry(walker, 5), new WaveEntry(runner, 3)),
                },
                0.4f, 1.5f, AfterLastWave.RepeatLast));
        }

        // Waves grow from zone to zone; the first two repeat their last wave until the gate opens, the last one ends.
        private static WaveDefinition[] CreateWaves()
        {
            var walker = ArenaTestSceneBuilder.LoadOrCreateConfig<EnemyDefinition>(ArenaTestSceneBuilder.WalkerDefinitionPath);
            EnemyDefinition runner = ArenaTestSceneBuilder.LoadOrCreateRunner();
            EnemyDefinition brute = ArenaTestSceneBuilder.LoadOrCreateBrute();
            EnemyDefinition boomer = ArenaTestSceneBuilder.LoadOrCreateBoomer();
            return new[]
            {
                ArenaTestSceneBuilder.LoadOrCreateWaves($"{WavesDir}/Arena_Zonen_Zone1.asset", w => w.Configure(
                    new[] { new Wave(new WaveEntry(walker, 4)), new Wave(new WaveEntry(walker, 6)), new Wave(new WaveEntry(walker, 8)) },
                    1f, 4f, AfterLastWave.RepeatLast)),
                ArenaTestSceneBuilder.LoadOrCreateWaves($"{WavesDir}/Arena_Zonen_Zone2.asset", w => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(walker, 6)),
                        new Wave(new WaveEntry(walker, 8), new WaveEntry(runner, 2)),
                        new Wave(new WaveEntry(walker, 9), new WaveEntry(runner, 3), new WaveEntry(boomer, 1)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)),
                ArenaTestSceneBuilder.LoadOrCreateWaves($"{WavesDir}/Arena_Zonen_Zone3.asset", w => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(walker, 6), new WaveEntry(runner, 2)),
                        new Wave(new WaveEntry(walker, 8), new WaveEntry(runner, 4), new WaveEntry(brute, 1), new WaveEntry(boomer, 2)),
                        new Wave(new WaveEntry(walker, 10), new WaveEntry(runner, 6), new WaveEntry(brute, 2), new WaveEntry(boomer, 3)),
                    },
                    0.6f, 3f, AfterLastWave.End)),
            };
        }
    }
}
