using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Perk drops (F-PRK-01 to F-PRK-03, issue #23): global drop chance per killed enemy, the perks of the weighted
    /// pool, how long a drop stays on the floor and how it floats, glows and blinks.
    /// </summary>
    [CreateAssetMenu(fileName = "LootConfig", menuName = "RushOps/Loot Config")]
    public class LootConfig : ScriptableObject
    {
        [Header("Drops")]
        [Tooltip("Chance per killed enemy that a perk drops, 0..1.")]
        [SerializeField, Range(0f, 1f)] private float _dropChance = 0.08f;

        [Tooltip("Perks that can drop; their weights decide which one.")]
        [SerializeField] private PerkDefinition[] _perks = System.Array.Empty<PerkDefinition>();

        [Tooltip("If an enemy dies on a blocked cell, the drop moves to the nearest walkable cell within this many cells; otherwise it is lost.")]
        [SerializeField, Min(0)] private int _placementRadius = 3;

        [Header("On the floor")]
        [Tooltip("Seconds a perk stays before it disappears.")]
        [SerializeField, Min(0.1f)] private float _lifetime = 10f;

        [Tooltip("The perk blinks during these last seconds.")]
        [SerializeField, Min(0f)] private float _blinkDuration = 3f;

        [Tooltip("Seconds per on or off phase of the blinking.")]
        [SerializeField, Min(0.01f)] private float _blinkInterval = 0.15f;

        [Tooltip("How high the icon floats up and down, in units.")]
        [SerializeField, Min(0f)] private float _bobHeight = 0.12f;

        [Tooltip("Seconds per float cycle.")]
        [SerializeField, Min(0f)] private float _bobPeriod = 1.2f;

        [Tooltip("Glow alpha range and seconds per pulse.")]
        [SerializeField, Range(0f, 1f)] private float _glowMin = 0.45f;
        [SerializeField, Range(0f, 1f)] private float _glowMax = 1f;
        [SerializeField, Min(0f)] private float _glowPeriod = 0.8f;

        /// <summary>Chance per killed enemy that a perk drops, 0..1.</summary>
        public float DropChance => _dropChance;

        /// <summary>Perks of the weighted pool.</summary>
        public PerkDefinition[] Perks => _perks;

        /// <summary>Largest distance in cells a drop is moved to reach a walkable cell.</summary>
        public int PlacementRadius => _placementRadius;

        /// <summary>Seconds a perk stays on the floor.</summary>
        public float Lifetime => _lifetime;

        /// <summary>Last seconds of the lifetime in which the perk blinks.</summary>
        public float BlinkDuration => _blinkDuration;

        /// <summary>Seconds per on or off phase of the blinking.</summary>
        public float BlinkInterval => _blinkInterval;

        /// <summary>Float height in units.</summary>
        public float BobHeight => _bobHeight;

        /// <summary>Seconds per float cycle.</summary>
        public float BobPeriod => _bobPeriod;

        /// <summary>Lowest glow alpha.</summary>
        public float GlowMin => _glowMin;

        /// <summary>Highest glow alpha.</summary>
        public float GlowMax => _glowMax;

        /// <summary>Seconds per glow pulse.</summary>
        public float GlowPeriod => _glowPeriod;
    }
}
