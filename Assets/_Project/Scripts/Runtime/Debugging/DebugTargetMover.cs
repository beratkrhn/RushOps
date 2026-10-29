using RushOps.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RushOps.Debugging
{
    /// <summary>
    /// Temporary stand-in for player movement so camera follow can be tested in the arena.
    /// Moves with WASD, arrow keys or the left gamepad stick and faces the sprite in the move direction.
    /// Replaced by real player movement (issue #4).
    /// </summary>
    public class DebugTargetMover : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _speed = 5f;
        [SerializeField] private DirectionalSpriteAnimator _view;

        private void Update()
        {
            Vector2 direction = ReadDirection();
            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            transform.position += (Vector3)(direction * (_speed * Time.deltaTime));
            if (_view != null)
            {
                _view.SetFacing(direction);
            }
        }

        private static Vector2 ReadDirection()
        {
            Vector2 direction = Vector2.zero;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) direction.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) direction.x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) direction.y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) direction.y += 1f;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                direction += gamepad.leftStick.ReadValue();
            }

            return direction;
        }
    }
}
