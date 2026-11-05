using RushOps.Bosses;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Test scene „Stress-Test“ (issue #189): the heaviest case the campaign can show, for measuring NF-01 in the
    /// editor. A short zone, then a volcano arena with the Mega-Boomer and its puddles, while the arena's own waves
    /// keep the level at the cap of 10 enemies with runners, boomers (explosions) and a brute. Listed in the test menu,
    /// not in the campaign.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.StressTestSceneBuilder.Build</c>
    /// </summary>
    public static class StressTestSceneBuilder
    {
        /// <summary>Scene name and level id.</summary>
        public const string SceneName = "Stress_Test";

        private const int Width = 28;
        private const int Height = 52;
        private const int DividerRow = 12;
        private const int GateX = 12;
        private const int GateWidth = 4;
        // Two rows behind the gate still belong to the first zone, so the door closes behind the player.
        private const int ArenaBottom = DividerRow + 3;

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Stress_Test erzeugen")]
        public static void Build()
        {
            LevelBlueprintBuilder.Build(Blueprint);
        }

        private static LevelBlueprint CreateBlueprint()
        {
            var level = new LevelBlueprint(SceneName, "Stress-Test", Width, Height, new Vector2Int(14, 3))
            {
                StarScores = new Vector2Int(5000, 10000),
                ThemePath = IslandThemes.VulkanPath,
            };
            int gate = level.AddHorizontalDivider("Gate_1", DividerRow, 1, Width - 2, GateX, GateWidth);
            level.Gates[gate].Kills = 3;
            var entry = new LevelWaves("Stress_Test_Zone1.asset", (w, e) => w.Configure(
                new[] { new Wave(new WaveEntry(e.Walker, 3)) }, 1f, 3f, AfterLastWave.RepeatLast));
            level.AddZone(new LevelZone("Zone_1", new RectInt(1, 1, Width - 2, ArenaBottom - 1),
                new[] { new Vector2Int(3, 9), new Vector2Int(24, 9) }, entry, gate));

            // Always more enemies queued than the cap allows, so the arena stays full.
            var crowd = new LevelWaves("Stress_Test_Arena.asset", (w, e) => w.Configure(
                new[]
                {
                    new Wave(new WaveEntry(e.Runner, 5), new WaveEntry(e.Boomer, 4), new WaveEntry(e.Walker, 3),
                        new WaveEntry(e.Brute, 1)),
                },
                0.3f, 0.5f, AfterLastWave.RepeatLast));
            int arena = level.AddZone(new LevelZone(
                "Zone_Arena", new RectInt(1, ArenaBottom, Width - 2, Height - 1 - ArenaBottom),
                new[]
                {
                    new Vector2Int(3, Height - 4), new Vector2Int(Width - 4, Height - 4),
                    new Vector2Int(3, ArenaBottom + 3), new Vector2Int(Width - 4, ArenaBottom + 3),
                },
                crowd, -1));
            level.Boss = new LevelBoss(MegaBoomerBuilder.DefinitionPath, new Vector2(14f, 40f), arena, gate,
                MegaBoomerBuilder.Configure)
            {
                DefinitionType = typeof(MegaBoomerDefinition),
                Create = MegaBoomerBuilder.Create,
                Connect = MegaBoomerBuilder.Connect,
            };
            return level;
        }
    }
}
