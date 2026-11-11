using System;
using System.Collections.Generic;
using RushOps.Audio;
using RushOps.Combat;
using RushOps.Feel;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// Thin adapter of the fire trail upgrade (F-END-12, issue #278): while <see cref="Active"/> and the player dashes,
    /// it lays burning tiles (<see cref="FireTrailTiles"/>) that hurt enemies and bosses standing in them, paid out in
    /// whole points like the flamethrower (<see cref="FlameBurns{T}"/>, with no afterburn damage). It only looks for enemy
    /// hurtboxes, so it never hurts the player; kills count like own kills. The flames come from a
    /// <see cref="SpriteEffectPool"/>.
    /// </summary>
    public class FireTrail : MonoBehaviour
    {
        private const int Capacity = 32;

        [SerializeField] private FireTrailConfig _config;
        [SerializeField] private PlayerDash _dash;
        [Tooltip("Its damage multiplier (perks, rush, damage upgrade) scales the fire; insta-kill makes it lethal.")]
        [SerializeField] private PlayerWeapon _weapon;
        [Tooltip("Puts the trail out on death.")]
        [SerializeField] private PlayerHealth _health;
        [Tooltip("Layers of the enemy hurtboxes.")]
        [SerializeField] private LayerMask _targets;
        [SerializeField] private SpriteEffectPool _flames;
        [Tooltip("Flame loop, played for the life of a tile.")]
        [SerializeField] private Sprite[] _frames = Array.Empty<Sprite>();
        [SerializeField] private SfxPlayer _sfx;

        private readonly FlameBurns<IAreaTarget> _burns = new FlameBurns<IAreaTarget>(Capacity);
        private readonly List<Collider2D> _hits = new List<Collider2D>(Capacity);
        private readonly Dictionary<Collider2D, IAreaTarget> _owners = new Dictionary<Collider2D, IAreaTarget>(Capacity);
        private FireTrailTiles _tiles;
        private ContactFilter2D _filter;
        private Sprite[] _loop = Array.Empty<Sprite>();
        private bool _wasDashing;

        /// <summary>Whether dashes lay a trail; set by the upgrades of the run.</summary>
        public bool Active { get; set; }

        /// <summary>The burning tiles, e.g. for tests.</summary>
        public FireTrailTiles Tiles => _tiles ??= new FireTrailTiles(_config != null ? _config.MaxTiles : 1);

        /// <summary>The values of the trail.</summary>
        public FireTrailConfig Config => _config;

        /// <summary>Lays a tile at <paramref name="position"/> as if a dash passed there, e.g. for tests.</summary>
        public void Lay(Vector2 position)
        {
            if (_config != null && Tiles.TryLay(position, _config.Spacing, _config.Duration) && _flames != null)
            {
                _flames.Play(position, _loop, _config.FrameDuration);
            }
        }

        /// <summary>Puts out every tile and fire, e.g. on death or a new run.</summary>
        public void Clear()
        {
            Tiles.Clear();
            _burns.Clear();
        }

        private void Awake()
        {
            _filter = new ContactFilter2D { useTriggers = true };
            _filter.SetLayerMask(_targets);
            _loop = BuildLoop();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.Died += Clear;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.Died -= Clear;
            }
        }

        private void Update()
        {
            if (_config == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            bool dashing = Active && _dash != null && _dash.IsDashing;
            if (dashing && !_wasDashing && _sfx != null)
            {
                _sfx.Play(_config.CrackleClip, _config.CrackleVolume, 0.1f);
            }

            if (dashing)
            {
                Lay(_dash.transform.position);
            }
            else if (_wasDashing)
            {
                Tiles.BreakChain();
            }

            _wasDashing = dashing;
            Tiles.Tick(dt);
            if (Tiles.Count == 0 && _burns.Count == 0)
            {
                return;
            }

            Ignite(dt);
            PayOut(dt);
        }

        // Every enemy body that reaches into a burning tile this frame collects the fire's damage once.
        private void Ignite(float dt)
        {
            float damagePerSecond = _config.DamagePerSecond * DamageMultiplier;
            for (int t = 0; t < Tiles.Count; t++)
            {
                int count = Physics2D.OverlapCircle(Tiles.Position(t), _config.Radius, _filter, _hits);
                for (int i = 0; i < count; i++)
                {
                    IAreaTarget target = Owner(_hits[i]);
                    if (target != null && target.CanBeHit)
                    {
                        // An afterburn of one tick keeps the collected damage until it is paid out; it burns at 0 per
                        // second, so leaving the fire stops the damage.
                        _burns.Expose(target, damagePerSecond, dt, _config.TickInterval);
                    }
                }
            }
        }

        private void PayOut(float dt)
        {
            if (!_burns.Tick(dt, 0f, _config.TickInterval))
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

        // The flame frames repeated for the life of a tile, so one pooled effect shows the whole tile.
        private Sprite[] BuildLoop()
        {
            if (_config == null || _frames == null || _frames.Length == 0)
            {
                return Array.Empty<Sprite>();
            }

            int count = Mathf.Max(1, Mathf.RoundToInt(_config.Duration / _config.FrameDuration));
            var loop = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                loop[i] = _frames[i % _frames.Length];
            }

            return loop;
        }

        private float DamageMultiplier => _weapon != null ? Mathf.Max(0f, _weapon.DamageMultiplier) : 1f;
    }
}
