using System;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Fires the active weapon of <see cref="Slots"/> automatically while the player aims (F-STE-02): rate, damage,
    /// spread and projectiles per shot from its <see cref="WeaponDefinition"/> (issue #173), projectiles from a
    /// <see cref="ProjectilePool"/>, unlimited ammo without reloading (F-SPF-04). <see cref="Modifiers"/> change damage
    /// and fire rate of every weapon for a while, e.g. from perks (issue #25).
    /// </summary>
    [RequireComponent(typeof(PlayerAim))]
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponConfig _config;
        [SerializeField] private ProjectilePool _pool;

        [Tooltip("Layers that stop projectiles, e.g. walls and enemies; enemies take damage.")]
        [SerializeField] private LayerMask _obstacles;

        [Tooltip("Muzzle of the pistol in the hand per direction (E, NE, N, NW, W, SW, S, SE), in units from the feet; "
            + "from Art/Sprites/Player/Player_Muzzles.json (Tools/gen_pixelart.py).")]
        [SerializeField] private Vector2[] _muzzles = new Vector2[Direction8Math.Count];

        // Without a muzzle table: hand height in front of the body.
        private const float FallbackMuzzleHeight = 0.6f;
        private const float FallbackMuzzleDistance = 0.4f;

        private readonly FireRateTimer _timer = new FireRateTimer();
        private readonly IRandomSource _random = new SystemRandomSource();
        private WeaponSlots _slots;
        private PlayerAim _aim;
        private AimAssist _assist;
        private DirectionalSpriteAnimator _view;
        private ShotModifiers _modifiers = ShotModifiers.None;
        private int _shotCount;

        /// <summary>Shots fired so far; alternating weapons pick their muzzle from it.</summary>
        public int ShotCount => _shotCount;

        /// <summary>Raised once per shot with its direction, e.g. for the shot sound.</summary>
        public event Action<Vector2> Fired;

        /// <summary>Shared weapon settings; replacing it starts over with its start weapon, e.g. in tests.</summary>
        public WeaponConfig Config
        {
            get => _config;
            set
            {
                _config = value;
                Slots.Reset(StartWeapon);
            }
        }

        /// <summary>The two weapons the player carries; starts with the start weapon of the config.</summary>
        public WeaponSlots Slots => _slots ??= new WeaponSlots(StartWeapon);

        /// <summary>The weapon that fires; null without config.</summary>
        public WeaponDefinition ActiveWeapon => Slots.Active;

        private WeaponDefinition StartWeapon => _config != null ? _config.StartWeapon : null;

        /// <summary>Temporary changes to damage, fire rate and insta-kill; <see cref="ShotModifiers.None"/> by default.</summary>
        public ShotModifiers Modifiers
        {
            get => _modifiers;
            set => _modifiers = value;
        }

        /// <summary>Shots per second of the active weapon with the current <see cref="Modifiers"/>; 0 without weapon.</summary>
        public float FireRate
        {
            get
            {
                WeaponDefinition weapon = ActiveWeapon;
                return weapon != null ? WeaponMath.FireRate(weapon.FireRate, _modifiers.FireRateMultiplier) : 0f;
            }
        }

        /// <summary>Damage per hit of the active weapon with the current <see cref="Modifiers"/>; 0 without weapon.</summary>
        public int Damage
        {
            get
            {
                WeaponDefinition weapon = ActiveWeapon;
                return weapon != null ? WeaponMath.ShotDamage(weapon.Damage, _modifiers.DamageMultiplier) : 0;
            }
        }

        /// <summary>
        /// Point where projectiles start when firing towards <paramref name="direction"/>: the muzzle of the pistol in
        /// the sprite the player shows (zero uses the current facing).
        /// </summary>
        public Vector2 MuzzlePosition(Vector2 direction)
        {
            Direction8 shown = _view != null ? _view.Direction : Direction8.S;
            Direction8 facing = _view != null ? shown : Direction8Math.FromVector(direction, shown);
            Vector2 fallback = Vector2.up * FallbackMuzzleHeight + direction.normalized * FallbackMuzzleDistance;
            return (Vector2)transform.position + MuzzleMath.Offset(_muzzles, facing, fallback);
        }

        private void Awake()
        {
            _aim = GetComponent<PlayerAim>();
            _assist = GetComponent<AimAssist>();
            _view = GetComponent<DirectionalSpriteAnimator>();
        }

        private void Update()
        {
            WeaponDefinition weapon = ActiveWeapon;
            if (weapon == null || _pool == null)
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
                direction = _assist.Adjust(origin, direction, weapon.Range);
            }

            int damage = Damage;
            ShotLook look = WeaponMath.Look(_modifiers);
            int projectiles = weapon.ProjectilesPerShot;
            for (int i = 0; i < shots; i++)
            {
                Vector2 muzzle = weapon.AlternateMuzzles
                    ? origin + WeaponMath.AlternateMuzzleOffset(direction, _shotCount, weapon.MuzzleSeparation)
                    : origin;
                _shotCount++;
                for (int p = 0; p < projectiles; p++)
                {
                    // Only roll when needed, so weapons without spread fire exactly along the aim.
                    float random = weapon.Spread > 0f ? _random.Value() : 0.5f;
                    Vector2 projectileDirection = WeaponMath.ProjectileDirection(
                        direction, p, projectiles, weapon.FanAngle, weapon.Spread, random);
                    _pool.Launch(muzzle, projectileDirection, weapon.ProjectileSpeed, weapon.Range, _obstacles, damage,
                        _modifiers.InstaKill, look, weapon.ProjectileSprite);
                }

                Fired?.Invoke(direction);
            }
        }

        private void OnDisable()
        {
            _timer.Reset();
        }
    }
}
