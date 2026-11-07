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

        /// <summary>Seconds until the explosion.</summary>
        public float Duration => _duration;

        /// <summary>Explosion radius in tiles.</summary>
        public float ExplosionRadius => _explosionRadius;

        /// <summary>Explosion damage against normal enemies.</summary>
        public int ExplosionDamage => _explosionDamage;

        /// <summary>Search radius in tiles for a free cell.</summary>
        public float PlacementSearchRadius => _placementSearchRadius;
    }
}
