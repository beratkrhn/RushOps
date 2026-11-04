using System;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Target station of the object transport (F-RAE-06, issue #33): takes the carried cell when the player touches it
    /// while the <see cref="TransportPuzzle"/> accepts cells, and shows running once it has all it needs.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Generator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _offSprite;
        [SerializeField] private Sprite _onSprite;

        /// <summary>Raised when the player touches the generator carrying a cell.</summary>
        public event Action<PlayerCarrier> Touched;

        /// <summary>True once the generator shows running.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>Shows the generator running or not.</summary>
        public void SetRunning(bool running)
        {
            IsRunning = running;
            if (_renderer != null)
            {
                _renderer.sprite = running ? _onSprite : _offSprite;
            }
        }

        // Stay as well, so a player who walked up before picking a cell elsewhere still hands in on return.
        private void OnTriggerEnter2D(Collider2D other)
        {
            Check(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Check(other);
        }

        private void Check(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent(out PlayerCarrier carrier) && carrier.IsCarrying)
            {
                Touched?.Invoke(carrier);
            }
        }
    }
}
