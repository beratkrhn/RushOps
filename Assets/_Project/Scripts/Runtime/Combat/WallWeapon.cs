using System;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// A weapon hanging on a wall (issue #175, decisions W6, W8, W9): its silhouette stays on the wall; standing on the
    /// spot in front of it fills a ring, and once it is full the weapon goes into the player's second slot and becomes
    /// active, even if it is not unlocked yet. Free, and with the weapon already in hand nothing happens.
    /// Rules in <see cref="WallPickupTimer"/>. In the endless mode a weapon is locked until its round (issue #209): a dark
    /// silhouette whose ring does not fill; <see cref="Unlock"/> frees it with a short glow.
    /// </summary>
    public class WallWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition _weapon;
        [SerializeField] private WallWeaponConfig _config;
        [SerializeField] private PlayerWeapon _player;
        [Tooltip("Spot on the floor in front of the weapon, relative to this object.")]
        [SerializeField] private Vector2 _pickupOffset = new Vector2(0f, -1f);
        [Tooltip("Silhouette of the weapon on the wall; dark while locked.")]
        [SerializeField] private SpriteRenderer _silhouette;
        [Tooltip("Ring on the floor that fills while the player stands in front.")]
        [SerializeField] private SpriteRenderer _ring;
        [Tooltip("Frames of the ring from almost empty to full.")]
        [SerializeField] private Sprite[] _ringFrames = Array.Empty<Sprite>();

        private WallPickupTimer _timer;
        private float _glowRemaining;

        /// <summary>Raised when the player picked the weapon up.</summary>
        public event Action<WallWeapon> PickedUp;

        /// <summary>The weapon hanging here.</summary>
        public WeaponDefinition Weapon => _weapon;

        /// <summary>True while the weapon cannot be picked up: dark silhouette, the ring does not fill.</summary>
        public bool IsLocked { get; private set; }

        /// <summary>Locks the weapon (dark silhouette) or frees it at once, without glow.</summary>
        public void SetLocked(bool locked)
        {
            IsLocked = locked;
            _glowRemaining = 0f;
            SetTint(locked ? (_config != null ? _config.LockedTint : Color.black) : Color.white);
        }

        /// <summary>Frees a locked weapon with a short glow; does nothing if it is not locked.</summary>
        public void Unlock()
        {
            if (!IsLocked)
            {
                return;
            }

            IsLocked = false;
            _glowRemaining = _config != null ? _config.UnlockGlowSeconds : 0f;
            SetTint(_glowRemaining > 0f ? _config.UnlockGlowTint : Color.white);
        }

        /// <summary>Fill of the ring, 0..1.</summary>
        public float Progress => Timer.Progress;

        /// <summary>World position of the spot in front of the weapon.</summary>
        public Vector2 PickupPoint => (Vector2)transform.position + _pickupOffset;

        private WallPickupTimer Timer => _timer ??= new WallPickupTimer(_config != null ? _config.PickupTime : 0.5f);

        private void Update()
        {
            if (_player == null || _weapon == null)
            {
                return;
            }

            float radius = _config != null ? _config.PickupRadius : 0.8f;
            bool inFront = ((Vector2)_player.transform.position - PickupPoint).sqrMagnitude <= radius * radius;
            bool canPick = !IsLocked && _player.isActiveAndEnabled && !_player.Slots.Holds(_weapon);
            if (Timer.Tick(Time.deltaTime, inFront, canPick) && _player.Slots.PickUp(_weapon))
            {
                PickedUp?.Invoke(this);
            }

            ShowRing();
            FadeGlow();
        }

        private void FadeGlow()
        {
            if (_glowRemaining <= 0f)
            {
                return;
            }

            _glowRemaining = Mathf.Max(0f, _glowRemaining - Time.deltaTime);
            float duration = _config != null ? _config.UnlockGlowSeconds : 0f;
            float share = duration > 0f ? _glowRemaining / duration : 0f;
            SetTint(Color.Lerp(Color.white, _config != null ? _config.UnlockGlowTint : Color.white, share));
        }

        private void SetTint(Color color)
        {
            if (_silhouette != null)
            {
                _silhouette.color = color;
            }
        }

        private void ShowRing()
        {
            if (_ring == null)
            {
                return;
            }

            float progress = Timer.Progress;
            bool show = progress > 0f && _ringFrames.Length > 0;
            if (_ring.enabled != show)
            {
                _ring.enabled = show;
            }

            if (show)
            {
                int frame = Mathf.Min(_ringFrames.Length - 1, (int)(progress * _ringFrames.Length));
                _ring.sprite = _ringFrames[frame];
            }
        }
    }
}
