using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// Balancing values for a weapon: fire rate, projectile flight and damage.
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

        [Tooltip("Damage per projectile hit.")]
        [SerializeField, Min(0)] private int _damage = 1;

        [Header("Aim assist (F-STE-06)")]
        [Tooltip("Correct shots towards an enemy close to the aim direction.")]
        [SerializeField] private bool _aimAssistEnabled = true;

        [Tooltip("Enemies at most this many degrees off the aim direction are aimed at.")]
        [SerializeField, Range(0f, 45f)] private float _aimAssistAngle = 10f;

        [Tooltip("Enemies at most this far from the muzzle in units are aimed at.")]
        [SerializeField, Min(0f)] private float _aimAssistRange = 8f;

        /// <summary>Shots per second while the trigger is held.</summary>
        public float FireRate => _fireRate;

        /// <summary>Projectile speed in units per second.</summary>
        public float ProjectileSpeed => _projectileSpeed;

        /// <summary>Distance in units after which a projectile disappears.</summary>
        public float MaxRange => _maxRange;

        /// <summary>Damage per projectile hit.</summary>
        public int Damage => _damage;

        /// <summary>True if shots are corrected towards enemies close to the aim direction.</summary>
        public bool AimAssistEnabled => _aimAssistEnabled;

        /// <summary>Largest angle in degrees between aim direction and an enemy the assist corrects to.</summary>
        public float AimAssistAngle => _aimAssistAngle;

        /// <summary>Largest distance in units of an enemy the assist corrects to.</summary>
        public float AimAssistRange => _aimAssistRange;
    }
}
