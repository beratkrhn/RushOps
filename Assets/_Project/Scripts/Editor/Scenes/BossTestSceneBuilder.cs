using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Test scene „Boss-Test“ (issue #150) with the placeholder boss: a small zone with a kill gate of three walkers,
    /// then the open arena; the door closes behind the player and the fight starts. Listed in the test menu, not in the
    /// campaign.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.BossTestSceneBuilder.Build</c>
    /// </summary>
    public static class BossTestSceneBuilder
    {
        /// <summary>Scene name and level id.</summary>
        public const string SceneName = "Boss_Test";

        /// <summary>Definition of the placeholder boss.</summary>
        public const string DefinitionPath = "Assets/_Project/Data/Bosses/Boss_Test.asset";

        private const int Width = 24;
        private const int Height = 48;
        private const int DividerRow = 12;
        private const int GateX = 10;
        private const int GateWidth = 4;
        // Two rows behind the gate still belong to the first zone, so the door closes behind the player.
        private const int ArenaBottom = DividerRow + 3;

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Boss_Test erzeugen")]
        public static void Build()
        {
            LevelBlueprintBuilder.Build(Blueprint);
        }

        private static LevelBlueprint CreateBlueprint()
        {
            var level = new LevelBlueprint(SceneName, "Boss-Test", Width, Height, new Vector2Int(12, 3))
            {
                StarScores = new Vector2Int(1100, 1300),
            };
            int gate = level.AddHorizontalDivider("Gate_1", DividerRow, 1, Width - 2, GateX, GateWidth);
            level.Gates[gate].Kills = 3;
            var waves = new LevelWaves("Boss_Test_Zone1.asset", (w, e) => w.Configure(
                new[] { new Wave(new WaveEntry(e.Walker, 3)) }, 1f, 3f, AfterLastWave.RepeatLast));
            level.AddZone(new LevelZone("Zone_1", new RectInt(1, 1, Width - 2, ArenaBottom - 1),
                new[] { new Vector2Int(3, 9), new Vector2Int(20, 9) }, waves, gate));
            int arena = level.AddZone(new LevelZone(
                "Zone_Arena", new RectInt(1, ArenaBottom, Width - 2, Height - 1 - ArenaBottom), null, null, -1));
            level.Boss = new LevelBoss(DefinitionPath, new Vector2(12f, 36f), arena, gate,
                d => d.Configure("Testboss", 600, 2f, 1, 1000));
            return level;
        }
    }
}
