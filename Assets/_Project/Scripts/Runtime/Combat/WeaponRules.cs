using System.Collections.Generic;
using RushOps.Save;

namespace RushOps.Combat
{
    /// <summary>
    /// Unlocking and choosing weapons as pure rules (issue #176, decisions W4, W5). A weapon is given by its id and the
    /// level whose victory unlocks it; an empty level means it is available from the start. The first weapon of the
    /// list is the start weapon and the choice when nothing (or nothing unlocked) is chosen.
    /// </summary>
    public static class WeaponRules
    {
        /// <summary>True if the weapon with <paramref name="id"/> and <paramref name="unlockLevel"/> may be chosen.</summary>
        public static bool IsUnlocked(string id, string unlockLevel, WeaponsData weapons)
        {
            if (string.IsNullOrEmpty(unlockLevel))
            {
                return true;
            }

            return weapons?.Unlocked != null && !string.IsNullOrEmpty(id) && weapons.Unlocked.Contains(id);
        }

        /// <summary>
        /// Unlocks every weapon whose level is completed in <paramref name="data"/> and not unlocked yet; adds their
        /// indices to <paramref name="newlyUnlocked"/>. Returns how many were unlocked now.
        /// </summary>
        public static int UnlockNew(IReadOnlyList<string> ids, IReadOnlyList<string> unlockLevels, SaveData data,
            List<int> newlyUnlocked = null)
        {
            if (ids == null || unlockLevels == null || data == null)
            {
                return 0;
            }

            data.Weapons ??= new WeaponsData();
            data.Weapons.Unlocked ??= new List<string>();
            int count = 0;
            for (int i = 0; i < ids.Count && i < unlockLevels.Count; i++)
            {
                string level = unlockLevels[i];
                if (string.IsNullOrEmpty(level) || string.IsNullOrEmpty(ids[i]) || data.Weapons.Unlocked.Contains(ids[i])
                    || !ProgressRules.IsCompleted(data.Progress, level))
                {
                    continue;
                }

                data.Weapons.Unlocked.Add(ids[i]);
                newlyUnlocked?.Add(i);
                count++;
            }

            return count;
        }

        /// <summary>Index of the chosen weapon; 0 if none is chosen or the chosen one is not unlocked; -1 for an empty list.</summary>
        public static int CurrentIndex(IReadOnlyList<string> ids, IReadOnlyList<string> unlockLevels, WeaponsData weapons)
        {
            if (ids == null || ids.Count == 0)
            {
                return -1;
            }

            string current = weapons?.Current;
            for (int i = 0; i < ids.Count; i++)
            {
                if (!string.IsNullOrEmpty(current) && ids[i] == current
                    && IsUnlocked(ids[i], i < unlockLevels.Count ? unlockLevels[i] : null, weapons))
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>Chooses weapon <paramref name="index"/>; returns false if it does not exist or is locked.</summary>
        public static bool Select(IReadOnlyList<string> ids, IReadOnlyList<string> unlockLevels, SaveData data, int index)
        {
            if (ids == null || data == null || index < 0 || index >= ids.Count
                || !IsUnlocked(ids[index], index < unlockLevels.Count ? unlockLevels[index] : null, data.Weapons))
            {
                return false;
            }

            data.Weapons ??= new WeaponsData();
            data.Weapons.Current = index == 0 ? string.Empty : ids[index];
            return true;
        }
    }
}
