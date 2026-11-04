using RushOps.Characters;
using UnityEngine;

namespace RushOps.Skins
{
    /// <summary>
    /// One skin of the player (issue #128, F-SKN-02 to F-SKN-04): its name, its animations and how it is unlocked.
    /// Skins are recolours with the same animations and never change game values.
    /// </summary>
    [CreateAssetMenu(fileName = "Skin", menuName = "RushOps/Skin Definition")]
    public class SkinDefinition : ScriptableObject
    {
        [Tooltip("Stable id in the save game; never change it once released.")]
        [SerializeField] private string _id = "";

        [Tooltip("Name shown to the player (German).")]
        [SerializeField] private string _displayName = "";

        [Header("Animationen")]
        [SerializeField] private DirectionalAnimation _idle;
        [SerializeField] private DirectionalAnimation _walk;
        [SerializeField] private DirectionalAnimation _hit;
        [SerializeField] private DirectionalAnimation _death;

        [Header("Freischaltung")]
        [SerializeField] private SkinUnlockKind _unlockKind = SkinUnlockKind.Default;

        [Tooltip("Level to finish (its scene name), for LevelCompleted.")]
        [SerializeField] private string _levelId = "";

        [Tooltip("Level name in the unlock hint, e.g. „Level 1-2“.")]
        [SerializeField] private string _levelName = "";

        [Tooltip("Statistic counted, for Milestone.")]
        [SerializeField] private SkinMilestone _milestone = SkinMilestone.TotalKills;

        [Tooltip("Value the statistic has to reach, for Milestone.")]
        [SerializeField, Min(0)] private int _threshold = 100;

        /// <summary>Stable id in the save game.</summary>
        public string Id => _id;

        /// <summary>Name shown to the player.</summary>
        public string DisplayName => _displayName;

        /// <summary>Standing pose.</summary>
        public DirectionalAnimation Idle => _idle;

        /// <summary>Walk cycle.</summary>
        public DirectionalAnimation Walk => _walk;

        /// <summary>Hit reaction.</summary>
        public DirectionalAnimation Hit => _hit;

        /// <summary>Death.</summary>
        public DirectionalAnimation Death => _death;

        /// <summary>How the skin is unlocked.</summary>
        public SkinUnlockKind UnlockKind => _unlockKind;

        /// <summary>Level name in the unlock hint.</summary>
        public string LevelName => _levelName;

        /// <summary>The unlock condition as plain data.</summary>
        public SkinUnlock Unlock => new SkinUnlock(_id, _unlockKind, _levelId, _milestone, _threshold);
    }
}
