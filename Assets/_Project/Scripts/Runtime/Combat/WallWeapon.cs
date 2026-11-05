using System;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// A weapon hanging on a wall (issue #175, decisions W6, W8, W9): its silhouette stays on the wall; standing on the
    /// spot in front of it fills a ring, and once it is full the weapon goes into the player's second slot and becomes
    /// active, even if it is not unlocked yet. Free, and with the weapon already in hand nothing happens.
    /// Rules in <see cref="WallPickupTimer"/>.
    /// </summary>
    public class WallWeapon : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition _weapon;
        [SerializeField] private WallWeaponConfig _config;
        [SerializeField] private PlayerWeapon _player;
        [Tooltip("Spot on the floor in front of the weapon, relative to this object.")]
        [SerializeField] private Vector2 _pickupOffset = new Vector2(0f, -1f);
        [Tooltip("Ring on the floor that fills while the player stands in front.")]
        [SerializeField] private SpriteRenderer _ring;
        [Tooltip("Frames of the ring from almost empty to full.")]
        [SerializeField] private Sprite[] _ringFrames = Array.Empty<Sprite>();

        private WallPickupTimer _timer;

        /// <summary>Raised when the player picked the weapon up.</summary>
        public event Action<WallWeapon> PickedUp;

        /// <summary>The weapon hanging here.</summary>
        public WeaponDefinition Weapon => _weapon;

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
            bool canPick = _player.isActiveAndEnabled && !_player.Slots.Holds(_weapon);
            if (Timer.Tick(Time.deltaTime, inFront, canPick) && _player.Slots.PickUp(_weapon))
            {
                PickedUp?.Invoke(this);
            }

            ShowRing();
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
