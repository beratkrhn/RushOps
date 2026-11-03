using System;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// A dropped perk on the floor (F-PRK-01, F-PRK-03): its icon floats above a pulsing glow, the player collects
    /// it by touching it, and it blinks before it despawns after <see cref="LootConfig.Lifetime"/>. Pooled by
    /// <see cref="PerkPickupPool"/>; the trigger collider sits on this object.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PerkPickup : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _icon;
        [SerializeField] private SpriteRenderer _glow;

        private readonly PickupLifetime _lifetime = new PickupLifetime();
        private LootConfig _config;
        private Action<PerkPickup> _release;
        private Vector3 _iconRest;
        private Color _glowColor = Color.white;
        private bool _active;

        /// <summary>Raised when the player touched the pickup, before it returns to the pool.</summary>
        public event Action<PerkPickup> Collected;

        /// <summary>Raised when the pickup ran out of time, before it returns to the pool.</summary>
        public event Action<PerkPickup> Expired;

        /// <summary>The dropped perk.</summary>
        public PerkDefinition Perk { get; private set; }

        /// <summary>True from the drop until it is collected or expires.</summary>
        public bool IsActive => _active;

        /// <summary>Seconds on the floor and blinking state.</summary>
        public PickupLifetime Lifetime => _lifetime;

        /// <summary>True while the icon shows (false during the off phases of the blinking).</summary>
        public bool IsIconVisible => _icon != null && _icon.enabled;

        /// <summary>
        /// Drops <paramref name="perk"/> at <paramref name="position"/>. <paramref name="release"/> is called when it
        /// is collected or expires; without it the pickup deactivates itself.
        /// </summary>
        public void Spawn(Vector2 position, PerkDefinition perk, LootConfig config, Action<PerkPickup> release)
        {
            Perk = perk;
            _config = config;
            _release = release;
            transform.position = position;
            _lifetime.Reset(config != null ? config.Lifetime : 0f, config != null ? config.BlinkDuration : 0f);
            if (_icon != null)
            {
                _icon.sprite = perk != null ? perk.Icon : null;
            }

            _active = true;
            Refresh();
        }

        /// <summary>Collects the perk, as if the player touched it. False if it was already gone.</summary>
        public bool Collect()
        {
            if (!_active)
            {
                return false;
            }

            _active = false;
            Collected?.Invoke(this);
            Finish();
            return true;
        }

        private void Awake()
        {
            if (_icon != null)
            {
                _iconRest = _icon.transform.localPosition;
            }

            if (_glow != null)
            {
                _glowColor = _glow.color;
            }
        }

        private void Update()
        {
            if (!_active)
            {
                return;
            }

            if (_lifetime.Tick(Time.deltaTime))
            {
                _active = false;
                Expired?.Invoke(this);
                Finish();
                return;
            }

            Refresh();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (_active && body != null && body.CompareTag("Player") && IsAlive(body))
            {
                Collect();
            }
        }

        // A dead player cannot pick anything up.
        private static bool IsAlive(Rigidbody2D body)
        {
            return !body.TryGetComponent(out PlayerHealth health) || !health.IsDead;
        }

        private void Refresh()
        {
            if (_config == null)
            {
                return;
            }

            float elapsed = _lifetime.Elapsed;
            bool visible = _lifetime.IsVisible(_config.BlinkInterval);
            if (_icon != null)
            {
                _icon.enabled = visible;
                _icon.transform.localPosition = _iconRest + Vector3.up * PickupMath.Bob(elapsed, _config.BobHeight, _config.BobPeriod);
            }

            if (_glow != null)
            {
                _glow.enabled = visible;
                Color color = _glowColor;
                color.a = _glowColor.a * PickupMath.Pulse(elapsed, _config.GlowMin, _config.GlowMax, _config.GlowPeriod);
                _glow.color = color;
            }
        }

        private void Finish()
        {
            Action<PerkPickup> release = _release;
            _release = null;
            if (release != null)
            {
                release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
