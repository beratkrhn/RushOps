using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// Balancing values for a weapon: fire rate and projectile flight.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "RushOps/Weapon Config")]
    public class WeaponConfig : ScriptableObject
    {
        [Tooltip("Shots per second while the trigger is held.")]
        [SerializeField, Min(0f)] private float _fireRate = 6f;

        [Tooltip("Projectile speed in units (tiles) per second.")]
        [SerializeField, Min(0f)] private float _projectileSpeed = 14f;

        [Tooltip("Distance in units after which a projectile disappears.")]
        [SerializeField, Min(0f)] private float _maxRange = 10f;

        /// <summary>Shots per second while the trigger is held.</summary>
        public float FireRate => _fireRate;

        /// <summary>Projectile speed in units per second.</summary>
        public float ProjectileSpeed => _projectileSpeed;

        /// <summary>Distance in units after which a projectile disappears.</summary>
        public float MaxRange => _maxRange;
    }
}
