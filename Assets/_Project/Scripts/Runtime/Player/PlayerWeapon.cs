using System;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Fires automatically while the player aims (F-STE-02): rate from <see cref="WeaponConfig"/>,
    /// projectiles from a <see cref="ProjectilePool"/>, unlimited ammo without reloading (F-SPF-04).
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

        /// <summary>Raised once per shot with its direction, e.g. for the shot sound.</summary>
        public event Action<Vector2> Fired;

        /// <summary>Replaces the config, e.g. for tests or temporary modifiers.</summary>
        public WeaponConfig Config
        {
            get => _config;
            set => _config = value;
        }

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

            int shots = _timer.Tick(Time.deltaTime, _aim.IsAiming, _config.FireRate);
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

            for (int i = 0; i < shots; i++)
            {
                _pool.Launch(origin, direction, _config.ProjectileSpeed, _config.MaxRange, _obstacles, _config.Damage);
                Fired?.Invoke(direction);
            }
        }

        private void OnDisable()
        {
            _timer.Reset();
        }
    }
}
