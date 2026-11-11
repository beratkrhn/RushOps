using System.Collections.Generic;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Level 2-3 „Kanalisation“ (issue #155): two short zones before the arena (decision C6), then the fight against
    /// the Brutmutter. Island 2 makes waves denser and hold zones longer (C3).
    /// <list type="number">
    /// <item>Abwasserkanal: kill gate between the sewage channels; walkers and runners.</item>
    /// <item>Pumpwerk: hold zone between two pumps; mixed.</item>
    /// <item>Brutkammer: open arena; the door closes behind the player and the Brutmutter starts giving birth.</item>
    /// </list>
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.Level2_3SceneBuilder.Build</c>
    /// </summary>
    public static class Level2_3SceneBuilder
    {
        private const string WavesPrefix = "Level_2-3_";

        // Wider than level 1-3: the arena needs room to run from the runners.
        private const int Width = 28;
        private const int Height = 82;
        private const int GateX = 12;
        private const int GateWidth = 4;
        private static readonly int[] DividerRows = { 18, 36 };
        // Two rows behind the second gate still belong to zone 2, so the door closes behind the player.
        private const int ArenaBottom = 39;
        private const int KanalKills = 14;
        private static readonly Vector2Int StartCell = new Vector2Int(14, 3);
        private static readonly Vector2 BossStart = new Vector2(14f, 68f);

        private static readonly Vector2 HoldCenter = new Vector2(14f, 27.5f);

        private static readonly Vector2Int[][] ZoneSpawnCells =
        {
            new[] { new Vector2Int(2, 16), new Vector2Int(25, 16), new Vector2Int(2, 3), new Vector2Int(25, 3), new Vector2Int(14, 16) },
            new[] { new Vector2Int(2, 21), new Vector2Int(25, 21), new Vector2Int(2, 34), new Vector2Int(25, 34) },
        };

        private static readonly RectInt[] Obstacles =
        {
            // Zone 1: sewage channels across the tunnel, with ways round both ends.
            new RectInt(4, 8, 7, 1),
            new RectInt(17, 8, 7, 1),
            new RectInt(9, 12, 10, 1),
            // Zone 2: two pumps beside the hold zone.
            new RectInt(4, 26, 3, 3),
            new RectInt(21, 26, 3, 3),
        };

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Level_2-3 erzeugen")]
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
            var level = new LevelBlueprint("Level_2-3", "Kanalisation", Width, Height, StartCell)
            {
                Island = 1,
                Slot = 2,
                ThemePath = IslandThemes.StadtPath,
                // Proposed by StarThresholdProposer (issue #301, N33); the feel is checked on the device.
                StarScores = new Vector2Int(4500, 7550),
                // Target time for the badge „Blitz“ (issue #287, start value).
                BadgeSeconds = 180f,
                DifficultyPath = LevelDifficulties.Island2Path,
            };
            level.Walls.AddRange(Obstacles);
            int kanal = level.AddHorizontalDivider("Gate_1", DividerRows[0], 1, Width - 2, GateX, GateWidth);
            int pumpwerk = level.AddHorizontalDivider("Gate_2", DividerRows[1], 1, Width - 2, GateX, GateWidth);
            level.Gates[kanal].Kills = KanalKills;

            level.AddZone(new LevelZone("Zone_1", new RectInt(1, 1, Width - 2, DividerRows[0]), ZoneSpawnCells[0],
                new LevelWaves(WavesPrefix + "Zone1.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 2)),
                        new Wave(new WaveEntry(e.Walker, 5), new WaveEntry(e.Runner, 3)),
                    },
                    0.9f, 3f, AfterLastWave.RepeatLast)), kanal));
            int zone2 = level.AddZone(new LevelZone("Zone_2",
                new RectInt(1, DividerRows[0] + 1, Width - 2, ArenaBottom - DividerRows[0] - 1), ZoneSpawnCells[1],
                new LevelWaves(WavesPrefix + "Zone2.asset", (w, e) => w.Configure(
                    new[]
                    {
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 3), new WaveEntry(e.Boomer, 1)),
                        new Wave(new WaveEntry(e.Walker, 4), new WaveEntry(e.Runner, 3), new WaveEntry(e.Brute, 1)),
                    },
                    0.8f, 3f, AfterLastWave.RepeatLast)), pumpwerk));
            int arena = level.AddZone(new LevelZone(
                "Zone_Arena", new RectInt(1, ArenaBottom, Width - 2, Height - 1 - ArenaBottom), null, null, -1));

            int hold = level.AddPuzzle(LevelPuzzle.HoldZone("HoldZone_Zone_2", zone2, HoldCenter));
            level.Gates[pumpwerk].Puzzles.Add(hold);
            level.Boss = new LevelBoss(BroodMotherBuilder.DefinitionPath, BossStart, arena, pumpwerk, BroodMotherBuilder.Configure)
            {
                DefinitionType = typeof(RushOps.Bosses.BroodMotherDefinition),
                Create = BroodMotherBuilder.Create,
            };
            // Wall weapons (issue #177): the island's new weapon at least once before its boss unlocks it.
            // Zone 2 Pumpwerk before the arena, on the west pump.
            level.WallWeapons.Add(new LevelWallWeapon(new Vector2Int(5, 26), Combat.WeaponCatalogBuilder.AssaultRiflePath));
            return level;
        }
    }
}
