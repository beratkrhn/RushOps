using System;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Loot
{
    /// <summary>
    /// A gold coin on the floor (F-GLD-02 to F-GLD-04): it spins, flies to the player inside the magnet range, is
    /// collected by touching the player, and blinks before it vanishes after <see cref="GoldConfig.Lifetime"/>.
    /// Pooled by <see cref="GoldPickupPool"/>; the trigger collider sits on this object.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GoldPickup : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Spin frames in playback order.")]
        [SerializeField] private Sprite[] _frames = Array.Empty<Sprite>();

        private readonly PickupLifetime _lifetime = new PickupLifetime();
        private GoldConfig _config;
        private Transform _player;
        private PlayerHealth _playerHealth;
        private Action<GoldPickup> _release;
        private int _spinOffset;
        private bool _active;
        private float _magnetBonus;

        /// <summary>Raised when the player touched the coin, before it returns to the pool.</summary>
        public event Action<GoldPickup> Collected;

        /// <summary>Raised when the coin ran out of time, before it returns to the pool.</summary>
        public event Action<GoldPickup> Expired;

        /// <summary>True from the drop until it is collected or expires.</summary>
        public bool IsActive => _active;

        /// <summary>Seconds on the floor and blinking state.</summary>
        public PickupLifetime Lifetime => _lifetime;

        /// <summary>True while the coin shows (false during the off phases of the blinking).</summary>
        public bool IsVisible => _renderer != null && _renderer.enabled;

        /// <summary>
        /// Drops the coin at <paramref name="position"/>. <paramref name="player"/> is what the magnet pulls towards.
        /// <paramref name="release"/> is called when it is collected or expires; without it the coin deactivates itself.
        /// </summary>
        public void Spawn(Vector2 position, GoldConfig config, Transform player, Action<GoldPickup> release)
        {
            _config = config;
            _release = release;
            _player = player;
            _playerHealth = player != null ? player.GetComponent<PlayerHealth>() : null;
            transform.position = position;
            // Coins dropped together should not spin in step.
            _spinOffset = Mathf.Abs((int)(position.x * 97f + position.y * 57f));
            _lifetime.Reset(config != null ? config.Lifetime : 0f, config != null ? config.BlinkDuration : 0f);
            _active = true;
            Refresh();
        }

        /// <summary>Units added to the magnet range of the config, e.g. by the gold magnet upgrade (F-END-12).</summary>
        public float MagnetBonus
        {
            get => _magnetBonus;
            set => _magnetBonus = Mathf.Max(0f, value);
        }

        /// <summary>Magnet range of this coin in units: the config's plus <see cref="MagnetBonus"/>.</summary>
        public float MagnetRange => (_config != null ? _config.MagnetRange : 0f) + _magnetBonus;

        /// <summary>Collects the coin, as if the player touched it. False if it was already gone.</summary>
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

            if (_config != null && _player != null && !IsPlayerDead)
            {
                transform.position = GoldMath.MagnetStep(
                    transform.position, _player.position, MagnetRange, _config.FlightSpeed, Time.deltaTime);
            }

            Refresh();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (_active && body != null && body.CompareTag("Player")
                && (!body.TryGetComponent(out PlayerHealth health) || !health.IsDead))
            {
                Collect();
            }
        }

        private bool IsPlayerDead => _playerHealth != null && _playerHealth.IsDead;

        private void Refresh()
        {
            if (_renderer == null || _config == null)
            {
                return;
            }

            _renderer.enabled = _lifetime.IsVisible(_config.BlinkInterval);
            if (_frames.Length > 0)
            {
                int frame = _config.SpinFrameDuration > 0f ? (int)(_lifetime.Elapsed / _config.SpinFrameDuration) : 0;
                _renderer.sprite = _frames[(frame + _spinOffset) % _frames.Length];
            }
        }

        private void Finish()
        {
            Action<GoldPickup> release = _release;
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
