using System;
using System.Collections.Generic;
using RushOps.Save;

namespace RushOps.Levels
{
    /// <summary>
    /// Which islands and levels are open (F-UI-06, design question M9): the first island is open from the start; an
    /// island opens once every level of the previous island is finished; within an island a level opens once the one
    /// before it is finished. Stars do not matter. A level without an id is not built yet and never opens.
    /// </summary>
    public sealed class CampaignUnlocks
    {
        private readonly string[][] _islands;

        /// <param name="islands">Level ids per island in play order; null or empty ids are levels not built yet.</param>
        public CampaignUnlocks(IReadOnlyList<IReadOnlyList<string>> islands)
        {
            if (islands == null)
            {
                throw new ArgumentNullException(nameof(islands));
            }

            _islands = new string[islands.Count][];
            for (int i = 0; i < islands.Count; i++)
            {
                IReadOnlyList<string> levels = islands[i] ?? Array.Empty<string>();
                _islands[i] = new string[levels.Count];
                for (int j = 0; j < levels.Count; j++)
                {
                    _islands[i][j] = string.IsNullOrEmpty(levels[j]) ? null : levels[j];
                }
            }
        }

        /// <summary>Number of islands.</summary>
        public int IslandCount => _islands.Length;

        /// <summary>Number of level slots on <paramref name="island"/>, built or not.</summary>
        public int LevelCount(int island)
        {
            return IsIsland(island) ? _islands[island].Length : 0;
        }

        /// <summary>Id of a level slot, or null if it is not built yet.</summary>
        public string LevelId(int island, int level)
        {
            return IsSlot(island, level) ? _islands[island][level] : null;
        }

        /// <summary>True if <paramref name="island"/> can be entered.</summary>
        public bool IsIslandUnlocked(int island, ProgressData progress)
        {
            if (!IsIsland(island))
            {
                return false;
            }

            return island == 0 || IsIslandCompleted(island - 1, progress);
        }

        /// <summary>True if every level of <paramref name="island"/> is built and finished.</summary>
        public bool IsIslandCompleted(int island, ProgressData progress)
        {
            if (!IsIsland(island) || _islands[island].Length == 0)
            {
                return false;
            }

            foreach (string id in _islands[island])
            {
                if (id == null || !ProgressRules.IsCompleted(progress, id))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>True if the level can be played.</summary>
        public bool IsLevelUnlocked(int island, int level, ProgressData progress)
        {
            if (LevelId(island, level) == null || !IsIslandUnlocked(island, progress))
            {
                return false;
            }

            return level == 0 || ProgressRules.IsCompleted(progress, _islands[island][level - 1]);
        }

        /// <summary>Finds the island and slot of <paramref name="levelId"/>.</summary>
        public bool TryFind(string levelId, out int island, out int level)
        {
            for (island = 0; island < _islands.Length; island++)
            {
                for (level = 0; level < _islands[island].Length; level++)
                {
                    if (levelId != null && _islands[island][level] == levelId)
                    {
                        return true;
                    }
                }
            }

            island = -1;
            level = -1;
            return false;
        }

        /// <summary>
        /// The first open level that is not finished yet, i.e. where the player continues; false if all open levels
        /// are finished.
        /// </summary>
        public bool TryGetNextOpen(ProgressData progress, out int island, out int level)
        {
            for (island = 0; island < _islands.Length; island++)
            {
                for (level = 0; level < _islands[island].Length; level++)
                {
                    if (IsLevelUnlocked(island, level, progress) && !ProgressRules.IsCompleted(progress, _islands[island][level]))
                    {
                        return true;
                    }
                }
            }

            island = -1;
            level = -1;
            return false;
        }

        /// <summary>
        /// Level „Story Mode“ starts while there is no island map (issue #125): the next open level, or the first
        /// level once every open level is finished. False if the campaign has no playable level.
        /// </summary>
        public bool TryGetLevelToPlay(ProgressData progress, out int island, out int level)
        {
            if (TryGetNextOpen(progress, out island, out level))
            {
                return true;
            }

            island = 0;
            level = 0;
            return IsLevelUnlocked(0, 0, progress);
        }

        private bool IsIsland(int island)
        {
            return island >= 0 && island < _islands.Length;
        }

        private bool IsSlot(int island, int level)
        {
            return IsIsland(island) && level >= 0 && level < _islands[island].Length;
        }
    }
}
