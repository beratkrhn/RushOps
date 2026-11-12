using RushOps.Save;

namespace RushOps.Story
{
    /// <summary>Rules of the story start (issue #333, F-STO-02): the intro plays once, on the first story start.</summary>
    public static class StoryRules
    {
        /// <summary>Scene that „Story Mode“ opens: the intro until it was seen, then the island map.</summary>
        public static string SceneOnStoryStart(StoryData story, string introScene, string mapScene)
        {
            bool seen = story != null && story.IntroSeen;
            return seen || string.IsNullOrEmpty(introScene) ? mapScene : introScene;
        }

        /// <summary>Marks the intro as seen; returns true when that changed something, so the caller saves.</summary>
        public static bool MarkIntroSeen(StoryData story)
        {
            if (story == null || story.IntroSeen)
            {
                return false;
            }

            story.IntroSeen = true;
            return true;
        }
    }
}
