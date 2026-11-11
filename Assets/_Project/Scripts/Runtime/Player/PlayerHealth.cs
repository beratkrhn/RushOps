using System;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// The player's hearts (F-SPF-01 to F-SPF-03): takes contact damage from colliders with an
    /// <see cref="IContactDamage"/>, blinks during the invulnerability after a hit and stops the player on death.
    /// Dash i-frames and an <see cref="IDamageBlocker"/> on the player (shield perk, issue #26) also protect. Values come from <see cref="PlayerConfig"/>; the rules live in <see cref="Health"/>.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerHealth : MonoBehaviour
    {
        [Tooltip("Blinks during the invulnerability after a hit.")]
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Tint of the sprite after death.")]
        [SerializeField] private Color _deadTint = new Color(1f, 0.35f, 0.35f, 1f);
        [Tooltip("Components switched off on death, e.g. input, weapon and movement.")]
        [SerializeField] private Behaviour[] _disableOnDeath;

        private Health _health;
        private PlayerConfig _config;
        private PlayerDash _dash;
        private IDamageBlocker _blocker;
        private int _extraMax;

        /// <summary>Raised when the hearts change, with current and maximum hearts. Also raised once on start for the HUD.</summary>
        public event Action<int, int> HealthChanged;

        /// <summary>Raised once when the hearts reach 0.</summary>
        public event Action Died;

        /// <summary>Current hearts.</summary>
        public int Current => _health != null ? _health.Current : 0;

        /// <summary>Maximum hearts.</summary>
        public int Max => _health != null ? _health.Max : 0;

        /// <summary>True once the hearts reached 0.</summary>
        public bool IsDead => _health != null && _health.IsDead;

        /// <summary>True while no damage can hurt the player: after a hit, during the dash i-frames or behind a shield.</summary>
        public bool IsInvulnerable => _health == null || _health.IsInvulnerable || (_dash != null && _dash.IsInvulnerable)
            || IsBlocked;

        private bool IsBlocked => _blocker != null && _blocker.BlocksDamage;

        /// <summary>
        /// Hearts added to the maximum of the config, e.g. by the +1 heart upgrade of the endless run (F-END-12). Raising
        /// it heals the hearts gained; lowering it cuts the current hearts down to the new maximum.
        /// </summary>
        public int ExtraMax
        {
            get => _extraMax;
            set
            {
                _extraMax = Math.Max(0, value);
                if (_health != null && _config != null)
                {
                    _health.SetMax(_config.MaxHearts + _extraMax);
                }
            }
        }

        /// <summary>Removes <paramref name="hearts"/> unless the player is invulnerable or dead.</summary>
        /// <returns>True if the hit counted.</returns>
        public bool TakeDamage(int hearts)
        {
            if (_health == null || (_dash != null && _dash.IsInvulnerable) || IsBlocked)
            {
                return false;
            }

            return _health.TakeDamage(hearts);
        }

        /// <summary>Restores up to <paramref name="hearts"/>, at most to the maximum.</summary>
        /// <returns>The hearts actually restored.</returns>
        public int Heal(int hearts)
        {
            return _health != null ? _health.Heal(hearts) : 0;
        }

        private void Awake()
        {
            _config = GetComponent<PlayerMovement>().Config;
            _dash = GetComponent<PlayerDash>();
            _blocker = GetComponent<IDamageBlocker>();
            if (_config == null)
            {
                return;
            }

            _health = new Health(_config.MaxHearts + _extraMax, _config.HitInvulnerability);
            _health.Changed += OnChanged;
            _health.Died += OnDied;
        }

        private void Start()
        {
            if (_health != null)
            {
                HealthChanged?.Invoke(_health.Current, _health.Max);
            }
        }

        private void Update()
        {
            if (_health == null)
            {
                return;
            }

            _health.Tick(Time.deltaTime);
            if (_renderer != null && !_health.IsDead)
            {
                float elapsed = _config.HitInvulnerability - _health.InvulnerableRemaining;
                _renderer.enabled = !_health.IsInvulnerable || BlinkMath.IsVisible(elapsed, _config.BlinkInterval);
            }
        }

        // Enter hurts on the first touch; Stay hurts again once the invulnerability ends while still touching.
        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryContact(collision.collider);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryContact(collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryContact(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryContact(other);
        }

        private void TryContact(Collider2D other)
        {
            if (other.TryGetComponent(out IContactDamage source) || (other.attachedRigidbody != null
                && other.attachedRigidbody.TryGetComponent(out source)))
            {
                TakeDamage(source.ContactDamage);
            }
        }

        private void OnChanged(int current, int max)
        {
            HealthChanged?.Invoke(current, max);
        }

        private void OnDied()
        {
            foreach (Behaviour behaviour in _disableOnDeath)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = false;
                }
            }

            if (_renderer != null)
            {
                _renderer.enabled = true;
                _renderer.color = _deadTint;
            }

            Died?.Invoke();
        }
    }
}
