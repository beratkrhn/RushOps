using System;
using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// Order of the islands and their levels (F-UI-05, F-UI-06, issue #123). A level's id in the save game is its
    /// <see cref="LevelDefinition.SceneName"/>. The unlock rules live in <see cref="CampaignUnlocks"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Campaign", menuName = "RushOps/Campaign")]
    public class Campaign : ScriptableObject
    {
        [Tooltip("Islands from bottom to top of the map.")]
        [SerializeField] private CampaignIsland[] _islands = Array.Empty<CampaignIsland>();

        [NonSerialized] private CampaignUnlocks _unlocks;

        /// <summary>Islands from bottom to top of the map.</summary>
        public CampaignIsland[] Islands => _islands;

        /// <summary>Unlock rules for these islands.</summary>
        public CampaignUnlocks Unlocks => _unlocks ??= CreateUnlocks();

        /// <summary>The level in a slot, or null if it is not built yet.</summary>
        public LevelDefinition LevelAt(int island, int level)
        {
            if (island < 0 || island >= _islands.Length || _islands[island] == null)
            {
                return null;
            }

            LevelDefinition[] levels = _islands[island].Levels;
            return level >= 0 && level < levels.Length ? levels[level] : null;
        }

        private CampaignUnlocks CreateUnlocks()
        {
            var islands = new List<IReadOnlyList<string>>(_islands.Length);
            foreach (CampaignIsland island in _islands)
            {
                LevelDefinition[] levels = island != null ? island.Levels : Array.Empty<LevelDefinition>();
                var ids = new string[levels.Length];
                for (int i = 0; i < levels.Length; i++)
                {
                    ids[i] = levels[i] != null ? levels[i].SceneName : null;
                }

                islands.Add(ids);
            }

            return new CampaignUnlocks(islands);
        }

        private void OnValidate()
        {
            _unlocks = null;
        }
    }
}
