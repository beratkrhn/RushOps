using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Moves the player's <see cref="Rigidbody2D"/> from a move input, so walls block it through physics.
    /// The input comes from an adapter such as <see cref="PlayerMoveInput"/>; this component does not read devices.
    /// The sprite's facing is handled by <see cref="PlayerAim"/>. Abilities such as <see cref="PlayerDash"/>
    /// can take over the velocity for a while through <see cref="SetVelocityOverride"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;

        private Rigidbody2D _body;
        private Vector2 _moveInput;
        private Vector2 _velocityOverride;
        private bool _hasVelocityOverride;
        private float _speedMultiplier = 1f;
        private float _upgradeMultiplier = 1f;
        private float _loadMultiplier = 1f;

        /// <summary>Stick direction and deflection; length 1 is full speed, longer values are clamped.</summary>
        public Vector2 MoveInput
        {
            get => _moveInput;
            set => _moveInput = value;
        }

        /// <summary>Factor on the walking speed, e.g. 1.5 during a speed boost (F-PRK-04); 1 by default, never negative.</summary>
        public float SpeedMultiplier
        {
            get => _speedMultiplier;
            set => _speedMultiplier = value > 0f ? value : 0f;
        }

        /// <summary>
        /// Factor on the walking speed from what the player carries, e.g. 0.8 with an energy cell (F-RAE-07, issue #33);
        /// 1 by default, never negative. Independent of <see cref="SpeedMultiplier"/>, so perks and carrying combine.
        /// </summary>
        public float LoadMultiplier
        {
            get => _loadMultiplier;
            set => _loadMultiplier = value > 0f ? value : 0f;
        }

        /// <summary>
        /// Factor on the walking speed from the upgrades of the endless run, e.g. 1.1 with quick feet (F-END-12); 1 by
        /// default, never negative. Multiplies with <see cref="SpeedMultiplier"/> and <see cref="LoadMultiplier"/>.
        /// </summary>
        public float UpgradeMultiplier
        {
            get => _upgradeMultiplier;
            set => _upgradeMultiplier = value > 0f ? value : 0f;
        }

        /// <summary>
        /// Current walking speed at full deflection, including <see cref="SpeedMultiplier"/>, <see cref="LoadMultiplier"/>
        /// and <see cref="UpgradeMultiplier"/>.
        /// </summary>
        public float MoveSpeed =>
            _config != null ? _config.MoveSpeed * _speedMultiplier * _loadMultiplier * _upgradeMultiplier : 0f;

        /// <summary>Current velocity of the player in units per second.</summary>
        public Vector2 Velocity => _body.linearVelocity;

        /// <summary>Replaces the config, e.g. for tests or temporary modifiers.</summary>
        public PlayerConfig Config
        {
            get => _config;
            set => _config = value;
        }

        /// <summary>
        /// Moves the player at exactly <paramref name="velocity"/> from the next physics step on,
        /// ignoring the move input, until <see cref="ClearVelocityOverride"/>.
        /// </summary>
        public void SetVelocityOverride(Vector2 velocity)
        {
            _velocityOverride = velocity;
            _hasVelocityOverride = true;
        }

        /// <summary>Hands the velocity back to the move input and continues at the regular walking speed.</summary>
        public void ClearVelocityOverride()
        {
            if (!_hasVelocityOverride)
            {
                return;
            }

            _hasVelocityOverride = false;
            if (_body != null && _config != null)
            {
                _body.linearVelocity = PlayerMovementMath.TargetVelocity(_moveInput, MoveSpeed);
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (_hasVelocityOverride)
            {
                _body.linearVelocity = _velocityOverride;
                return;
            }

            if (_config == null)
            {
                return;
            }

            Vector2 target = PlayerMovementMath.TargetVelocity(_moveInput, MoveSpeed);
            _body.linearVelocity = PlayerMovementMath.StepVelocity(
                _body.linearVelocity, target, _config.Acceleration, _config.Deceleration, Time.fixedDeltaTime);
        }

        private void OnDisable()
        {
            _moveInput = Vector2.zero;
            _hasVelocityOverride = false;
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
            }
        }
    }
}
