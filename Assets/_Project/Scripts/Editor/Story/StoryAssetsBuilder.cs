using RushOps.Editor.Scenes;
using RushOps.Story;
using UnityEditor;

namespace RushOps.Editor.Story
{
    /// <summary>
    /// Assets of the story (issue #331): the cutscene config under <c>Data/Story/</c>. Existing assets are kept, so tuned
    /// values survive. Batchmode: <c>-executeMethod RushOps.Editor.Story.StoryAssetsBuilder.EnsureAssets</c>
    /// </summary>
    public static class StoryAssetsBuilder
    {
        /// <summary>Path of the cutscene config.</summary>
        public const string CutsceneConfigPath = "Assets/_Project/Data/Story/CutsceneConfig.asset";

        /// <summary>Creates the story assets that are missing.</summary>
        [MenuItem("RushOps/Story/Story-Assets anlegen")]
        public static void EnsureAssets()
        {
            LoadOrCreateCutsceneConfig();
            AssetDatabase.SaveAssets();
        }

        /// <summary>The cutscene config, created with start values when missing.</summary>
        public static CutsceneConfig LoadOrCreateCutsceneConfig()
        {
            return ArenaTestSceneBuilder.LoadOrCreateConfig<CutsceneConfig>(CutsceneConfigPath);
        }
    }
}
