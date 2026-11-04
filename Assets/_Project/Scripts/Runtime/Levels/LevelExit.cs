using System;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// The level goal behind the last gate (F-LVL-05, issue #35): reports once when the player walks into it. With a
    /// zone set, it stays locked until that zone's waves are over and all enemies are dead (<see cref="ExitLock"/>);
    /// a player already standing on it then finishes at once. While locked it shows its locked sprite (issue #103),
    /// or the open sprite dimmed if none is set.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelExit : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [Tooltip("Zone whose waves must be over before the exit opens. Empty: always open.")]
        [SerializeField] private Zone _zone;
        [SerializeField] private WaveDirector _director;
        [SerializeField] private SpriteRenderer _renderer;
        [Tooltip("Sprite while open; empty keeps the renderer's sprite.")]
        [SerializeField] private Sprite _openSprite;

        [Tooltip("Sprite while locked; empty dims the open sprite with the locked tint instead.")]
        [SerializeField] private Sprite _lockedSprite;

        [Tooltip("Sprite colour while locked, used without a locked sprite.")]
        [SerializeField] private Color _lockedTint = new Color(1f, 1f, 1f, 0.35f);

        private bool _playerInside;

        /// <summary>Raised once when the player reaches the open exit.</summary>
        public event Action<LevelExit> Reached;

        /// <summary>Raised once when a locked exit opens.</summary>
        public event Action<LevelExit> Opened;

        /// <summary>True once the player reached the exit.</summary>
        public bool IsReached { get; private set; }

        /// <summary>True once the exit can be used; always true without a zone.</summary>
        public bool IsOpen { get; private set; }

        /// <summary>Zone that locks the exit, or null.</summary>
        public Zone Zone => _zone;

        private void Start()
        {
            if (_openSprite == null && _renderer != null)
            {
                _openSprite = _renderer.sprite;
            }

            IsOpen = CheckOpen();
            ApplyTint();
        }

        private void Update()
        {
            if (IsOpen)
            {
                return;
            }

            if (CheckOpen())
            {
                IsOpen = true;
                ApplyTint();
                Opened?.Invoke(this);
                if (_playerInside)
                {
                    Reach();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsPlayer(other))
            {
                return;
            }

            _playerInside = true;
            if (IsOpen)
            {
                Reach();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsPlayer(other))
            {
                _playerInside = false;
            }
        }

        private bool CheckOpen()
        {
            if (_zone == null || _director == null)
            {
                return true;
            }

            WaveSequence waves = _director.SequenceOf(_zone);
            return ExitLock.IsOpen(waves != null && _zone.Waves != null, waves != null ? waves.Phase : WavePhase.Finished,
                _director.AliveCount);
        }

        private void Reach()
        {
            if (IsReached)
            {
                return;
            }

            IsReached = true;
            Reached?.Invoke(this);
        }

        private void ApplyTint()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_lockedSprite != null)
            {
                _renderer.sprite = IsOpen ? _openSprite : _lockedSprite;
                _renderer.color = Color.white;
            }
            else
            {
                _renderer.color = IsOpen ? Color.white : _lockedTint;
            }
        }

        private static bool IsPlayer(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            return body != null ? body.CompareTag(PlayerTag) : other.CompareTag(PlayerTag);
        }
    }
}
