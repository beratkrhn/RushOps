using System;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Fires automatically while the player aims (F-STE-02): rate from <see cref="WeaponConfig"/>,
    /// projectiles from a <see cref="ProjectilePool"/>, unlimited ammo without reloading (F-SPF-04).
    /// <see cref="Modifiers"/> change damage and fire rate for a while, e.g. from perks (issue #25).
    /// </summary>
    [RequireComponent(typeof(PlayerAim))]
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponConfig _config;
        [SerializeField] private ProjectilePool _pool;

        [Tooltip("Layers that stop projectiles, e.g. walls and enemies; enemies take damage.")]
        [SerializeField] private LayerMask _obstacles;

        [Tooltip("Height of the gun above the feet (transform pivot) in units.")]
        [SerializeField] private float _muzzleHeight = 0.6f;

        [Tooltip("Distance of the muzzle from the body centre along the aim direction in units.")]
        [SerializeField] private float _muzzleDistance = 0.4f;

        private readonly FireRateTimer _timer = new FireRateTimer();
        private PlayerAim _aim;
        private AimAssist _assist;
        private ShotModifiers _modifiers = ShotModifiers.None;

        /// <summary>Raised once per shot with its direction, e.g. for the shot sound.</summary>
        public event Action<Vector2> Fired;

        /// <summary>Replaces the config, e.g. for tests or temporary modifiers.</summary>
        public WeaponConfig Config
        {
            get => _config;
            set => _config = value;
        }

        /// <summary>Temporary changes to damage, fire rate and insta-kill; <see cref="ShotModifiers.None"/> by default.</summary>
        public ShotModifiers Modifiers
        {
            get => _modifiers;
            set => _modifiers = value;
        }

        /// <summary>Shots per second with the current <see cref="Modifiers"/>; 0 without config.</summary>
        public float FireRate => _config != null ? WeaponMath.FireRate(_config.FireRate, _modifiers.FireRateMultiplier) : 0f;

        /// <summary>Damage per hit with the current <see cref="Modifiers"/>; 0 without config.</summary>
        public int Damage => _config != null ? WeaponMath.ShotDamage(_config.Damage, _modifiers.DamageMultiplier) : 0;

        /// <summary>Point where projectiles start when firing towards <paramref name="direction"/>.</summary>
        public Vector2 MuzzlePosition(Vector2 direction)
        {
            return (Vector2)transform.position + Vector2.up * _muzzleHeight + direction * _muzzleDistance;
        }

        private void Awake()
        {
            _aim = GetComponent<PlayerAim>();
            _assist = GetComponent<AimAssist>();
        }

        private void Update()
        {
            if (_config == null || _pool == null)
            {
                return;
            }

            int shots = _timer.Tick(Time.deltaTime, _aim.IsAiming, FireRate);
            if (shots == 0)
            {
                return;
            }

            Vector2 direction = _aim.AimDirection;
            Vector2 origin = MuzzlePosition(direction);
            if (_assist != null)
            {
                direction = _assist.Adjust(origin, direction, _config);
            }

            int damage = Damage;
            ShotLook look = WeaponMath.Look(_modifiers);
            for (int i = 0; i < shots; i++)
            {
                _pool.Launch(origin, direction, _config.ProjectileSpeed, _config.MaxRange, _obstacles, damage,
                    _modifiers.InstaKill, look);
                Fired?.Invoke(direction);
            }
        }

        private void OnDisable()
        {
            _timer.Reset();
        }
    }
}
