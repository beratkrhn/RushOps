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
            IslandTestScenesBuilder.BuildStadt();
            IslandTestScenesBuilder.BuildVulkan();
            TestMenuSceneBuilder.Build();
            MainMenuSceneBuilder.Build();
            IslandMapSceneBuilder.Build();
            BootSceneBuilder.Build();
        }
    }
}
