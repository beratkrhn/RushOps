using System.Collections.Generic;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Level 2-1 „Hinterhöfe“ (issue #153): six zones (decision C2) in the dark city. The way leaves the main lane to
    /// the east into a side yard and comes back two zones later (side branch, decision L3). All enemy types are mixed,
    /// each zone with its own focus (C4); island 2 makes waves denser and hold zones longer (C3).
    /// <list type="number">
    /// <item>Hinterhof: kill gate counted from the level start; walkers.</item>
    /// <item>Gasse: three timed switches, the gate leads east into the side yard; boomers.</item>
    /// <item>Garagenhof (east): hold zone and timed switches at one gate (L1, L2); mixed.</item>
    /// <item>Müllplatz (east): two energy cells to the generator, the gate leads back west; brutes.</item>
    /// <item>Hinterhaus: kill gate that counts from entering the zone (P1); runners.</item>
    /// <item>Durchfahrt: a last rush of all types; the exit opens once it is beaten.</item>
    /// </list>
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level2_1SceneBuilder.Build</c>
    /// </summary>
    public static class Level2_1SceneBuilder
    {
        private const string WavesPrefix = "Level_2-1_";

        // The main lane takes columns 1..22, column 23 divides it from the side lane in columns 24..38.
        private const int Width = 40;
        private const int Height = 110;
        private const int MainLeft = 1;
        private const int MainRight = 22;
        private const int SideColumn = 23;
        private const int SideLeft = 24;
        private const int SideRight = 38;
        private const int GateWidth = 4;
        private static readonly Vector2Int StartCell = new Vector2Int(12, 3);

        // Rows and first cells of the gates in the order the player passes them.
        private const int HofRow = 24;
        private const int HofGateX = 9;
        private const int GasseGateY = 40;
        private const int GarageRow = 61;
        private const int GarageGateX = 29;
        private const int MuellGateY = 76;
        private const int HinterhausRow = 89;
        private const int HinterhausGateX = 3;

        // Main lane between the Gasse and the Hinterhaus: closed, the way runs through the side lane.
        private const int MainSplitRow = 53;

        // Side lane: solid below the Garagenhof and above the Müllplatz.
        private const int SideBottomRow = 24;
        private const int SideTopRow = 87;

        // Kills that open zone 1 (from level start) and zone 5 (from entering it).
        private const int HofKills = 12;
        private const int HinterhausKills = 20;

        // Zone 2: three switches around crates.
        private static readonly Vector2[] GasseSwitches =
        {
            new Vector2(4.5f, 30.5f), new Vector2(19.5f, 36.5f), new Vector2(5.5f, 49.5f),
        };

        // Zone 3: the hold zone right in front of the entry, the switches in the corners around it.
        private static readonly Vector2 GarageHoldCenter = new Vector2(31.5f, 42.5f);
        private static readonly Vector2[] GarageSwitches =
        {
            new Vector2(26.5f, 29.5f), new Vector2(36.5f, 30.5f), new Vector2(31.5f, 56.5f),
        };

        // Zone 4: the generator in sight of the entry, the cells at the far end.
        private static readonly Vector2 GeneratorPosition = new Vector2(35.5f, 66.5f);
        private static readonly Vector2[] CellPositions = { new Vector2(26.5f, 82.5f), new Vector2(36.5f, 84.5f) };
        private const int RequiredCells = 2;

        private static readonly RectInt[] ZoneAreas =
        {
            new RectInt(MainLeft, 1, MainRight, HofRow - 1),
            new RectInt(MainLeft, HofRow + 1, MainRight, MainSplitRow - HofRow - 1),
            new RectInt(SideLeft, SideBottomRow + 1, SideRight - SideLeft + 1, GarageRow - SideBottomRow - 1),
            new RectInt(SideLeft, GarageRow + 1, SideRight - SideLeft + 1, SideTopRow - GarageRow - 1),
            new RectInt(MainLeft, MainSplitRow + 1, MainRight, HinterhausRow - MainSplitRow - 1),
            new RectInt(MainLeft, HinterhausRow + 1, MainRight, Height - 2 - HinterhausRow),
        };

        private static readonly Vector2Int[][] ZoneSpawnCells =
        {
            new[] { new Vector2Int(2, 21), new Vector2Int(21, 21), new Vector2Int(12, 21), new Vector2Int(2, 12), new Vector2Int(21, 13) },
            new[] { new Vector2Int(2, 35), new Vector2Int(21, 30), new Vector2Int(2, 47), new Vector2Int(21, 50), new Vector2Int(12, 51) },
            new[] { new Vector2Int(26, 26), new Vector2Int(37, 26), new Vector2Int(37, 45), new Vector2Int(25, 58), new Vector2Int(37, 58) },
            new[] { new Vector2Int(25, 74), new Vector2Int(37, 74), new Vector2Int(25, 85), new Vector2Int(37, 85), new Vector2Int(31, 85) },
            new[] { new Vector2Int(2, 57), new Vector2Int(21, 57), new Vector2Int(2, 70), new Vector2Int(2, 86), new Vector2Int(12, 87) },
            new[] { new Vector2Int(2, 107), new Vector2Int(21, 107), new Vector2Int(21, 99), new Vector2Int(2, 104) },
        };

        private static readonly RectInt[] Obstacles =
        {
            // Side lane outside the side yard.
            new RectInt(SideColumn, 1, Width - 1 - SideColumn, SideBottomRow),
            new RectInt(SideColumn, SideTopRow, Width - 1 - SideColumn, Height - 1 - SideTopRow),
            // Main lane between Gasse and Hinterhaus.
            new RectInt(MainLeft, MainSplitRow, MainRight, 1),
            // Zone 1: bins and a low wall in the yard.
            new RectInt(4, 10, 2, 2),
            new RectInt(17, 8, 3, 2),
            new RectInt(9, 16, 5, 1),
            // Zone 2: crates in the alley.
            new RectInt(8, 32, 2, 2),
            new RectInt(4, 40, 2, 2),
            new RectInt(14, 44, 3, 2),
            // Zone 3: two car wrecks, clear of the hold zone.
            new RectInt(33, 36, 2, 2),
            new RectInt(27, 47, 3, 2),
            // Zone 4: dumpsters.
            new RectInt(28, 72, 4, 2),
            new RectInt(33, 79, 2, 2),
            // Zone 5: the back house's pillars and a shed.
            new RectInt(6, 60, 3, 2),
            new RectInt(14, 66, 2, 2),
            new RectInt(5, 76, 2, 3),
            new RectInt(12, 82, 4, 2),
            // Zone 6: crates along the passage.
            new RectInt(15, 95, 3, 2),
            new RectInt(4, 100, 2, 2),
        };

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Level_2-1 erzeugen")]
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
            var level = new LevelBlueprint("Level_2-1", "Hinterhöfe", Width, Height, StartCell)
            {
                Island = 1,
                Slot = 0,
                ThemePath = IslandThemes.StadtPath,
                // Proposed by StarThresholdProposer (issue #301, N33); the feel is checked on the device.
                StarScores = new Vector2Int(10400, 17300),
                // Target time for the badge „Blitz“ (issue #287, start value).
                BadgeSeconds = 270f,
                DifficultyPath = LevelDifficulties.Island2Path,
                ExitCenter = new Vector2(12f, Height - 2.25f),
                ExitLockZone = 5,
            };
            level.Walls.AddRange(Obstacles);

            // In the order the player passes them; the zones' exit gates refer to these indices.
            int hof = level.AddHorizontalDivider("Gate_1", HofRow, MainLeft, MainRight, HofGateX, GateWidth);
            int gasse = level.AddVerticalDivider("Gate_2", SideColumn, HofRow + 1, GarageRow - 1, GasseGateY, GateWidth);
            int garage = level.AddHorizontalDivider("Gate_3", GarageRow, SideLeft, SideRight, GarageGateX, GateWidth);
            int muell = level.AddVerticalDivider("Gate_4", SideColumn, GarageRow, SideTopRow - 1, MuellGateY, GateWidth);
            int hinterhaus = level.AddHorizontalDivider(
                "Gate_5", HinterhausRow, MainLeft, MainRight, HinterhausGateX, GateWidth);
            int[] exits = { hof, gasse, garage, muell, hinterhaus, -1 };

            LevelWaves[] waves = CreateWaves();
            for (int i = 0; i < ZoneAreas.Length; i++)
            {
                level.AddZone(new LevelZone($"Zone_{i + 1}", ZoneAreas[i], ZoneSpawnCells[i], waves[i], exits[i]));
            }

            int gasseSwitches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_2", 1, GasseSwitches));
            int garageHold = level.AddPuzzle(LevelPuzzle.HoldZone("HoldZone_Zone_3", 2, GarageHoldCenter));
            int garageSwitches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_3", 2, GarageSwitches));
            int transport = level.AddPuzzle(
                LevelPuzzle.Transport("Transport_Zone_4", 3, GeneratorPosition, RequiredCells, CellPositions));

            level.Gates[hof].Kills = HofKills;
            level.Gates[gasse].Puzzles.Add(gasseSwitches);
            level.Gates[garage].Puzzles.Add(garageHold);
            level.Gates[garage].Puzzles.Add(garageSwitches);
            level.Gates[muell].Puzzles.Add(transport);
            level.Gates[hinterhaus].Kills = HinterhausKills;
            level.Gates[hinterhaus].CountKillsFromZone = 4;
            // Wall weapons (issue #177): the island's new weapon at least once before its boss unlocks it.
            // Garagenhof, the eastern side branch: reward for the detour.
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(33, 36), Combat.WeaponCatalogBuilder.AssaultRiflePath));
            // Zone 5 Hinterhaus.
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(13, 82), Combat.WeaponCatalogBuilder.ShotgunPath));
            return level;
        }

        // All types from the start (decision C4); each zone leans on one type. Island 2 density comes from the
        // difficulty asset (C3), not from these spacings.
        private static LevelWaves[] CreateWaves()
        {
            return new[]
            {
                new LevelWaves(WavesPrefix + "Zone1.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 1)),
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 2), new WaveEntry(e.Boomer, 1)),
                    },
                    0.9f, 3.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone2.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Boomer, 2)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 2), new WaveEntry(e.Boomer, 2)),
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 2), new WaveEntry(e.Boomer, 3)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone3.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 2), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 3), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 3), new WaveEntry(e.Boomer, 1), new WaveEntry(e.Brute, 1)),
                    },
                    0.7f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone4.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 2), new WaveEntry(e.Brute, 2)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 3), new WaveEntry(e.Brute, 2), new WaveEntry(e.Boomer, 1)),
                    },
                    0.7f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone5.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Runner, 5), new WaveEntry(e.Walker, 2)),
                        new Wave(new WaveEntry(e.Runner, 6), new WaveEntry(e.Walker, 3), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Runner, 7), new WaveEntry(e.Walker, 2), new WaveEntry(e.Boomer, 1)),
                    },
                    0.6f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone6.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 4), new WaveEntry(e.Boomer, 2)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 5), new WaveEntry(e.Brute, 1), new WaveEntry(e.Boomer, 2)),
                    },
                    0.5f, 2f, AfterLastWave.End)),
            };
        }
    }
}
