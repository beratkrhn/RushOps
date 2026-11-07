using UnityEditor;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Rebuilds every generated scene, and with them the prefabs and animation assets they use, in one go.
    /// Batch mode: <c>-executeMethod RushOps.Editor.Scenes.AllScenesBuilder.BuildAll</c>.
    /// </summary>
    public static class AllScenesBuilder
    {
        [MenuItem("RushOps/Szenen/Alle erzeugen")]
        public static void BuildAll()
        {
            ArenaTestSceneBuilder.Build();
            ArenaZonenSceneBuilder.Build();
            Level1_1SceneBuilder.Build();
            Level1_2SceneBuilder.Build();
            Level1_3SceneBuilder.Build();
            Level2_1SceneBuilder.Build();
            Level2_2SceneBuilder.Build();
            Level2_3SceneBuilder.Build();
            Level3_1SceneBuilder.Build();
            Level3_2SceneBuilder.Build();
            Level3_3SceneBuilder.Build();
            BossTestSceneBuilder.Build();
            StressTestSceneBuilder.Build();
            IslandTestScenesBuilder.BuildStadt();
            IslandTestScenesBuilder.BuildVulkan();
            EndlessArenaSceneBuilder.Build();
            TestMenuSceneBuilder.Build();
            MainMenuSceneBuilder.Build();
            IslandMapSceneBuilder.Build();
            BootSceneBuilder.Build();
        }
    }
}
