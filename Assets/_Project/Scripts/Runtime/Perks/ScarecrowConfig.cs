using RushOps.Combat;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>Start values of the scarecrow perk (F-PRK-07, F-PRK-08, issue #244, K5 to K7).</summary>
    [CreateAssetMenu(fileName = "ScarecrowConfig", menuName = "RushOps/Scarecrow Config")]
    public class ScarecrowConfig : ScriptableObject
    {
        [Tooltip("Seconds until the scarecrow explodes; collecting the perk again restarts it.")]
        [SerializeField, Min(0f)] private float _duration = 8f;

        [Tooltip("Radius in tiles of the explosion.")]
        [SerializeField, Min(0f)] private float _explosionRadius = 3f;

        [Tooltip("Damage the explosion deals to every normal enemy in the radius.")]
        [SerializeField, Min(0)] private int _explosionDamage = 100;

        [Tooltip("Radius in tiles around the pickup spot in which a free cell is searched if the spot is blocked (K5).")]
        [SerializeField, Min(0f)] private float _placementSearchRadius = 3f;

        [Tooltip("Enemies closer than this many tiles to the scarecrow stop and stand around it.")]
        [SerializeField, Min(0f)] private float _gatherRadius = 1.2f;

        [Tooltip("Seconds before the explosion in which the scarecrow blinks as a warning.")]
        [SerializeField, Min(0f)] private float _warningTime = 1f;

        [Tooltip("Seconds the lit warning look stays on or off while it blinks.")]
        [SerializeField, Min(0f)] private float _blinkInterval = 0.1f;

        [Tooltip("Frames per second of the sway animation.")]
        [SerializeField, Min(0.1f)] private float _frameRate = 3f;

        [Tooltip("Shake, sound and effect frames of the blast; its damage values are not used, the ones above count.")]
        [SerializeField] private ExplosionDefinition _explosionFeedback;

        /// <summary>Seconds until the explosion.</summary>
        public float Duration => _duration;

        /// <summary>Explosion radius in tiles.</summary>
        public float ExplosionRadius => _explosionRadius;

        /// <summary>Explosion damage against normal enemies.</summary>
        public int ExplosionDamage => _explosionDamage;

        /// <summary>Search radius in tiles for a free cell.</summary>
        public float PlacementSearchRadius => _placementSearchRadius;

        /// <summary>Distance in tiles within which lured enemies stop around the scarecrow.</summary>
        public float GatherRadius => _gatherRadius;

        /// <summary>Seconds of blinking before the explosion.</summary>
        public float WarningTime => _warningTime;

        /// <summary>Seconds the warning look stays on or off while it blinks.</summary>
        public float BlinkInterval => _blinkInterval;

        /// <summary>Frames per second of the sway animation.</summary>
        public float FrameRate => _frameRate;

        /// <summary>Shake, sound and effect of the blast; may be null.</summary>
        public ExplosionDefinition ExplosionFeedback => _explosionFeedback;
    }
}
