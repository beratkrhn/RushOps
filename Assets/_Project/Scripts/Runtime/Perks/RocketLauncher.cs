using System;
using System.Collections.Generic;
using RushOps.Audio;
using RushOps.Bosses;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Feel;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// The rocket launcher super weapon (F-PRK-11, F-PRK-12, issue #255, W12, W14): while <see cref="PlayerSuperWeapon"/>
    /// holds it, the player carries it in the hand, and while the player aims (F-STE-02) it fires rockets at the
    /// configured rate towards the aim, corrected by the aim assist with the launcher's range, so they go for the
    /// nearest enemy. Rockets come from the player's <see cref="ProjectilePool"/> with the rocket sprite and explode at the first enemy or wall
    /// they hit: every enemy and boss whose body reaches into the radius takes the explosion's damage, bosses the full
    /// value. The explosion only looks for enemy hurtboxes, so it never hurts the player. Double damage, rapid fire and
    /// insta-kill of the <see cref="PlayerWeapon"/> apply; kills count like the player's own, since they are ordinary
    /// hits. Death and level end clear the rockets in flight. Thin adapter; the rules live in <see cref="RocketRules"/>,
    /// the values in <see cref="RocketLauncherConfig"/>.
    /// </summary>
    public class RocketLauncher : MonoBehaviour
    {
        // Most hurtboxes looked at per explosion; more than any level crowds into one blast.
        private const int Capacity = 64;
        // Without a muzzle table: hand height in front of the body.
        private const float FallbackMuzzleHeight = 0.6f;
        private const float FallbackMuzzleDistance = 0.6f;

        [SerializeField] private RocketLauncherConfig _config;
        [SerializeField] private PlayerSuperWeapon _superWeapon;
        [Tooltip("Gives the aim and whether the player aims.")]
        [SerializeField] private PlayerAim _aim;
        [Tooltip("Corrects the aim towards an enemy in range; optional.")]
        [SerializeField] private AimAssist _assist;
        [Tooltip("Gives double damage, rapid fire and insta-kill.")]
        [SerializeField] private PlayerWeapon _weapon;
        [Tooltip("Animator of the player figure; its direction picks the hand sprite and the muzzle.")]
        [SerializeField] private DirectionalSpriteAnimator _view;
        [Tooltip("Hides the weapon during the dash roll; optional.")]
        [SerializeField] private PlayerDash _dash;
        [SerializeField] private PlayerHealth _health;
        [Tooltip("Clears the rockets when the level is over.")]
        [SerializeField] private LevelResults _results;

        [Tooltip("The player's projectile pool; rockets fly from it with their own sprite.")]
        [SerializeField] private ProjectilePool _rockets;
        [Tooltip("Layers that stop a rocket: walls, enemy bodies and hurtboxes.")]
        [SerializeField] private LayerMask _obstacles;
        [Tooltip("Layers of the hurtboxes of enemies and bosses; the player is never on them (W12).")]
        [SerializeField] private LayerMask _targets;

        [Tooltip("Muzzle per direction (E, NE, N, NW, W, SW, S, SE) in units from the feet; "
            + "from Art/Sprites/RocketLauncher/RocketLauncher_Muzzles.json (Tools/gen_pixelart.py).")]
        [SerializeField] private Vector2[] _muzzles = new Vector2[Direction8Math.Count];

        [Tooltip("Shows the launcher in the hand, laid over the player sprite.")]
        [SerializeField] private SpriteRenderer _hand;
        [Tooltip("Hand sprite per direction (E, NE, N, NW, W, SW, S, SE).")]
        [SerializeField] private Sprite[] _handSprites = new Sprite[Direction8Math.Count];
        [Tooltip("Sprite of a rocket in flight, pointing right.")]
        [SerializeField] private Sprite _rocketSprite;

        [SerializeField] private SpriteEffectPool _explosionEffects;
        [SerializeField] private Sprite[] _explosionFrames = Array.Empty<Sprite>();

        [SerializeField] private SfxPlayer _sfx;
        [SerializeField] private AudioClip _launchClip;
        [SerializeField] private AudioClip _explosionClip;

        private readonly FireRateTimer _timer = new FireRateTimer();
        private readonly List<Collider2D> _hits = new List<Collider2D>(Capacity);
        private readonly HashSet<IAreaTarget> _struck = new HashSet<IAreaTarget>();
        private readonly Dictionary<Collider2D, IAreaTarget> _owners = new Dictionary<Collider2D, IAreaTarget>(Capacity);
        private ContactFilter2D _filter;
        private Action<Projectile, Vector2> _onImpact;

        /// <summary>Raised after an explosion dealt its damage, with its center.</summary>
        public event Action<Vector2> Exploded;

        /// <summary>Raised once per rocket fired, with its direction.</summary>
        public event Action<Vector2> Fired;

        /// <summary>True while the rocket launcher is in the hands.</summary>
        public bool IsHeld => _superWeapon != null && _superWeapon.Kind == SuperWeaponKind.RocketLauncher;

        /// <summary>Rockets in flight.</summary>
        public int RocketsInFlight => _rockets != null ? _rockets.CountCarrying(_onImpact) : 0;

        /// <summary>Removes every rocket in flight without an explosion, e.g. at death or level end.</summary>
        public void ClearRockets()
        {
            if (_rockets != null)
            {
                _rockets.CancelAll(_onImpact);
            }
        }

        /// <summary>
        /// Sets off a rocket explosion at <paramref name="center"/>: every enemy and boss whose body reaches into the
        /// radius takes the explosion's damage once. The player is never hit (W12).
        /// </summary>
        public void Explode(Vector2 center)
        {
            if (_config == null)
            {
                return;
            }

            float radius = _config.ExplosionRadius;
            float multiplier = _weapon != null ? _weapon.Modifiers.DamageMultiplier : 1f;
            bool lethal = _weapon != null && _weapon.Modifiers.InstaKill;
            _struck.Clear();
            int count = Physics2D.OverlapCircle(center, radius, _filter, _hits);
            for (int i = 0; i < count; i++)
            {
                Collider2D hurtbox = _hits[i];
                IAreaTarget target = Owner(hurtbox);
                if (target == null || !target.CanBeHit || _struck.Contains(target))
                {
                    continue;
                }

                // Large bodies count as soon as the part nearest to the center is in the radius.
                if (!RocketRules.InBlast(center, radius, target.AimPoint, hurtbox.ClosestPoint(center)))
                {
                    continue;
                }

                _struck.Add(target);
                int damage = RocketRules.ScaledDamage(
                    RocketRules.Damage(_config.ExplosionDamage, target is Boss), multiplier);
                if (damage > 0)
                {
                    target.TakeHit(damage, RocketRules.PushDirection(center, target.AimPoint), lethal);
                }
            }

            if (_explosionEffects != null)
            {
                _explosionEffects.Play(center, _explosionFrames, _config.ExplosionFrameDuration);
            }

            if (_sfx != null)
            {
                _sfx.Play(_explosionClip, _config.ExplosionVolume, 0.08f);
            }

            Exploded?.Invoke(center);
        }

        private void Awake()
        {
            _filter = new ContactFilter2D { useTriggers = true };
            _filter.SetLayerMask(_targets);
            _onImpact = OnImpact;
            if (_hand != null)
            {
                _hand.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.Died += ClearRockets;
            }

            if (_results != null)
            {
                _results.Completed += OnCompleted;
                _results.Failed += ClearRockets;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Died -= ClearRockets;
            }

            if (_results != null)
            {
                _results.Completed -= OnCompleted;
                _results.Failed -= ClearRockets;
            }

            _timer.Reset();
            ClearRockets();
        }

        private void OnCompleted(int stars, int score)
        {
            ClearRockets();
        }

        private void OnImpact(Projectile rocket, Vector2 point)
        {
            Explode(point);
        }

        private void Update()
        {
            if (_config == null)
            {
                return;
            }

            bool held = IsHeld && (_health == null || !_health.IsDead);
            bool dashing = _dash != null && _dash.IsDashing;
            Direction8 facing = _view != null ? _view.Direction : Direction8.S;
            ShowHand(held && !dashing, facing);
            if (!held)
            {
                _timer.Reset();
                return;
            }

            bool firing = !dashing && _aim != null && _aim.IsAiming;
            int shots = _timer.Tick(Time.deltaTime, firing, FireRate);
            if (shots == 0 || _rockets == null)
            {
                return;
            }

            Vector2 direction = _aim.AimDirection;
            Vector2 origin = MuzzlePosition(facing, direction);
            if (_assist != null)
            {
                direction = _assist.Adjust(origin, direction, _config.Range);
            }

            for (int i = 0; i < shots; i++)
            {
                _rockets.Launch(origin, direction, _config.RocketSpeed, _config.Range, _obstacles, 0, false,
                    ShotLook.Normal, _rocketSprite, _onImpact);
                Fired?.Invoke(direction);
            }

            if (_sfx != null)
            {
                _sfx.Play(_launchClip, _config.LaunchVolume, 0.06f);
            }
        }

        private float FireRate => _weapon != null
            ? WeaponMath.FireRate(_config.FireRate, _weapon.Modifiers.FireRateMultiplier)
            : _config.FireRate;

        private Vector2 MuzzlePosition(Direction8 facing, Vector2 direction)
        {
            Vector2 fallback = Vector2.up * FallbackMuzzleHeight + direction.normalized * FallbackMuzzleDistance;
            return (Vector2)transform.position + MuzzleMath.Offset(_muzzles, facing, fallback);
        }

        // The component behind a hurtbox, looked up once per collider.
        private IAreaTarget Owner(Collider2D hurtbox)
        {
            if (!_owners.TryGetValue(hurtbox, out IAreaTarget target))
            {
                target = hurtbox.GetComponentInParent<IAreaTarget>();
                _owners[hurtbox] = target;
            }

            return target;
        }

        private void ShowHand(bool visible, Direction8 facing)
        {
            if (_hand == null)
            {
                return;
            }

            _hand.enabled = visible;
            int index = (int)facing;
            if (visible && index < _handSprites.Length)
            {
                _hand.sprite = _handSprites[index];
            }
        }
    }
}
