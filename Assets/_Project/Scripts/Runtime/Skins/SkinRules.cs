using System;
using System.Collections.Generic;
using RushOps.Save;

namespace RushOps.Skins
{
    /// <summary>
    /// Unlock and selection rules for skins (issue #128, F-SKN-02, F-SKN-03): default skins are always available,
    /// level skins once their level was finished, milestone skins once a statistic reaches its threshold. Unlocked
    /// skins stay unlocked. Skins are cosmetic only (F-SKN-04); nothing here touches game values.
    /// </summary>
    public static class SkinRules
    {
        /// <summary>Current value of <paramref name="milestone"/> in the save game.</summary>
        public static int MilestoneValue(SkinMilestone milestone, ProgressData progress, StatsData stats)
        {
            return milestone switch
            {
                SkinMilestone.TotalKills => stats?.TotalKills ?? 0,
                SkinMilestone.TotalStars => ProgressRules.TotalStars(progress),
                SkinMilestone.LevelsCompleted => stats?.LevelsCompleted ?? 0,
                SkinMilestone.BestCombo => stats?.BestCombo ?? 0,
                _ => 0,
            };
        }

        /// <summary>True if the condition of <paramref name="unlock"/> is met by the progress and statistics.</summary>
        public static bool IsMet(SkinUnlock unlock, ProgressData progress, StatsData stats)
        {
            return unlock.Kind switch
            {
                SkinUnlockKind.Default => true,
                SkinUnlockKind.LevelCompleted => ProgressRules.IsCompleted(progress, unlock.LevelId),
                SkinUnlockKind.Milestone => MilestoneValue(unlock.Milestone, progress, stats) >= unlock.Threshold,
                _ => false,
            };
        }

        /// <summary>
        /// How much of a milestone is still missing, e.g. 340 kills; 0 if reached or not a milestone skin.
        /// </summary>
        public static int Remaining(SkinUnlock unlock, ProgressData progress, StatsData stats)
        {
            if (unlock.Kind != SkinUnlockKind.Milestone)
            {
                return 0;
            }

            return Math.Max(0, unlock.Threshold - MilestoneValue(unlock.Milestone, progress, stats));
        }

        /// <summary>True if the player may use <paramref name="unlock"/>'s skin.</summary>
        public static bool IsUnlocked(SkinUnlock unlock, SkinsData skins)
        {
            if (unlock.Kind == SkinUnlockKind.Default)
            {
                return true;
            }

            return skins?.Unlocked != null && !string.IsNullOrEmpty(unlock.SkinId) && skins.Unlocked.Contains(unlock.SkinId);
        }

        /// <summary>
        /// Unlocks every skin of <paramref name="skins"/> whose condition is now met and adds it to
        /// <paramref name="data"/>; checked after each level and on death.
        /// </summary>
        /// <param name="newlyUnlocked">Receives the indices of the skins unlocked by this call, in list order.</param>
        /// <returns>The number of skins unlocked by this call.</returns>
        public static int UnlockNew(IReadOnlyList<SkinUnlock> skins, SaveData data, List<int> newlyUnlocked = null)
        {
            if (skins == null || data == null)
            {
                return 0;
            }

            data.Skins ??= new SkinsData();
            data.Skins.Unlocked ??= new List<string>();
            int count = 0;
            for (int i = 0; i < skins.Count; i++)
            {
                SkinUnlock skin = skins[i];
                if (skin.Kind == SkinUnlockKind.Default || string.IsNullOrEmpty(skin.SkinId) || IsUnlocked(skin, data.Skins))
                {
                    continue;
                }

                if (IsMet(skin, data.Progress, data.Stats))
                {
                    data.Skins.Unlocked.Add(skin.SkinId);
                    newlyUnlocked?.Add(i);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Index of the skin the player plays with: the saved choice if it still exists and is unlocked, otherwise the
        /// first default skin; -1 if the list has none.
        /// </summary>
        public static int CurrentIndex(IReadOnlyList<SkinUnlock> skins, SkinsData data)
        {
            if (skins == null)
            {
                return -1;
            }

            string current = data?.Current;
            int fallback = -1;
            for (int i = 0; i < skins.Count; i++)
            {
                if (!string.IsNullOrEmpty(current) && skins[i].SkinId == current && IsUnlocked(skins[i], data))
                {
                    return i;
                }

                if (fallback < 0 && skins[i].Kind == SkinUnlockKind.Default)
                {
                    fallback = i;
                }
            }

            return fallback;
        }

        /// <summary>Makes skin <paramref name="index"/> the current one if it is unlocked.</summary>
        /// <returns>False if the index is out of range or the skin is locked; nothing changes then.</returns>
        public static bool Select(IReadOnlyList<SkinUnlock> skins, SaveData data, int index)
        {
            if (skins == null || data == null || index < 0 || index >= skins.Count)
            {
                return false;
            }

            data.Skins ??= new SkinsData();
            if (!IsUnlocked(skins[index], data.Skins))
            {
                return false;
            }

            data.Skins.Current = skins[index].SkinId;
            return true;
        }
    }
}
