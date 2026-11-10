using System;
using RushOps.Rush;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Dash with i-frames (F-STE-03, F-STE-04): moves the player a configured distance in the move direction,
    /// or in the facing direction while standing, through <see cref="PlayerMovement"/>, so walls stop it.
    /// Values come from <see cref="PlayerConfig"/>; the timing rules live in <see cref="DashState"/>.
    /// The trigger comes from an adapter such as <see cref="PlayerDashInput"/>; this component does not read devices.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerAim))]
    // Runs before PlayerMovement, so the override is cleared before the movement applies the velocity.
    [DefaultExecutionOrder(-10)]
    public class PlayerDash : MonoBehaviour
    {
        private readonly DashState _state = new DashState();
        private PlayerMovement _movement;
        private PlayerAim _aim;
        private bool _overriding;
        private bool _cooldownFree;

        /// <summary>Raised when a dash starts, with its direction.</summary>
        public event Action<Vector2> Dashed;

        /// <summary>True while the dash moves the player.</summary>
        public bool IsDashing => _state.IsDashing;

        /// <summary>True during the i-frames; damage sources must not hurt the player.</summary>
        public bool IsInvulnerable => _state.IsInvulnerable;

        /// <summary>True while at least one source locks the dash, see <see cref="Lock"/>.</summary>
        public bool IsLocked => _state.IsLocked;

        /// <summary>True if <see cref="TryDash"/> would start a dash now.</summary>
        public bool CanDash => _state.CanDash && _movement.Config != null;

        /// <summary>Seconds until the dash is available again; 0 when ready.</summary>
        public float CooldownRemaining => _state.CooldownRemaining;

        /// <summary>Cooldown progress for the dash button: 0 right after a dash, 1 when ready.</summary>
        public float CooldownProgress => _state.CooldownProgress;

        /// <summary>
        /// True while the dash has no cooldown, e.g. during the rush (F-RSH-04); setting it ends a running cooldown. The
        /// i-frames stay as they are.
        /// </summary>
        public bool CooldownFree
        {
            get => _cooldownFree;
            set
            {
                _cooldownFree = value;
                if (value)
                {
                    _state.ClearCooldown();
                }
            }
        }

        /// <summary>Starts a dash if possible.</summary>
        /// <returns>True if a dash started.</returns>
        public bool TryDash()
        {
            PlayerConfig config = _movement.Config;
            if (config == null || !_state.CanDash)
            {
                return false;
            }

            Vector2 direction = PlayerDashMath.Direction(_movement.MoveInput, _aim.FacingDirection);
            if (direction == Vector2.zero
                || !_state.TryStart(config.DashDuration, config.DashInvulnerability,
                    RushRules.DashCooldown(_cooldownFree, config.DashCooldown)))
            {
                return false;
            }

            float speed = PlayerDashMath.Speed(config.DashDistance, config.DashDuration);
            _movement.SetVelocityOverride(direction * speed);
            _overriding = true;
            Dashed?.Invoke(direction);
            return true;
        }

        /// <summary>
        /// Forbids dashing until <paramref name="source"/> calls <see cref="Unlock"/>, e.g. while carrying
        /// an energy cell (F-RAE-07). Several sources can lock at once; a running dash continues.
        /// </summary>
        public void Lock(object source)
        {
            _state.Lock(source);
        }

        /// <summary>Removes the lock of <paramref name="source"/>.</summary>
        public void Unlock(object source)
        {
            _state.Unlock(source);
        }

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _aim = GetComponent<PlayerAim>();
        }

        private void FixedUpdate()
        {
            // The step in which the dash time runs out still moves at dash speed,
            // so the dash covers its full distance; control returns on the following step.
            if (_overriding && !_state.IsDashing)
            {
                EndOverride();
            }

            _state.Tick(Time.fixedDeltaTime);
        }

        private void OnDisable()
        {
            if (_overriding)
            {
                EndOverride();
            }

            _state.Reset();
        }

        private void EndOverride()
        {
            _movement.ClearVelocityOverride();
            _overriding = false;
        }
    }
}
