using UnityEditor;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Test scenes for the island tilesets (issues #105, #106): the layout of <c>Arena_Zonen</c> in an island's look,
    /// listed in the test menu like a level.
    /// </summary>
    public static class IslandTestScenesBuilder
    {
        private const string StadtScenePath = "Assets/_Project/Scenes/Arena_Stadt.unity";

        [MenuItem("RushOps/Szenen/Arena_Stadt erzeugen")]
        public static void BuildStadt()
        {
            ArenaZonenSceneBuilder.BuildWithTheme(StadtScenePath, "Stadt-Test", IslandThemes.StadtPath);
        }
    }
}
