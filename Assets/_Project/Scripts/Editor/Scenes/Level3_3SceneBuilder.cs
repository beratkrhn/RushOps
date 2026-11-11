using System.Collections.Generic;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Level 3-3 „Krater“ (issue #159), the finale: two short zones before the arena (decision C6), then the fight
    /// against the Mega-Boomer. Island 3 shortens switch time limits and leans on runners and boomers (C3). Beating it
    /// shows „Alle Inseln geschafft!“ on the done screen, then the map (C12).
    /// <list type="number">
    /// <item>Schlackefeld: kill gate between slag heaps; walkers, runners and boomers.</item>
    /// <item>Kraterrand: two timed switches at the far corners; mixed.</item>
    /// <item>Krater: open arena; the door closes behind the player and the Mega-Boomer starts.</item>
    /// </list>
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level3_3SceneBuilder.Build</c>
    /// </summary>
    public static class Level3_3SceneBuilder
    {
        /// <summary>Line on the done screen after the last boss (decision C12).</summary>
        public const string CompletionMessage = "Alle Inseln geschafft!";

        private const string WavesPrefix = "Level_3-3_";

        // As wide as level 2-3: the arena needs room to walk round the puddles.
        private const int Width = 28;
        private const int Height = 84;
        private const int GateX = 12;
        private const int GateWidth = 4;
        private static readonly int[] DividerRows = { 18, 36 };
        // Two rows behind the second gate still belong to zone 2, so the door closes behind the player.
        private const int ArenaBottom = 39;
        private const int SchlackeKills = 16;
        private static readonly Vector2Int StartCell = new Vector2Int(14, 3);
        private static readonly Vector2 BossStart = new Vector2(14f, 70f);

        private static readonly Vector2[] RandSwitches = { new Vector2(3.5f, 27.5f), new Vector2(24.5f, 27.5f) };

        private static readonly Vector2Int[][] ZoneSpawnCells =
        {
            new[] { new Vector2Int(2, 16), new Vector2Int(25, 16), new Vector2Int(2, 3), new Vector2Int(25, 3), new Vector2Int(14, 16) },
            new[] { new Vector2Int(2, 21), new Vector2Int(25, 21), new Vector2Int(8, 34), new Vector2Int(19, 34) },
        };

        private static readonly RectInt[] Obstacles =
        {
            // Zone 1: slag heaps, with ways round both sides.
            new RectInt(5, 8, 4, 2),
            new RectInt(19, 8, 4, 2),
            new RectInt(11, 12, 6, 2),
            // Zone 2: rocks on the crater rim between the switches.
            new RectInt(7, 30, 3, 2),
            new RectInt(18, 30, 3, 2),
        };

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Level_3-3 erzeugen")]
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
            var level = new LevelBlueprint("Level_3-3", "Krater", Width, Height, StartCell)
            {
                Island = 2,
                Slot = 2,
                ThemePath = IslandThemes.VulkanPath,
                StarScores = new Vector2Int(1700, 2300),
                // Target time for the badge „Blitz“ (issue #287, start value).
                BadgeSeconds = 210f,
                DifficultyPath = LevelDifficulties.Island3Path,
                CompletionMessage = CompletionMessage,
            };
            level.Walls.AddRange(Obstacles);
            int schlacke = level.AddHorizontalDivider("Gate_1", DividerRows[0], 1, Width - 2, GateX, GateWidth);
            int rand = level.AddHorizontalDivider("Gate_2", DividerRows[1], 1, Width - 2, GateX, GateWidth);
            level.Gates[schlacke].Kills = SchlackeKills;

            level.AddZone(new LevelZone("Zone_1", new RectInt(1, 1, Width - 2, DividerRows[0]), ZoneSpawnCells[0],
                new LevelWaves(WavesPrefix + "Zone1.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 3), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 4), new WaveEntry(e.Boomer, 2)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)), schlacke));
            int zone2 = level.AddZone(new LevelZone("Zone_2",
                new RectInt(1, DividerRows[0] + 1, Width - 2, ArenaBottom - DividerRows[0] - 1), ZoneSpawnCells[1],
                new LevelWaves(WavesPrefix + "Zone2.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 3), new WaveEntry(e.Runner, 4), new WaveEntry(e.Boomer, 2)),
                        new Wave(new WaveEntry(e.Walker, 3), new WaveEntry(e.Runner, 4), new WaveEntry(e.Boomer, 2), new WaveEntry(e.Brute, 1)),
                    },
                    0.7f, 3f, AfterLastWave.RepeatLast)), rand));
            int arena = level.AddZone(new LevelZone(
                "Zone_Arena", new RectInt(1, ArenaBottom, Width - 2, Height - 1 - ArenaBottom), null, null, -1));

            int switches = level.AddPuzzle(LevelPuzzle.TimedSwitches("Switches_Zone_2", zone2, RandSwitches));
            level.Gates[rand].Puzzles.Add(switches);
            level.Boss = new LevelBoss(MegaBoomerBuilder.DefinitionPath, BossStart, arena, rand, MegaBoomerBuilder.Configure)
            {
                DefinitionType = typeof(RushOps.Bosses.MegaBoomerDefinition),
                Create = MegaBoomerBuilder.Create,
                Connect = MegaBoomerBuilder.Connect,
            };
            // Wall weapons (issue #177): the island's new weapon at least once before its boss unlocks it.
            // Zone 2 Kraterrand before the arena, on the west rock.
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(8, 30), Combat.WeaponCatalogBuilder.DualPistolsPath));
            return level;
        }
    }
}
