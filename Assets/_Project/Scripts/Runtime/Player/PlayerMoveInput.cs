using UnityEngine;
using UnityEngine.InputSystem;

namespace RushOps.Player
{
    /// <summary>
    /// Feeds the Input System action <c>Player/Move</c> (gamepad stick, WASD, arrow keys) into <see cref="PlayerMovement"/>.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerMoveInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference _moveAction;

        private PlayerMovement _movement;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
        }

        private void OnEnable()
        {
            _moveAction?.action?.Enable();
        }

        private void Update()
        {
            InputAction action = _moveAction != null ? _moveAction.action : null;
            if (action != null)
            {
                _movement.MoveInput = action.ReadValue<Vector2>();
            }
        }
    }
}
