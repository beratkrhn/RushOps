using System.Collections.Generic;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Feel;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// The flamethrower super weapon (F-PRK-10, issue #254, W13, W14): while <see cref="PlayerSuperWeapon"/> holds it,
    /// the player carries it in the hand, and while the player aims (F-STE-02) a flame burns from its nozzle in a cone
    /// towards the aim, corrected by the aim assist with the flamethrower's range. Every enemy and boss in the cone,
    /// not behind a wall, takes the flame's damage per second and keeps burning for the afterburn after leaving it,
    /// with small flames from a <see cref="SpriteEffectPool"/> licking up its body. Double damage and insta-kill of the
    /// <see cref="PlayerWeapon"/> apply; kills count like the player's own, since they are ordinary hits. A looping
    /// sound plays while the flame burns. Death and level end put out every fire. Thin adapter; the rules live in
    /// <see cref="FlamethrowerRules"/> and <see cref="FlameBurns{T}"/>, the values in <see cref="FlamethrowerConfig"/>.
    /// </summary>
    public class Flamethrower : MonoBehaviour
    {
        // Most targets burning at once and colliders looked at per frame; more than any level spawns at a time.
        private const int Capacity = 64;
        // Without a nozzle table: hand height in front of the body.
        private const float FallbackNozzleHeight = 0.6f;
        private const float FallbackNozzleDistance = 0.7f;

        [SerializeField] private FlamethrowerConfig _config;
        [SerializeField] private PlayerSuperWeapon _superWeapon;
        [Tooltip("Gives the aim and whether the player aims.")]
        [SerializeField] private PlayerAim _aim;
        [Tooltip("Corrects the aim towards an enemy in range; optional.")]
        [SerializeField] private AimAssist _assist;
        [Tooltip("Gives double damage and insta-kill.")]
        [SerializeField] private PlayerWeapon _weapon;
        [Tooltip("Animator of the player figure; its direction picks the hand sprite and the nozzle.")]
        [SerializeField] private DirectionalSpriteAnimator _view;
        [Tooltip("Hides the weapon during the dash roll; optional.")]
        [SerializeField] private PlayerDash _dash;
        [SerializeField] private PlayerHealth _health;
        [Tooltip("Puts out every fire when the level is over.")]
        [SerializeField] private LevelResults _results;
        [Tooltip("Mutes the flame while the game is paused; optional.")]
        [SerializeField] private PauseController _pause;

        [Tooltip("Layers of the hurtboxes of enemies and bosses.")]
        [SerializeField] private LayerMask _targets;
        [Tooltip("Layers that block the flame.")]
        [SerializeField] private LayerMask _walls;

        [Tooltip("Nozzle per direction (E, NE, N, NW, W, SW, S, SE) in units from the feet; "
            + "from Art/Sprites/Flamethrower/Flamethrower_Muzzles.json (Tools/gen_pixelart.py).")]
        [SerializeField] private Vector2[] _nozzles = new Vector2[Direction8Math.Count];

        [Tooltip("Shows the flamethrower in the hand, laid over the player sprite.")]
        [SerializeField] private SpriteRenderer _hand;
        [Tooltip("Hand sprite per direction (E, NE, N, NW, W, SW, S, SE).")]
        [SerializeField] private Sprite[] _handSprites = new Sprite[Direction8Math.Count];

        [Tooltip("Sits on the nozzle and turns towards the aim; switched off while there is no flame.")]
        [SerializeField] private Transform _flamePivot;
        [SerializeField] private SpriteRenderer _flame;
        [Tooltip("Flame loop frames, pointing right from the nozzle.")]
        [SerializeField] private Sprite[] _flameFrames = System.Array.Empty<Sprite>();
        [Tooltip("Length of the flame sprite in units; it is stretched to the configured range.")]
        [SerializeField, Min(0.01f)] private float _flameLength = 3.5f;

        [SerializeField] private SpriteEffectPool _burnEffects;
        [SerializeField] private Sprite[] _burnFrames = System.Array.Empty<Sprite>();

        [Tooltip("Plays the flame loop; set to loop.")]
        [SerializeField] private AudioSource _audio;

        private readonly FlameBurns<IAreaTarget> _burns = new FlameBurns<IAreaTarget>(Capacity);
        private readonly List<Collider2D> _hits = new List<Collider2D>(Capacity);
        private readonly Dictionary<Collider2D, IAreaTarget> _owners = new Dictionary<Collider2D, IAreaTarget>(Capacity);
        private ContactFilter2D _filter;
        private float _flameAge;

        /// <summary>True while the flamethrower is in the hands.</summary>
        public bool IsHeld => _superWeapon != null && _superWeapon.Kind == SuperWeaponKind.Flamethrower;

        /// <summary>True while the flame burns, i.e. the flamethrower is held and the player aims.</summary>
        public bool IsFlaming { get; private set; }

        /// <summary>Enemies and bosses in the flame or still burning.</summary>
        public int BurningCount => _burns.Count;

        /// <summary>True if <paramref name="target"/> burns on after leaving the flame (W13).</summary>
        public bool IsBurning(IAreaTarget target)
        {
            return _burns.IsBurning(target);
        }

        /// <summary>Puts out every fire at once, e.g. at death or level end.</summary>
        public void Extinguish()
        {
            _burns.Clear();
            SetFlame(false);
        }

        private void Awake()
        {
            _filter = new ContactFilter2D { useTriggers = true };
            _filter.SetLayerMask(_targets);
            SetFlame(false);
            if (_hand != null)
            {
                _hand.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.Died += Extinguish;
            }

            if (_results != null)
            {
                _results.Completed += OnCompleted;
                _results.Failed += Extinguish;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Died -= Extinguish;
            }

            if (_results != null)
            {
                _results.Completed -= OnCompleted;
                _results.Failed -= Extinguish;
            }

            Extinguish();
        }

        private void OnCompleted(int stars, int score)
        {
            Extinguish();
        }

        private void Update()
        {
            if (_config == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            bool held = IsHeld && (_health == null || !_health.IsDead);
            bool dashing = _dash != null && _dash.IsDashing;
            Direction8 facing = _view != null ? _view.Direction : Direction8.S;
            ShowHand(held && !dashing, facing);

            bool flaming = held && !dashing && _aim != null && _aim.IsAiming;
            if (flaming)
            {
                Vector2 direction = _aim.AimDirection;
                Vector2 origin = NozzlePosition(facing, direction);
                if (_assist != null)
                {
                    direction = _assist.Adjust(origin, direction, _config.Range);
                }

                Ignite(origin, direction, dt);
                PlaceFlame(origin, direction, dt);
            }

            SetFlame(flaming);
            PayOutBurns(dt);
        }

        private Vector2 NozzlePosition(Direction8 facing, Vector2 direction)
        {
            Vector2 fallback = Vector2.up * FallbackNozzleHeight + direction.normalized * FallbackNozzleDistance;
            return (Vector2)transform.position + MuzzleMath.Offset(_nozzles, facing, fallback);
        }

        // Every target in the cone this frame collects the flame's damage and has its afterburn refreshed.
        private void Ignite(Vector2 origin, Vector2 direction, float dt)
        {
            float range = _config.Range;
            float damagePerSecond = _config.DamagePerSecond * DamageMultiplier;
            int count = Physics2D.OverlapCircle(origin, range, _filter, _hits);
            for (int i = 0; i < count; i++)
            {
                Collider2D hurtbox = _hits[i];
                IAreaTarget target = Owner(hurtbox);
                if (target == null || !target.CanBeHit)
                {
                    continue;
                }

                // Large bodies count as soon as the part nearest to the nozzle is in the flame.
                Vector2 point = target.AimPoint;
                if (!FlamethrowerRules.InFlame(origin, direction, range, _config.ConeAngle, point))
                {
                    point = hurtbox.ClosestPoint(origin);
                    if (!FlamethrowerRules.InFlame(origin, direction, range, _config.ConeAngle, point))
                    {
                        continue;
                    }
                }

                if (_walls.value != 0 && Physics2D.Linecast(origin, point, _walls).collider != null)
                {
                    continue;
                }

                _burns.Expose(target, damagePerSecond, dt, _config.AfterburnDuration);
            }
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

        private void PayOutBurns(float dt)
        {
            float afterburn = _config.AfterburnDamagePerSecond * DamageMultiplier;
            if (!_burns.Tick(dt, afterburn, _config.TickInterval))
            {
                return;
            }

            bool lethal = _weapon != null && _weapon.Modifiers.InstaKill;
            for (int i = _burns.Count - 1; i >= 0; i--)
            {
                IAreaTarget target = _burns.Target(i);
                if (!target.CanBeHit)
                {
                    _burns.RemoveAt(i);
                    continue;
                }

                int damage = _burns.TakeDamage(i);
                if (damage > 0)
                {
                    // No direction: fire does not push enemies back.
                    target.TakeHit(damage, Vector2.zero, lethal);
                }

                if (!target.CanBeHit)
                {
                    _burns.RemoveAt(i);
                }
                else if (_burnEffects != null)
                {
                    _burnEffects.Play(target.AimPoint, _burnFrames, _config.BurnFrameDuration);
                }
            }
        }

        private float DamageMultiplier => _weapon != null ? Mathf.Max(0f, _weapon.DamageMultiplier) : 1f;

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

        private void PlaceFlame(Vector2 origin, Vector2 direction, float dt)
        {
            if (_flamePivot == null)
            {
                return;
            }

            _flamePivot.position = origin;
            _flamePivot.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            _flamePivot.localScale = new Vector3(_config.Range / _flameLength, 1f, 1f);
            _flameAge += dt;
            if (_flame != null && _flameFrames.Length > 0)
            {
                int frame = Mathf.FloorToInt(_flameAge * _config.FlameFrameRate) % _flameFrames.Length;
                _flame.sprite = _flameFrames[frame];
            }
        }

        private void SetFlame(bool on)
        {
            IsFlaming = on;
            if (_flamePivot != null && _flamePivot.gameObject.activeSelf != on)
            {
                _flamePivot.gameObject.SetActive(on);
            }

            if (!on)
            {
                _flameAge = 0f;
            }

            if (_audio == null)
            {
                return;
            }

            bool audible = on && (_pause == null || !_pause.IsPaused);
            if (audible && !_audio.isPlaying)
            {
                _audio.volume = _config != null ? _config.SoundVolume : 1f;
                _audio.Play();
            }
            else if (!audible && _audio.isPlaying)
            {
                _audio.Stop();
            }
        }
    }
}
