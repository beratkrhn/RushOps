using System;
using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>One island of the <see cref="Campaign"/> with its levels in play order.</summary>
    [Serializable]
    public class CampaignIsland
    {
        [Tooltip("Name shown to the player (German).")]
        [SerializeField] private string _displayName = "";

        [Tooltip("Levels in play order; empty slots are levels not built yet.")]
        [SerializeField] private LevelDefinition[] _levels = Array.Empty<LevelDefinition>();

        /// <summary>Name shown to the player.</summary>
        public string DisplayName => _displayName;

        /// <summary>Levels in play order; null entries are levels not built yet.</summary>
        public LevelDefinition[] Levels => _levels;
    }
}
