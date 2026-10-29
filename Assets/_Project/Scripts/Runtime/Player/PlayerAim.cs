using RushOps.Characters;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Holds the player's aim input and faces the character sprite towards the aim direction,
    /// or towards the move direction while not aiming (ADR 0006).
    /// The input comes from an adapter such as <see cref="PlayerAimInput"/>; this component does not read devices.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField] private DirectionalSpriteAnimator _view;

        private PlayerMovement _movement;
        private Vector2 _aimInput;
        private Vector2 _facing = Vector2.down;

        /// <summary>Aim stick direction and deflection (0..1). Zero means not aiming.</summary>
        public Vector2 AimInput
        {
            get => _aimInput;
            set => _aimInput = value;
        }

        /// <summary>True while the aim deflection is beyond the dead zone from <see cref="PlayerConfig"/>.</summary>
        public bool IsAiming => PlayerAimMath.IsAiming(_aimInput, Deadzone);

        /// <summary>Unit vector of the aim direction; zero while not aiming.</summary>
        public Vector2 AimDirection => IsAiming ? _aimInput.normalized : Vector2.zero;

        /// <summary>
        /// Unit vector the player faces: the aim direction while aiming, otherwise the last move direction.
        /// Unlike the sprite it is not snapped to 8 directions.
        /// </summary>
        public Vector2 FacingDirection => _facing;

        private float Deadzone => _movement.Config != null ? _movement.Config.AimDeadzone : 0f;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            if (_view != null)
            {
                _facing = Direction8Math.ToVector(_view.Direction);
            }
        }

        private void Update()
        {
            Vector2 facing = PlayerAimMath.FacingVector(_movement.MoveInput, _aimInput, Deadzone);
            // normalized is zero for (near) zero vectors, which keep the last facing.
            Vector2 unit = facing.normalized;
            if (unit != Vector2.zero)
            {
                _facing = unit;
            }

            if (_view != null)
            {
                _view.SetFacing(facing);
            }
        }

        private void OnDisable()
        {
            _aimInput = Vector2.zero;
        }
    }
}
