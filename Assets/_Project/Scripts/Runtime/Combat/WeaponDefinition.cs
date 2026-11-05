using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// One weapon (issue #173, Lastenheft section 8): weapons differ only in fire rate, damage, spread, projectiles
    /// per shot, range and sound (W3). No reloading, no ammo (F-STE-02). All weapons should deal about the same
    /// damage per second as the start weapon (W7, see <see cref="WeaponMath.DamagePerSecond"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "Weapon", menuName = "RushOps/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [Tooltip("Name shown in menus and on the HUD.")]
        [SerializeField] private string _displayName = "Pistole";

        [Tooltip("Shots per second while the trigger is held.")]
        [SerializeField, Min(0f)] private float _fireRate = 6f;

        [Tooltip("Damage per projectile hit.")]
        [SerializeField, Min(0)] private int _damage = 10;

        [Tooltip("Projectiles fired with every shot, e.g. 5 for a shotgun.")]
        [SerializeField, Min(1)] private int _projectilesPerShot = 1;

        [Tooltip("Angle in degrees the projectiles of one shot are spread over evenly (fan); unused with one projectile.")]
        [SerializeField, Range(0f, 90f)] private float _fanAngle;

        [Tooltip("Largest random deviation in degrees to either side of every projectile.")]
        [SerializeField, Range(0f, 30f)] private float _spread;

        [Tooltip("Distance in units after which a projectile disappears.")]
        [SerializeField, Min(0f)] private float _range = 10f;

        [Tooltip("Projectile speed in units (tiles) per second.")]
        [SerializeField, Min(0f)] private float _projectileSpeed = 14f;

        [Tooltip("Sound of one shot; empty uses the default shot sound.")]
        [SerializeField] private AudioClip _shotSound;

        [Tooltip("Icon for the HUD, the swap button and the weapon selection.")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Normal look of its projectiles; empty keeps the bullet of the projectile prefab. Perk looks win.")]
        [SerializeField] private Sprite _projectileSprite;

        [Tooltip("Fires from two muzzles in turn, left and right of the aim (dual pistols, W2).")]
        [SerializeField] private bool _alternateMuzzles;

        [Tooltip("Distance in units of each muzzle from the aim line when alternating.")]
        [SerializeField, Min(0f)] private float _muzzleSeparation = 0.18f;

        /// <summary>Name shown in menus and on the HUD.</summary>
        public string DisplayName => _displayName;

        /// <summary>Shots per second while the trigger is held.</summary>
        public float FireRate => _fireRate;

        /// <summary>Damage per projectile hit.</summary>
        public int Damage => _damage;

        /// <summary>Projectiles fired with every shot; at least 1.</summary>
        public int ProjectilesPerShot => Mathf.Max(1, _projectilesPerShot);

        /// <summary>Angle in degrees the projectiles of one shot are spread over evenly.</summary>
        public float FanAngle => _fanAngle;

        /// <summary>Largest random deviation in degrees to either side of every projectile.</summary>
        public float Spread => _spread;

        /// <summary>Distance in units after which a projectile disappears.</summary>
        public float Range => _range;

        /// <summary>Projectile speed in units per second.</summary>
        public float ProjectileSpeed => _projectileSpeed;

        /// <summary>Sound of one shot; null uses the default shot sound.</summary>
        public AudioClip ShotSound => _shotSound;

        /// <summary>Icon for the HUD, the swap button and the weapon selection.</summary>
        public Sprite Icon => _icon;

        /// <summary>Normal look of its projectiles; null keeps the prefab's bullet.</summary>
        public Sprite ProjectileSprite => _projectileSprite;

        /// <summary>True if shots alternate between a left and a right muzzle.</summary>
        public bool AlternateMuzzles => _alternateMuzzles;

        /// <summary>Distance of each muzzle from the aim line when alternating.</summary>
        public float MuzzleSeparation => _muzzleSeparation;

        /// <summary>Damage per second without modifiers (W7).</summary>
        public float DamagePerSecond => WeaponMath.DamagePerSecond(_fireRate, _damage, ProjectilesPerShot);
    }
}
