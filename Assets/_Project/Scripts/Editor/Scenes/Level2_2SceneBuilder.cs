using System.Collections.Generic;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Level 2-2 „U-Bahn“ (issue #154): six zones (decision C2) in the dark city. The main lane runs in the east; after
    /// the platform the way branches west into the tunnel and the signal box and comes back two zones later (side
    /// branch, decision L3). All enemy types are mixed, each zone with its own focus (C4); island 2 makes waves denser
    /// and hold zones longer (C3).
    /// <list type="number">
    /// <item>Eingangshalle: kill gate counted from the level start; walkers.</item>
    /// <item>Bahnsteig: three timed switches, the gate leads west into the tunnel; boomers.</item>
    /// <item>Tunnel (west): two energy cells to the generator while a kill gate counts from entering the zone
    /// (L1, L2, P1); mixed.</item>
    /// <item>Stellwerk (west): hold zone, the gate leads back east; brutes.</item>
    /// <item>Gleise: four timed switches between two train wagons; runners.</item>
    /// <item>Ausgang: a last rush of all types; the exit opens once it is beaten.</item>
    /// </list>
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level2_2SceneBuilder.Build</c>
    /// </summary>
    public static class Level2_2SceneBuilder
    {
        private const string WavesPrefix = "Level_2-2_";

        // The side lane takes columns 1..15, column 16 divides it from the main lane in columns 17..38.
        private const int Width = 40;
        private const int Height = 110;
        private const int SideLeft = 1;
        private const int SideRight = 15;
        private const int SideColumn = 16;
        private const int MainLeft = 17;
        private const int MainRight = 38;
        private const int GateWidth = 4;
        private static readonly Vector2Int StartCell = new Vector2Int(27, 3);

        // Rows and first cells of the gates in the order the player passes them.
        private const int HalleRow = 24;
        private const int HalleGateX = 26;
        private const int BahnsteigGateY = 44;
        private const int TunnelRow = 61;
        private const int TunnelGateX = 6;
        private const int StellwerkGateY = 66;
        private const int GleiseRow = 89;
        private const int GleiseGateX = 32;

        // Main lane between the platform and the tracks: closed, the way runs through the side lane.
        private const int MainSplitRow = 53;

        // Side lane: solid below the tunnel and above the signal box.
        private const int SideBottomRow = 24;
        private const int SideTopRow = 87;

        // Kills that open zone 1 (from level start) and, together with the generator, zone 3 (from entering it).
        private const int HalleKills = 12;
        private const int TunnelKills = 15;

        // Zone 2: three switches along the platform.
        private static readonly Vector2[] BahnsteigSwitches =
        {
            new Vector2(35.5f, 30.5f), new Vector2(19.5f, 38.5f), new Vector2(34.5f, 49.5f),
        };

        // Zone 3: the generator in sight of the entry, the cells at both ends of the tunnel.
        private static readonly Vector2 GeneratorPosition = new Vector2(9.5f, 46.5f);
        private static readonly Vector2[] CellPositions = { new Vector2(3.5f, 28.5f), new Vector2(12.5f, 57.5f) };
        private const int RequiredCells = 2;

        // Zone 4: the hold zone straight ahead of the entry.
        private static readonly Vector2 StellwerkHoldCenter = new Vector2(8.5f, 72.5f);

        // Zone 5: four switches around the two wagons.
        private static readonly Vector2[] GleiseSwitches =
        {
            new Vector2(21.5f, 62.5f), new Vector2(35.5f, 60.5f), new Vector2(35.5f, 74.5f), new Vector2(21.5f, 78.5f),
        };

        private static readonly RectInt[] ZoneAreas =
        {
            new RectInt(MainLeft, 1, MainRight - MainLeft + 1, HalleRow - 1),
            new RectInt(MainLeft, HalleRow + 1, MainRight - MainLeft + 1, MainSplitRow - HalleRow - 1),
            new RectInt(SideLeft, SideBottomRow + 1, SideRight, TunnelRow - SideBottomRow - 1),
            new RectInt(SideLeft, TunnelRow + 1, SideRight, SideTopRow - TunnelRow - 1),
            new RectInt(MainLeft, MainSplitRow + 1, MainRight - MainLeft + 1, GleiseRow - MainSplitRow - 1),
            new RectInt(MainLeft, GleiseRow + 1, MainRight - MainLeft + 1, Height - 2 - GleiseRow),
        };

        private static readonly Vector2Int[][] ZoneSpawnCells =
        {
            new[] { new Vector2Int(18, 21), new Vector2Int(37, 21), new Vector2Int(28, 21), new Vector2Int(18, 12), new Vector2Int(37, 8) },
            new[] { new Vector2Int(18, 30), new Vector2Int(37, 33), new Vector2Int(37, 47), new Vector2Int(18, 51), new Vector2Int(28, 51) },
            new[] { new Vector2Int(2, 27), new Vector2Int(14, 27), new Vector2Int(2, 44), new Vector2Int(2, 58), new Vector2Int(14, 58) },
            new[] { new Vector2Int(2, 84), new Vector2Int(14, 84), new Vector2Int(8, 85), new Vector2Int(2, 70), new Vector2Int(14, 76) },
            new[] { new Vector2Int(37, 56), new Vector2Int(18, 56), new Vector2Int(37, 87), new Vector2Int(18, 87), new Vector2Int(29, 87) },
            new[] { new Vector2Int(18, 107), new Vector2Int(37, 107), new Vector2Int(18, 99), new Vector2Int(37, 98) },
        };

        private static readonly RectInt[] Obstacles =
        {
            // Side lane outside the tunnel and the signal box.
            new RectInt(SideLeft, 1, SideColumn, SideBottomRow),
            new RectInt(SideLeft, SideTopRow, SideColumn, Height - 1 - SideTopRow),
            // Main lane between the platform and the tracks.
            new RectInt(MainLeft, MainSplitRow, MainRight - MainLeft + 1, 1),
            // Zone 1: ticket machines and a barrier in the hall.
            new RectInt(20, 9, 3, 2),
            new RectInt(33, 11, 2, 2),
            new RectInt(25, 16, 4, 1),
            // Zone 2: benches on the platform.
            new RectInt(30, 34, 2, 2),
            new RectInt(22, 40, 3, 2),
            new RectInt(33, 41, 2, 2),
            // Zone 3: rubble in the tunnel.
            new RectInt(5, 36, 2, 2),
            new RectInt(10, 51, 3, 2),
            // Zone 4: switch cabinets, clear of the hold zone.
            new RectInt(3, 79, 2, 2),
            new RectInt(11, 80, 2, 2),
            // Zone 5: two train wagons and a crate.
            new RectInt(26, 64, 6, 2),
            new RectInt(26, 72, 6, 2),
            new RectInt(24, 83, 3, 2),
            // Zone 6: crates in front of the stairs.
            new RectInt(20, 96, 3, 2),
            new RectInt(33, 101, 2, 2),
        };

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Level_2-2 erzeugen")]
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
            var level = new LevelBlueprint("Level_2-2", "U-Bahn", Width, Height, StartCell)
            {
                Island = 1,
                Slot = 1,
                ThemePath = IslandThemes.StadtPath,
                StarScores = new Vector2Int(1700, 2400),
                DifficultyPath = LevelDifficulties.Island2Path,
                ExitCenter = new Vector2(27f, Height - 2.25f),
                ExitLockZone = 5,
            };
            level.Walls.AddRange(Obstacles);

            // In the order the player passes them; the zones' exit gates refer to these indices.
            int halle = level.AddHorizontalDivider("Gate_1", HalleRow, MainLeft, MainRight, HalleGateX, GateWidth);
            int bahnsteig = level.AddVerticalDivider(
                "Gate_2", SideColumn, HalleRow + 1, TunnelRow - 1, BahnsteigGateY, GateWidth);
            int tunnel = level.AddHorizontalDivider("Gate_3", TunnelRow, SideLeft, SideRight, TunnelGateX, GateWidth);
            int stellwerk = level.AddVerticalDivider(
                "Gate_4", SideColumn, TunnelRow, SideTopRow - 1, StellwerkGateY, GateWidth);
            int gleise = level.AddHorizontalDivider("Gate_5", GleiseRow, MainLeft, MainRight, GleiseGateX, GateWidth);
            int[] exits = { halle, bahnsteig, tunnel, stellwerk, gleise, -1 };

            LevelWaves[] waves = CreateWaves();
            for (int i = 0; i < ZoneAreas.Length; i++)
            {
                level.AddZone(new LevelZone($"Zone_{i + 1}", ZoneAreas[i], ZoneSpawnCells[i], waves[i], exits[i]));
            }

            int bahnsteigSwitches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_2", 1, BahnsteigSwitches));
            int transport = level.AddPuzzle(
                LevelPuzzle.Transport("Transport_Zone_3", 2, GeneratorPosition, RequiredCells, CellPositions));
            int hold = level.AddPuzzle(LevelPuzzle.HoldZone("HoldZone_Zone_4", 3, StellwerkHoldCenter));
            int gleiseSwitches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_5", 4, GleiseSwitches));

            level.Gates[halle].Kills = HalleKills;
            level.Gates[bahnsteig].Puzzles.Add(bahnsteigSwitches);
            level.Gates[tunnel].Puzzles.Add(transport);
            level.Gates[tunnel].Kills = TunnelKills;
            level.Gates[tunnel].CountKillsFromZone = 2;
            level.Gates[stellwerk].Puzzles.Add(hold);
            level.Gates[gleise].Puzzles.Add(gleiseSwitches);
            // Wall weapons (issue #177): the island's new weapon at least once before its boss unlocks it.
            // Tunnel, the western side branch, beside the generator.
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(5, 36), Combat.WeaponCatalogBuilder.AssaultRiflePath));
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
                        new Wave(new WaveEntry(e.Walker, 6), new WaveEntry(e.Runner, 2), new WaveEntry(e.Brute, 1)),
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
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 3)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 3), new WaveEntry(e.Boomer, 1), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 4), new WaveEntry(e.Brute, 1)),
                    },
                    0.7f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone4.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Brute, 1)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 2), new WaveEntry(e.Brute, 2)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 2), new WaveEntry(e.Brute, 2), new WaveEntry(e.Boomer, 1)),
                    },
                    0.7f, 3f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone5.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Runner, 5), new WaveEntry(e.Walker, 2)),
                        new Wave(new WaveEntry(e.Runner, 6), new WaveEntry(e.Walker, 3), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Runner, 7), new WaveEntry(e.Walker, 2), new WaveEntry(e.Brute, 1)),
                    },
                    0.6f, 2.5f, AfterLastWave.RepeatLast)),
                new LevelWaves(WavesPrefix + "Zone6.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 4), new WaveEntry(e.Boomer, 2)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 5), new WaveEntry(e.Brute, 2), new WaveEntry(e.Boomer, 1)),
                    },
                    0.5f, 2f, AfterLastWave.End)),
            };
        }
    }
}
