using System;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// A pressure plate of the timed switches (F-RAE-03, issue #32): reports when the player steps on it (enemies and
    /// shots do not count) and shows whether it is pressed. The <see cref="TimedSwitchPuzzle"/> decides what a press means.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FloorSwitch : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _offSprite;
        [SerializeField] private Sprite _onSprite;

        /// <summary>Raised when the player steps on the plate.</summary>
        public event Action<FloorSwitch> Stepped;

        /// <summary>True while the plate shows as pressed.</summary>
        public bool ShowsPressed { get; private set; }

        /// <summary>Shows the plate pressed or not.</summary>
        public void SetPressed(bool pressed)
        {
            ShowsPressed = pressed;
            if (_renderer != null)
            {
                _renderer.sprite = pressed ? _onSprite : _offSprite;
            }
        }

        /// <summary>Tints the plate, e.g. red while blinking after a reset.</summary>
        public void SetTint(Color color)
        {
            if (_renderer != null)
            {
                _renderer.color = color;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Rigidbody2D body = other.attachedRigidbody;
            if (body != null ? body.CompareTag(PlayerTag) : other.CompareTag(PlayerTag))
            {
                Stepped?.Invoke(this);
            }
        }
    }
}
