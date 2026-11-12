using System.Collections.Generic;

namespace RushOps.Save
{
    /// <summary>
    /// „Kampagne zurücksetzen“ (issue #328, F-UI-09, F-DAT-07): clears only the campaign, i.e. the level records with
    /// stars, best scores and badges (islands and levels unlock from them), the shown puzzle hints and the intro, which
    /// plays again. Gold, skins, weapons, statistics, endless and daily bests and the settings stay.
    /// </summary>
    public static class CampaignReset
    {
        /// <summary>Resets the campaign in <paramref name="data"/>; the caller saves.</summary>
        public static void Apply(SaveData data)
        {
            if (data == null)
            {
                return;
            }

            data.Progress ??= new ProgressData();
            data.Progress.Levels ??= new List<LevelRecord>();
            data.Progress.Levels.Clear();
            data.Hints ??= new HintsData();
            data.Hints.Seen ??= new List<string>();
            data.Hints.Seen.Clear();
            data.Story ??= new StoryData();
            data.Story.IntroSeen = false;
        }
    }
}
