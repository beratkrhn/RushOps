using RushOps.Characters;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Moves the player's <see cref="Rigidbody2D"/> from a move input, so walls block it through physics.
    /// The input comes from an adapter such as <see cref="PlayerMoveInput"/>; this component does not read devices.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private DirectionalSpriteAnimator _view;

        private Rigidbody2D _body;
        private Vector2 _moveInput;

        /// <summary>Stick direction and deflection; length 1 is full speed, longer values are clamped.</summary>
        public Vector2 MoveInput
        {
            get => _moveInput;
            set => _moveInput = value;
        }

        /// <summary>Current velocity of the player in units per second.</summary>
        public Vector2 Velocity => _body.linearVelocity;

        /// <summary>Replaces the config, e.g. for tests or temporary modifiers.</summary>
        public PlayerConfig Config
        {
            get => _config;
            set => _config = value;
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_view != null)
            {
                _view.SetFacing(_moveInput);
            }
        }

        private void FixedUpdate()
        {
            if (_config == null)
            {
                return;
            }

            Vector2 target = PlayerMovementMath.TargetVelocity(_moveInput, _config.MoveSpeed);
            _body.linearVelocity = PlayerMovementMath.StepVelocity(
                _body.linearVelocity, target, _config.Acceleration, _config.Deceleration, Time.fixedDeltaTime);
        }

        private void OnDisable()
        {
            _moveInput = Vector2.zero;
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
            }
        }
    }
}
