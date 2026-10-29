using UnityEngine;
using UnityEngine.InputSystem;

namespace RushOps.Player
{
    /// <summary>
    /// Feeds the Input System action <c>Player/Dash</c> (space, right mouse button, gamepad south button
    /// or right shoulder, later the on-screen dash button) into <see cref="PlayerDash"/>.
    /// </summary>
    [RequireComponent(typeof(PlayerDash))]
    public class PlayerDashInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference _dashAction;

        private PlayerDash _dash;

        private void Awake()
        {
            _dash = GetComponent<PlayerDash>();
        }

        private void OnEnable()
        {
            _dashAction?.action?.Enable();
        }

        private void Update()
        {
            InputAction action = _dashAction != null ? _dashAction.action : null;
            // No dash while the game time stands still, e.g. in the pause menu.
            if (action != null && Time.timeScale > 0f && action.WasPressedThisFrame())
            {
                _dash.TryDash();
            }
        }
    }
}
