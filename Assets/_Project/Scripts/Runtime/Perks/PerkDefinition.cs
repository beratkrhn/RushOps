using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// One perk type (F-PRK-02, issue #23): id, display name, icon, whether it acts at once or for a while, its
    /// duration and its weight in the drop pool. The effects themselves follow with issue #24.
    /// </summary>
    [CreateAssetMenu(fileName = "PerkDefinition", menuName = "RushOps/Perk Definition")]
    public class PerkDefinition : ScriptableObject
    {
        [Tooltip("Stable id for code and save games, e.g. speed_boost.")]
        [SerializeField] private string _id = "";

        [Tooltip("Name shown to the player (German).")]
        [SerializeField] private string _displayName = "";

        [Tooltip("Icon of the pickup and the HUD.")]
        [SerializeField] private Sprite _icon;

        [SerializeField] private PerkKind _kind = PerkKind.Timed;

        [Tooltip("Seconds a timed perk acts; ignored for instant perks.")]
        [SerializeField, Min(0f)] private float _duration = 10f;

        [Tooltip("Relative chance in the drop pool; 0 never drops.")]
        [SerializeField, Min(0f)] private float _weight = 10f;

        /// <summary>Stable id, e.g. <c>speed_boost</c>.</summary>
        public string Id => _id;

        /// <summary>Name shown to the player.</summary>
        public string DisplayName => _displayName;

        /// <summary>Icon of the pickup and the HUD; may be null.</summary>
        public Sprite Icon => _icon;

        /// <summary>Instant or timed.</summary>
        public PerkKind Kind => _kind;

        /// <summary>Seconds a timed perk acts; 0 for instant perks.</summary>
        public float Duration => _kind == PerkKind.Timed ? _duration : 0f;

        /// <summary>Relative chance in the drop pool.</summary>
        public float Weight => _weight;
    }
}
