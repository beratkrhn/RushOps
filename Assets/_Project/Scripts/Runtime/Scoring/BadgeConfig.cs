using UnityEngine;

namespace RushOps.Scoring
{
    /// <summary>
    /// Values of the level badges (issue #287, F-SCO-04, N23). The target time of „Blitz“ is per level, in its
    /// <see cref="Levels.LevelDefinition"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "BadgeConfig", menuName = "RushOps/Badge Config")]
    public class BadgeConfig : ScriptableObject
    {
        [Tooltip("Multiplier to reach once in a level for the badge „Kombo“.")]
        [SerializeField, Min(1)] private int _comboTarget = 5;

        /// <summary>Multiplier to reach once in a level for the badge „Kombo“.</summary>
        public int ComboTarget => _comboTarget;
    }
}
