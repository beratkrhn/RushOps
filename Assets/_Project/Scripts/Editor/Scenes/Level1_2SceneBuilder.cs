using System.Collections.Generic;
using RushOps.Enemies;
using RushOps.Puzzles;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Level 1-2 „Strandpromenade“ (issue #149): five zones from south to north, all four enemy types mixed from the
    /// start, each zone with its own focus (decision C4).
    /// <list type="number">
    /// <item>Promenade: hold zone right at the start; walkers.</item>
    /// <item>Kioske: four timed switches on two levels split by a long wall; boomers.</item>
    /// <item>Strandbar: kill gate that counts from entering the zone (decision P1); runners.</item>
    /// <item>Seebrücke: two energy cells behind a pier wall, carried to the generator; brutes.</item>
    /// <item>Hafen: a last rush of all types; the exit opens once it is beaten.</item>
    /// </list>
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level1_2SceneBuilder.Build</c>
    /// </summary>
    public static class Level1_2SceneBuilder
    {
        private const string WavesPrefix = "Level_1-2_";

        private const int Width = 24;
        private const int Height = 120;
        private const int GateWidth = 4;
        private static readonly Vector2Int StartCell = new Vector2Int(12, 3);

        // Rows of the walls between the zones and the first column of their gates; the gates change sides.
        private static readonly int[] DividerRows = { 24, 52, 76, 102 };
        private static readonly int[] GateColumns = { 10, 3, 17, 10 };

        // Kills after entering zone 3 that open its gate.
        private const int StrandbarKills = 18;

        // Zone 1: hold zone in sight of the start, between four crates.
        private static readonly Vector2 HoldZoneCenter = new Vector2(12f, 14f);

        // Zone 2: a wall in row 38 splits the zone into a lower and an upper level, open at both ends.
        private static readonly Vector2[] SwitchPlates =
        {
            new Vector2(4.5f, 34.5f), new Vector2(19.5f, 30.5f), new Vector2(20.5f, 42.5f), new Vector2(6.5f, 46.5f),
        };

        // Zone 4: the generator right behind the entry, the cells north of the pier wall.
        private static readonly Vector2 GeneratorPosition = new Vector2(19.5f, 81.5f);
        private static readonly Vector2[] CellPositions = { new Vector2(3.5f, 96.5f), new Vector2(20.5f, 97.5f) };
        private const int RequiredCells = 2;

        private static readonly Vector2Int[][] ZoneSpawnCells =
        {
            new[] { new Vector2Int(2, 22), new Vector2Int(21, 22), new Vector2Int(12, 21), new Vector2Int(2, 12), new Vector2Int(21, 12) },
            new[] { new Vector2Int(2, 32), new Vector2Int(21, 35), new Vector2Int(2, 44), new Vector2Int(21, 47), new Vector2Int(12, 49) },
            new[] { new Vector2Int(2, 74), new Vector2Int(21, 74), new Vector2Int(12, 73), new Vector2Int(2, 62), new Vector2Int(21, 65) },
            new[] { new Vector2Int(2, 99), new Vector2Int(21, 100), new Vector2Int(12, 99), new Vector2Int(2, 88), new Vector2Int(21, 90) },
            new[] { new Vector2Int(2, 116), new Vector2Int(21, 116), new Vector2Int(2, 111), new Vector2Int(21, 111) },
        };

        private static readonly RectInt[] Obstacles =
        {
            // Zone 1: beach crates around the hold zone.
            new RectInt(4, 9, 3, 2),
            new RectInt(17, 9, 3, 2),
            new RectInt(5, 18, 2, 2),
            new RectInt(17, 18, 2, 2),
            // Zone 2: the wall between the two levels and two kiosks.
            new RectInt(5, 38, 14, 1),
            new RectInt(8, 30, 2, 2),
            new RectInt(13, 44, 3, 2),
            // Zone 3: the bar counter in the middle and two pillars.
            new RectInt(8, 62, 8, 2),
            new RectInt(3, 70, 2, 2),
            new RectInt(19, 58, 2, 2),
            // Zone 4: the pier wall, open at both ends, and two crates.
            new RectInt(6, 84, 12, 2),
            new RectInt(9, 92, 2, 2),
            new RectInt(14, 96, 2, 2),
            // Zone 5: two crates in front of the exit.
            new RectInt(5, 108, 3, 2),
            new RectInt(16, 108, 3, 2),
        };

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Level_1-2 erzeugen")]
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
            var level = new LevelBlueprint("Level_1-2", "Strandpromenade", Width, Height, StartCell)
            {
                Island = 0,
                Slot = 1,
                ThemePath = IslandThemes.StrandPath,
                StarScores = new Vector2Int(1450, 2000),
                DifficultyPath = LevelDifficulties.Island1Path,
                ExitCenter = new Vector2(12f, Height - 2.25f),
                ExitLockZone = 4,
            };
            level.Walls.AddRange(Obstacles);

            var gates = new int[DividerRows.Length];
            for (int i = 0; i < gates.Length; i++)
            {
                gates[i] = level.AddHorizontalDivider($"Gate_{i + 1}", DividerRows[i], 1, Width - 2, GateColumns[i], GateWidth);
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

            int hold = level.AddPuzzle(LevelPuzzle.HoldZone("HoldZone_Zone_1", 0, HoldZoneCenter)
                .WithPressure(HoldPressureWaves(), AfterPuzzle.EndWaves));
            int switches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_2", 1, SwitchPlates));
            int transport = level.AddPuzzle(
                LevelPuzzle.Transport("Transport_Zone_4", 3, GeneratorPosition, RequiredCells, CellPositions));
            level.Gates[gates[0]].Puzzles.Add(hold);
            level.Gates[gates[1]].Puzzles.Add(switches);
            level.Gates[gates[2]].Kills = StrandbarKills;
            level.Gates[gates[2]].CountKillsFromZone = 2;
            level.Gates[gates[3]].Puzzles.Add(transport);
            // Wall weapons (issue #177): the island's new weapon at least once before its boss unlocks it.
            // Zone 3 Strandbar: on the bar counter.
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(12, 62), Combat.WeaponCatalogBuilder.ShotgunPath));
            return level;
        }

        private static LevelWaves HoldPressureWaves()
        {
            return new LevelWaves(WavesPrefix + "Zone1_Hold.asset", (w, e) => w.Configure(
                new[]
                {
                    new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 2)),
                    new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 2), new WaveEntry(e.Boomer, 1)),
                },
                0.5f, 2f, AfterLastWave.RepeatLast));
        }

        // All types from the start (decision C4); each zone leans on one type.
        private static LevelWaves[] CreateWaves()
        {
            return new[]
            {
                new LevelWaves(WavesPrefix + "Zone1.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 1)),
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 2)),
                    },
                    0.9f, 3.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone2.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 2), new WaveEntry(e.Boomer, 2)),
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 2), new WaveEntry(e.Boomer, 2)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone3.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Runner, 4), new WaveEntry(e.Walker, 2)),
                        new Wave(new WaveEntry(e.Runner, 5), new WaveEntry(e.Walker, 3), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Runner, 6), new WaveEntry(e.Walker, 2), new WaveEntry(e.Boomer, 1)),
                    },
                    0.6f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone4.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 2), new WaveEntry(e.Brute, 1), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 3), new WaveEntry(e.Brute, 2)),
                    },
                    0.7f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone5.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 4), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 4), new WaveEntry(e.Brute, 1), new WaveEntry(e.Boomer, 2)),
                    },
                    0.5f, 2f, AfterLastWave.End)),
            };
        }
    }
}
