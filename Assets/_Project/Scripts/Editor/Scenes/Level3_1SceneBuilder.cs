using System.Collections.Generic;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Level 3-1 „Aschefeld“ (issue #157): six zones (decision C2) on the volcano island. Switch time limits are 20 %
    /// shorter and the waves hold more runners and boomers (C3, via the island 3 difficulty and the wave tables).
    /// <list type="number">
    /// <item>Aschefeld: kill gate counted from the level start; walkers and runners.</item>
    /// <item>Glutspalten: three timed switches between lava cracks; runners.</item>
    /// <item>Schlotfeld: hold zone <b>and</b> three timed switches at one gate (L2); boomers.</item>
    /// <item>Basaltgrat: generator and two energy cells while a kill gate counts from entering the zone; mixed.</item>
    /// <item>Aschehang: four timed switches; runners and boomers.</item>
    /// <item>Kraterrand: a last rush of all types; the exit opens once it is beaten.</item>
    /// </list>
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level3_1SceneBuilder.Build</c>
    /// </summary>
    public static class Level3_1SceneBuilder
    {
        private const string WavesPrefix = "Level_3-1_";

        private const int Width = 28;
        private const int Height = 134;
        private const int GateX = 12;
        private const int GateWidth = 4;
        private static readonly int[] DividerRows = { 22, 44, 66, 88, 112 };
        private static readonly Vector2Int StartCell = new Vector2Int(14, 3);

        // Kills that open zone 1 (from level start) and, together with the generator, zone 4 (from entering it).
        private const int AscheKills = 14;
        private const int GratKills = 16;

        // Zone 2: three switches around the cracks.
        private static readonly Vector2[] SpaltenSwitches =
        {
            new Vector2(4.5f, 29.5f), new Vector2(22.5f, 33.5f), new Vector2(13.5f, 39.5f),
        };

        // Zone 3: the hold zone in the middle, the switches in the corners around it (L2: both at one gate).
        private static readonly Vector2 SchlotHoldCenter = new Vector2(13.5f, 54.5f);
        private static readonly Vector2[] SchlotSwitches =
        {
            new Vector2(4.5f, 48.5f), new Vector2(23.5f, 50.5f), new Vector2(13.5f, 62.5f),
        };

        // Zone 4: the generator in sight of the entry, the cells behind the basalt ridge.
        private static readonly Vector2 GeneratorPosition = new Vector2(13.5f, 72.5f);
        private static readonly Vector2[] CellPositions = { new Vector2(3.5f, 84.5f), new Vector2(23.5f, 84.5f) };
        private const int RequiredCells = 2;

        // Zone 5: four switches in the corners.
        private static readonly Vector2[] HangSwitches =
        {
            new Vector2(4.5f, 94.5f), new Vector2(23.5f, 95.5f), new Vector2(4.5f, 105.5f), new Vector2(23.5f, 104.5f),
        };

        private static readonly RectInt[] Obstacles =
        {
            // Zone 1: rocks on the ash field.
            new RectInt(5, 8, 3, 2),
            new RectInt(20, 8, 3, 2),
            new RectInt(10, 14, 8, 1),
            // Zone 2: lava cracks.
            new RectInt(8, 28, 3, 3),
            new RectInt(16, 35, 4, 2),
            new RectInt(6, 37, 3, 2),
            // Zone 3: basalt columns, clear of the hold zone.
            new RectInt(6, 58, 3, 2),
            new RectInt(19, 58, 3, 2),
            // Zone 4: the basalt ridge in front of the cells.
            new RectInt(8, 78, 12, 1),
            new RectInt(12, 84, 4, 2),
            // Zone 5: a rock pile between the switches.
            new RectInt(10, 99, 8, 2),
            // Zone 6: rocks before the crater rim.
            new RectInt(6, 118, 3, 2),
            new RectInt(19, 118, 3, 2),
            new RectInt(17, 124, 4, 2),
        };

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Level_3-1 erzeugen")]
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
            var level = new LevelBlueprint("Level_3-1", "Aschefeld", Width, Height, StartCell)
            {
                Island = 2,
                Slot = 0,
                ThemePath = IslandThemes.VulkanPath,
                StarScores = new Vector2Int(1900, 2700),
                DifficultyPath = LevelDifficulties.Island3Path,
                ExitCenter = new Vector2(14f, Height - 2.25f),
                ExitLockZone = 5,
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
                int top = i < gates.Length ? DividerRows[i] : Height - 2;
                var area = new RectInt(1, bottom, Width - 2, top - bottom + 1);
                level.AddZone(new LevelZone(
                    $"Zone_{i + 1}", area, SpawnCells(bottom, top), waves[i], i < gates.Length ? gates[i] : -1));
                bottom = top + 1;
            }

            int spalten = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_2", 1, SpaltenSwitches));
            int schlotHold = level.AddPuzzle(LevelPuzzle.HoldZone("HoldZone_Zone_3", 2, SchlotHoldCenter));
            int schlotSwitches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_3", 2, SchlotSwitches));
            int transport = level.AddPuzzle(
                LevelPuzzle.Transport("Transport_Zone_4", 3, GeneratorPosition, RequiredCells, CellPositions));
            int hang = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_5", 4, HangSwitches));

            level.Gates[gates[0]].Kills = AscheKills;
            level.Gates[gates[1]].Puzzles.Add(spalten);
            level.Gates[gates[2]].Puzzles.Add(schlotHold);
            level.Gates[gates[2]].Puzzles.Add(schlotSwitches);
            level.Gates[gates[3]].Puzzles.Add(transport);
            level.Gates[gates[3]].Kills = GratKills;
            level.Gates[gates[3]].CountKillsFromZone = 3;
            level.Gates[gates[4]].Puzzles.Add(hang);
            return level;
        }

        // Spawn points on both sides and at the far end of a zone, none at the entry (level design rules).
        private static Vector2Int[] SpawnCells(int bottom, int top)
        {
            return new[]
            {
                new Vector2Int(2, top - 2), new Vector2Int(25, top - 2), new Vector2Int(8, top - 2),
                new Vector2Int(2, bottom + 9), new Vector2Int(25, bottom + 11),
            };
        }

        // All types from the start (decision C4); island 3 leans on runners and boomers (C3). The wave density and
        // switch times come from the difficulty asset.
        private static LevelWaves[] CreateWaves()
        {
            return new[]
            {
                new LevelWaves(WavesPrefix + "Zone1.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 3)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 3), new WaveEntry(e.Boomer, 1)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone2.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Runner, 5), new WaveEntry(e.Walker, 3)),
                        new Wave(new WaveEntry(e.Runner, 6), new WaveEntry(e.Walker, 3), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Runner, 7), new WaveEntry(e.Walker, 2), new WaveEntry(e.Boomer, 2)),
                    },
                    0.7f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone3.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Boomer, 3)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 3), new WaveEntry(e.Boomer, 3)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 4), new WaveEntry(e.Boomer, 3), new WaveEntry(e.Brute, 1)),
                    },
                    0.7f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone4.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 4), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 5), new WaveEntry(e.Boomer, 2), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 5), new WaveEntry(e.Boomer, 2), new WaveEntry(e.Brute, 2)),
                    },
                    0.7f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone5.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Runner, 6), new WaveEntry(e.Boomer, 2)),
                        new Wave(new WaveEntry(e.Runner, 7), new WaveEntry(e.Boomer, 3), new WaveEntry(e.Walker, 2)),
                        new Wave(new WaveEntry(e.Runner, 8), new WaveEntry(e.Boomer, 3), new WaveEntry(e.Brute, 1)),
                    },
                    0.6f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone6.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 6), new WaveEntry(e.Boomer, 3)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 7), new WaveEntry(e.Brute, 2), new WaveEntry(e.Boomer, 3)),
                    },
                    0.5f, 2f, AfterLastWave.End)),
            };
        }
    }
}
