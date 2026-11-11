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

        [Tooltip("Icons of earned badges: Unversehrt, Blitz, Kombo (issue #288).")]
        [SerializeField] private Sprite[] _earnedIcons = new Sprite[BadgeRules.PerLevel];

        [Tooltip("Icons of missing badges: Unversehrt, Blitz, Kombo (issue #288).")]
        [SerializeField] private Sprite[] _missingIcons = new Sprite[BadgeRules.PerLevel];

        [Tooltip("Seconds of one pulse of a newly earned badge on „Level geschafft!“.")]
        [SerializeField, Min(0.05f)] private float _newPulseSeconds = 0.6f;

        [Tooltip("Largest scale of a newly earned badge while it pulses.")]
        [SerializeField, Min(1f)] private float _newPulseScale = 1.25f;

        /// <summary>Multiplier to reach once in a level for the badge „Kombo“.</summary>
        public int ComboTarget => _comboTarget;

        /// <summary>Seconds of one pulse of a newly earned badge.</summary>
        public float NewPulseSeconds => _newPulseSeconds;

        /// <summary>Largest scale of a newly earned badge while it pulses.</summary>
        public float NewPulseScale => _newPulseScale;

        /// <summary>Icon of <paramref name="badge"/>, bright if <paramref name="earned"/>, dark if not; null if missing.</summary>
        public Sprite Icon(BadgeKind badge, bool earned)
        {
            Sprite[] icons = earned ? _earnedIcons : _missingIcons;
            int index = BadgeRules.IndexOf(badge);
            return icons != null && index >= 0 && index < icons.Length ? icons[index] : null;
        }
    }
}
