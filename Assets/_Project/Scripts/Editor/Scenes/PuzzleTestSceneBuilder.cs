using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Test scene „Zombie-Rätsel“ (issues #289, #290): the zombie puzzles without playing up to their zones. Zone 1 has
    /// a pressure plate that opens the gate north; the zone keeps sending brutes until it locks. Listed in the test menu,
    /// not in the campaign.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.PuzzleTestSceneBuilder.Build</c>
    /// </summary>
    public static class PuzzleTestSceneBuilder
    {
        /// <summary>Scene name and level id.</summary>
        public const string SceneName = "Puzzle_Test";

        private const int Width = 24;
        private const int Height = 38;
        private const int DividerRow = 20;
        private const int GateX = 10;
        private const int GateWidth = 4;

        /// <summary>Center of the pressure plate.</summary>
        internal static readonly Vector2 PlateCenter = new Vector2(12f, 12f);

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Puzzle_Test erzeugen")]
        public static void Build()
        {
            LevelBlueprintBuilder.Build(Blueprint);
        }

        private static LevelBlueprint CreateBlueprint()
        {
            var level = new LevelBlueprint(SceneName, "Zombie-Rätsel", Width, Height, new Vector2Int(Width / 2, 3))
            {
                StarScores = new Vector2Int(1000, 2000),
                ExitCenter = new Vector2(Width / 2f, Height - 2.25f),
            };
            int gate = level.AddHorizontalDivider("Gate_Plate", DividerRow, 1, Width - 2, GateX, GateWidth);
            // A few walkers keep the zone busy; the brutes come from the plate itself.
            var walkers = new LevelWaves("Puzzle_Test_Walkers.asset", (w, e) => w.Configure(
                new[] { new Wave(new WaveEntry(e.Walker, 2)) }, 1f, 6f, AfterLastWave.RepeatLast));
            level.AddZone(new LevelZone("Zone_Plate", new RectInt(1, 1, Width - 2, DividerRow - 1),
                new[] { new Vector2Int(3, 17), new Vector2Int(20, 17), new Vector2Int(3, 4), new Vector2Int(20, 4) },
                walkers, gate));
            level.AddZone(new LevelZone("Zone_Wall", new RectInt(1, DividerRow + 1, Width - 2, Height - DividerRow - 2),
                new[] { new Vector2Int(3, Height - 4), new Vector2Int(20, Height - 4) }, null, -1));
            int plate = level.AddPuzzle(LevelPuzzle.PressurePlate("PressurePlate", 0, PlateCenter));
            level.Gates[gate].Puzzles.Add(plate);
            return level;
        }
    }
}
