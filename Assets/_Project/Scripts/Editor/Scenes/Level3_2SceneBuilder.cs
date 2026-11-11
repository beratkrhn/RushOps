using System.Collections.Generic;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Level 3-2 „Lavastrom“ (issue #158): the hardest regular level. Six zones (decision C2); the gates zigzag from
    /// side to side along the lava stream, so every zone is crossed diagonally. Two gates combine two puzzles (L2),
    /// island 3 shortens switch time limits by 20 % and leans on runners and boomers (C3).
    /// <list type="number">
    /// <item>Aschebank: kill gate counted from the level start; walkers and runners.</item>
    /// <item>Lavafall: hold zone <b>and</b> two timed switches at one gate; boomers.</item>
    /// <item>Schmelzkessel: generator and two energy cells while a kill gate counts from entering the zone; mixed.</item>
    /// <item>Glutbrücke: four timed switches; runners.</item>
    /// <item>Magmakammer: hold zone <b>and</b> a second generator with cells at one gate; mixed.</item>
    /// <item>Lavaufstieg: a last rush of all types; the gate north opens once it is beaten.</item>
    /// <item>Druckkammer (issue #291): a pressure plate only a brute pushes down; dog packs.</item>
    /// <item>Schlackewand (issue #291): a cracked wall only a boomer breaks; dog packs.</item>
    /// <item>Gipfel: the exit.</item>
    /// </list>
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level3_2SceneBuilder.Build</c>
    /// </summary>
    public static class Level3_2SceneBuilder
    {
        private const string WavesPrefix = "Level_3-2_";

        private const int Width = 28;
        private const int Height = 174;
        private const int GateWidth = 4;
        private static readonly int[] DividerRows = { 22, 44, 66, 88, 112, 133, 151, 165 };

        // First cell of each gate: they alternate between the east and the west side.
        private static readonly int[] GateXs = { 21, 3, 21, 3, 12, 12, 3, 12 };
        private static readonly Vector2Int StartCell = new Vector2Int(14, 3);

        private const int AscheKills = 18;
        private const int KesselKills = 18;

        // Zone 2: the hold zone mid-field, switches in the far corners (both at one gate).
        private static readonly Vector2 FallHoldCenter = new Vector2(14.5f, 31.5f);
        private static readonly Vector2[] FallSwitches = { new Vector2(4.5f, 40.5f), new Vector2(23.5f, 40.5f) };

        // Zone 3: the generator beside the entry, the cells on the far side of the ridge.
        private static readonly Vector2 KesselGenerator = new Vector2(6.5f, 50.5f);
        private static readonly Vector2[] KesselCells = { new Vector2(22.5f, 56.5f), new Vector2(24.5f, 62.5f) };

        // Zone 4: four switches around the obstacles.
        private static readonly Vector2[] BrueckeSwitches =
        {
            new Vector2(23.5f, 72.5f), new Vector2(4.5f, 74.5f), new Vector2(23.5f, 82.5f), new Vector2(4.5f, 84.5f),
        };

        // Zone 5: hold zone mid-field, a generator at the entry and the cells in the far corners.
        private static readonly Vector2 KammerHoldCenter = new Vector2(14.5f, 96.5f);
        private static readonly Vector2 KammerGenerator = new Vector2(5.5f, 94.5f);
        private static readonly Vector2[] KammerCells = { new Vector2(23.5f, 100.5f), new Vector2(5.5f, 104.5f) };
        private const int RequiredCells = 2;

        // Zones 7 and 8 (issue #291): the plate mid-field, then the cracked wall in the last gate.
        private static readonly Vector2 PlateCenter = new Vector2(14f, 142f);

        private static readonly RectInt[] Obstacles =
        {
            // Zone 1: ash dunes.
            new RectInt(6, 8, 4, 2),
            new RectInt(18, 11, 3, 2),
            new RectInt(10, 16, 8, 1),
            // Zone 2: lava fall rocks, clear of the hold zone.
            new RectInt(9, 38, 4, 2),
            new RectInt(17, 37, 3, 2),
            // Zone 3: the ridge between generator and cells.
            new RectInt(11, 56, 10, 1),
            new RectInt(6, 60, 3, 2),
            // Zone 4: slag blocks between the switches.
            new RectInt(9, 76, 4, 2),
            new RectInt(15, 80, 4, 2),
            // Zone 5: pillars around the hold zone.
            new RectInt(9, 102, 4, 2),
            new RectInt(18, 91, 3, 2),
            // Zone 6: rocks before the exit.
            new RectInt(6, 118, 3, 2),
            new RectInt(19, 118, 3, 2),
            new RectInt(17, 124, 4, 2),
            // Zone 7: basalt blocks, clear of the plate.
            new RectInt(6, 138, 3, 2),
            new RectInt(19, 146, 3, 2),
            // Zone 8: slag heaps at the sides, the way from the entry to the wall stays in sight.
            new RectInt(19, 155, 3, 2),
            new RectInt(6, 160, 3, 2),
        };

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Level_3-2 erzeugen")]
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
            var level = new LevelBlueprint("Level_3-2", "Lavastrom", Width, Height, StartCell)
            {
                Island = 2,
                Slot = 1,
                ThemePath = IslandThemes.VulkanPath,
                StarScores = new Vector2Int(2600, 3700),
                // Target time for the badge „Blitz“ (issue #287, start value).
                BadgeSeconds = 420f,
                DifficultyPath = LevelDifficulties.Island3Path,
                ExitCenter = new Vector2(14f, Height - 2.25f),
                // The two new zones have zombie dogs (A4, F-GEG-07); only their waves send them.
                Dogs = true,
            };
            level.Walls.AddRange(Obstacles);

            var gates = new int[DividerRows.Length];
            for (int i = 0; i < gates.Length; i++)
            {
                gates[i] = level.AddHorizontalDivider(
                    $"Gate_{i + 1}", DividerRows[i], 1, Width - 2, GateXs[i], GateWidth);
            }

            LevelWaves[] waves = CreateWaves();
            int bottom = 1;
            for (int i = 0; i <= gates.Length; i++)
            {
                int top = i < gates.Length ? DividerRows[i] : Height - 2;
                var area = new RectInt(1, bottom, Width - 2, top - bottom + 1);
                // The exit zone is too short for the usual spawn points at its entry side.
                Vector2Int[] spawns = i < gates.Length ? SpawnCells(bottom, top) : new[] { new Vector2Int(2, top - 2), new Vector2Int(25, top - 2) };
                level.AddZone(new LevelZone($"Zone_{i + 1}", area, spawns, waves[i], i < gates.Length ? gates[i] : -1));
                bottom = top + 1;
            }

            int fallHold = level.AddPuzzle(LevelPuzzle.HoldZone("HoldZone_Zone_2", 1, FallHoldCenter));
            int fallSwitches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_2", 1, FallSwitches));
            int kessel = level.AddPuzzle(
                LevelPuzzle.Transport("Transport_Zone_3", 2, KesselGenerator, RequiredCells, KesselCells));
            int bruecke = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_4", 3, BrueckeSwitches));
            int kammerHold = level.AddPuzzle(LevelPuzzle.HoldZone("HoldZone_Zone_5", 4, KammerHoldCenter));
            int kammer = level.AddPuzzle(
                LevelPuzzle.Transport("Transport_Zone_5", 4, KammerGenerator, RequiredCells, KammerCells));

            level.Gates[gates[0]].Kills = AscheKills;
            level.Gates[gates[1]].Puzzles.Add(fallHold);
            level.Gates[gates[1]].Puzzles.Add(fallSwitches);
            level.Gates[gates[2]].Puzzles.Add(kessel);
            level.Gates[gates[2]].Kills = KesselKills;
            level.Gates[gates[2]].CountKillsFromZone = 2;
            level.Gates[gates[3]].Puzzles.Add(bruecke);
            level.Gates[gates[4]].Puzzles.Add(kammerHold);
            level.Gates[gates[4]].Puzzles.Add(kammer);
            // Issue #291: the last rush must still be beaten, then a brute for the plate and a boomer for the wall.
            level.Gates[gates[5]].ClearZone = 5;
            int plate = level.AddPuzzle(LevelPuzzle.PressurePlate("PressurePlate_Zone_7", 6, PlateCenter));
            level.Gates[gates[6]].Puzzles.Add(plate);
            int wall = level.AddPuzzle(LevelPuzzle.CrackedWall("CrackedWall_Zone_8", 7, gates[7], level.Gates[gates[7]].Center));
            level.Gates[gates[7]].Puzzles.Add(wall);
            // Wall weapons (issue #177): the island's new weapon at least once before its boss unlocks it.
            // Zone 3 Schmelzkessel: on the ridge between generator and cells.
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(16, 56), Combat.WeaponCatalogBuilder.DualPistolsPath));
            return level;
        }

        // Spawn points on both sides and at the far end of a zone, none at the entry (level design rules).
        private static Vector2Int[] SpawnCells(int bottom, int top)
        {
            return new[]
            {
                new Vector2Int(2, top - 2), new Vector2Int(25, top - 2), new Vector2Int(14, top - 2),
                new Vector2Int(2, bottom + 9), new Vector2Int(25, bottom + 11),
            };
        }

        // All types from the start (decision C4); the hardest regular level leans on runners and boomers (C3).
        private static LevelWaves[] CreateWaves()
        {
            return new[]
            {
                new LevelWaves(WavesPrefix + "Zone1.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 4)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 4), new WaveEntry(e.Boomer, 2)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone2.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Boomer, 3), new WaveEntry(e.Runner, 3)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Boomer, 4), new WaveEntry(e.Runner, 4)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Boomer, 4), new WaveEntry(e.Runner, 4), new WaveEntry(e.Brute, 1)),
                    },
                    0.7f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone3.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 5), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 6), new WaveEntry(e.Boomer, 2), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 6), new WaveEntry(e.Boomer, 3), new WaveEntry(e.Brute, 2)),
                    },
                    0.6f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone4.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Runner, 7), new WaveEntry(e.Walker, 2), new WaveEntry(e.Boomer, 2)),
                        new Wave(new WaveEntry(e.Runner, 8), new WaveEntry(e.Walker, 2), new WaveEntry(e.Boomer, 3)),
                        new Wave(new WaveEntry(e.Runner, 9), new WaveEntry(e.Boomer, 3), new WaveEntry(e.Brute, 1)),
                    },
                    0.5f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone5.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 5), new WaveEntry(e.Boomer, 3), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 6), new WaveEntry(e.Boomer, 3), new WaveEntry(e.Brute, 2)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 7), new WaveEntry(e.Boomer, 4), new WaveEntry(e.Brute, 2)),
                    },
                    0.6f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone6.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 7), new WaveEntry(e.Boomer, 4)),
                        new Wave(new WaveEntry(e.Runner, 8), new WaveEntry(e.Brute, 3), new WaveEntry(e.Boomer, 4)),
                    },
                    0.5f, 2f, AfterLastWave.End)),
                // Pressure chamber (issue #291): dog packs with runners; the brutes come from the plate itself.
                new LevelWaves(WavesPrefix + "Zone7.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Runner, 3), new WaveEntry(e.Dog, e.Dog.PackSize)),
                        new Wave(new WaveEntry(e.Walker, 3), new WaveEntry(e.Runner, 3), new WaveEntry(e.Dog, e.Dog.PackSize)),
                    },
                    0.7f, 5f, AfterLastWave.RepeatLast)),
                // Slag wall (issue #291): dog packs with walkers; the boomers come from the wall itself.
                new LevelWaves(WavesPrefix + "Zone8.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 3), new WaveEntry(e.Dog, e.Dog.PackSize)),
                        new Wave(new WaveEntry(e.Walker, 3), new WaveEntry(e.Runner, 3), new WaveEntry(e.Dog, e.Dog.PackSize)),
                    },
                    0.7f, 5f, AfterLastWave.RepeatLast)),
                null,
            };
        }
    }
}
