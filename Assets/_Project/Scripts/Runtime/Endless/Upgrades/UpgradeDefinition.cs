using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// One run upgrade of the endless mode (F-END-12, decision N19): what it changes, by how much per level and up to
    /// which level. All numbers are start values.
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade", menuName = "RushOps/Endless Upgrade")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Tooltip("Stable id, e.g. \"bounce\"; never change it once used.")]
        [SerializeField] private string _id = "";

        [Tooltip("What the upgrade changes.")]
        [SerializeField] private UpgradeKind _kind;

        [Tooltip("Name on the card (German).")]
        [SerializeField] private string _displayName = "";

        [Tooltip("Short description on the card (German).")]
        [SerializeField, TextArea] private string _description = "";

        [Tooltip("Icon on the card.")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Effect per level; its unit depends on the kind (see UpgradeKind).")]
        [SerializeField] private float _valuePerLevel = 1f;

        [Tooltip("Highest level; the upgrade is not offered any more once reached.")]
        [SerializeField, Min(1)] private int _maxLevel = 1;

        /// <summary>Stable id.</summary>
        public string Id => _id;

        /// <summary>What the upgrade changes.</summary>
        public UpgradeKind Kind => _kind;

        /// <summary>Name on the card (German).</summary>
        public string DisplayName => _displayName;

        /// <summary>Short description on the card (German).</summary>
        public string Description => _description;

        /// <summary>Icon on the card; may be null.</summary>
        public Sprite Icon => _icon;

        /// <summary>Effect per level.</summary>
        public float ValuePerLevel => _valuePerLevel;

        /// <summary>Highest level, at least 1.</summary>
        public int MaxLevel => Mathf.Max(1, _maxLevel);

        /// <summary>Sets all values, for editor scripts and tests.</summary>
        public void Configure(string id, UpgradeKind kind, string displayName, string description, float valuePerLevel,
            int maxLevel)
        {
            _id = id ?? "";
            _kind = kind;
            _displayName = displayName ?? "";
            _description = description ?? "";
            _valuePerLevel = valuePerLevel;
            _maxLevel = Mathf.Max(1, maxLevel);
        }

        /// <summary>Sets the icon, for editor scripts.</summary>
        public void SetIcon(Sprite icon)
        {
            _icon = icon;
        }
    }
}
