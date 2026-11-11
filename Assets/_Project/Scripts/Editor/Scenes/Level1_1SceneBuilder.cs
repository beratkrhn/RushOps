using System.Collections.Generic;
using RushOps.Enemies;
using RushOps.Puzzles;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Level 1-1 „Strandbad“ (issue #36, F-LVL-06) as a <see cref="LevelBlueprint"/> (issue #148): five zones stacked
    /// from south to north, each one a step harder than the one before.
    /// <list type="number">
    /// <item>Ankunft: walkers only, kill gate.</item>
    /// <item>Halten: hold-the-line zone under its own faster waves; runners join.</item>
    /// <item>Schalter: two lanes split by a long wall, three timed switches; boomers join.</item>
    /// <item>Generator: two energy cells on both sides of a wall, carried to the generator; brutes join.</item>
    /// <item>Ausgang: a last rush of all types; the level exit opens once it is beaten.</item>
    /// </list>
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level1_1SceneBuilder.Build</c>
    /// </summary>
    public static class Level1_1SceneBuilder
    {
        private const string WavesPrefix = "Level_1-1_";

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

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        internal static ArenaLayout Layout => Blueprint.Layout;

        [MenuItem("RushOps/Szenen/Level_1-1 erzeugen")]
        public static void Build()
        {
            LevelBlueprintBuilder.Build(Blueprint);
        }

        /// <summary>
        /// Checks the layout against <c>docs/level-design.md</c>; returns one line per broken rule, empty if all hold.
        /// </summary>
        internal static List<string> Validate()
        {
            return LevelBlueprintBuilder.Validate(Blueprint);
        }

        private static LevelBlueprint CreateBlueprint()
        {
            var level = new LevelBlueprint("Level_1-1", "Strandbad", Width, Height, StartCell)
            {
                Island = 0,
                Slot = 0,
                ThemePath = IslandThemes.StrandPath,
                // Proposed by StarThresholdProposer (issue #301, N33); the feel is checked on the device.
                StarScores = new Vector2Int(8450, 14100),
                // Target time for the badge „Blitz“ (issue #287, start value).
                BadgeSeconds = 240f,
                DifficultyPath = LevelDifficulties.Island1Path,
                // The exit lies on the last floor rows at the north end.
                ExitCenter = new Vector2(12f, Height - 2.25f),
                // The exit opens only after the last rush, so it cannot be skipped (playtest v0.3.0).
                ExitLockZone = 4,
            };
            level.Walls.AddRange(Obstacles);

            var gates = new int[DividerRows.Length];
            for (int i = 0; i < gates.Length; i++)
            {
                gates[i] = level.AddHorizontalDivider($"Gate_{i + 1}", DividerRows[i], 1, Width - 2, GateX, GateWidth);
            }

            LevelWaves[] waves = CreateWaves();
            int bottom = 1;
            for (int i = 0; i <= gates.Length; i++)
            {
                int top = i < gates.Length ? DividerRows[i] + 1 : Height - 1;
                var area = new RectInt(1, bottom, Width - 2, top - bottom);
                level.AddZone(new LevelZone($"Zone_{i + 1}", area, ZoneSpawnCells[i], waves[i], i < gates.Length ? gates[i] : -1));
                bottom = top;
            }

            level.Gates[gates[0]].Kills = KillGateKills;
            int hold = level.AddPuzzle(LevelPuzzle.HoldZone("HoldZone_Zone_2", 1, HoldZoneCenter)
                .WithPressure(HoldPressureWaves(), AfterPuzzle.EndWaves));
            int switches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_3", 2, SwitchPlates));
            int transport = level.AddPuzzle(
                LevelPuzzle.Transport("Transport_Zone_4", 3, GeneratorPosition, RequiredCells, CellPositions));
            level.Gates[gates[1]].Puzzles.Add(hold);
            level.Gates[gates[2]].Puzzles.Add(switches);
            level.Gates[gates[3]].Puzzles.Add(transport);
            // Wall weapons (issue #177): the island's new weapon at least once before its boss unlocks it.
            // Zone 4 Generator: on the long block, between the two cells.
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(12, 86), Combat.WeaponCatalogBuilder.ShotgunPath));
            return level;
        }

        // While the hold zone runs, zone 2 sends small waves at a fast pace; once it is held, the zone goes quiet.
        private static LevelWaves HoldPressureWaves()
        {
            return new LevelWaves(WavesPrefix + "Zone2_Hold.asset", (w, e) => w.Configure(
                new[]
                {
                    new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 1)),
                    new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 2)),
                },
                0.5f, 2f, AfterLastWave.RepeatLast));
        }

        // Each zone adds one enemy type: walkers, runners, boomers, brutes. Zones with a gate repeat their last wave
        // until it opens; the last zone ends after its waves.
        private static LevelWaves[] CreateWaves()
        {
            return new[]
            {
                new LevelWaves(WavesPrefix + "Zone1.asset", (w, e) => w.Configure(
                    new[] { new Wave(new WaveEntry(e.Walker, 4)), new Wave(new WaveEntry(e.Walker, 6)), new Wave(new WaveEntry(e.Walker, 8)) },
                    1f, 4f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone2.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 2)),
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 3)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone3.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 2)),
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 3), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Walker, 8), new WaveEntry(e.Runner, 4), new WaveEntry(e.Boomer, 2)),
                    },
                    0.7f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone4.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 2), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 8), new WaveEntry(e.Runner, 3), new WaveEntry(e.Brute, 1), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Walker, 8), new WaveEntry(e.Runner, 4), new WaveEntry(e.Brute, 2), new WaveEntry(e.Boomer, 2)),
                    },
                    0.6f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone5.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 4)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 4), new WaveEntry(e.Brute, 1), new WaveEntry(e.Boomer, 2)),
                    },
                    0.5f, 2f, AfterLastWave.End)),
            };
        }
    }
}
