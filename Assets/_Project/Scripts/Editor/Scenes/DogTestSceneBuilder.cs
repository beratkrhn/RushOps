using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Test scene „Hunde-Test“ (issue #263): one open arena whose waves send packs of zombie dogs, one pack after the
    /// other, so the crouch, the lunge and the dodge can be tried without playing up to a dog round. Listed in the test
    /// menu, not in the campaign (A4).
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.DogTestSceneBuilder.Build</c>
    /// </summary>
    public static class DogTestSceneBuilder
    {
        /// <summary>Scene name and level id.</summary>
        public const string SceneName = "Dog_Test";

        private const int Width = 28;
        private const int Height = 24;

        internal static readonly LevelBlueprint Blueprint = CreateBlueprint();

        [MenuItem("RushOps/Szenen/Dog_Test erzeugen")]
        public static void Build()
        {
            LevelBlueprintBuilder.Build(Blueprint);
        }

        private static LevelBlueprint CreateBlueprint()
        {
            var level = new LevelBlueprint(SceneName, "Hunde-Test", Width, Height, new Vector2Int(Width / 2, 4))
            {
                StarScores = new Vector2Int(1000, 2000),
                Dogs = true,
            };
            // One pack at a time, again and again.
            var packs = new LevelWaves("Dog_Test_Packs.asset", (w, e) => w.Configure(
                new[] { new Wave(new WaveEntry(e.Dog, e.Dog.PackSize)) }, 0.2f, 2f, AfterLastWave.RepeatLast));
            level.AddZone(new LevelZone("Zone_Arena", new RectInt(1, 1, Width - 2, Height - 2),
                new[]
                {
                    new Vector2Int(3, Height - 4), new Vector2Int(Width - 4, Height - 4),
                    new Vector2Int(3, Height / 2), new Vector2Int(Width - 4, Height / 2),
                },
                packs, -1));
            // The packs never end, so the exit stays locked; the test is left through the pause menu.
            level.ExitCenter = new Vector2(Width / 2f, Height - 3f);
            level.ExitLockZone = 0;
            return level;
        }
    }
}
