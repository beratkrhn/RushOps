using UnityEngine;
using UnityEngine.InputSystem;

namespace RushOps.Player
{
    /// <summary>
    /// Feeds the aim actions into <see cref="PlayerAim"/>: <c>Player/Aim</c> (gamepad right stick,
    /// later the on-screen stick) and, for testing in the editor, the mouse: while <c>Player/AimPress</c>
    /// is held the player aims at full deflection towards the cursor (<c>Player/AimPoint</c>).
    /// </summary>
    [RequireComponent(typeof(PlayerAim))]
    public class PlayerAimInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference _aimAction;
        [SerializeField] private InputActionReference _pointAction;
        [SerializeField] private InputActionReference _pressAction;

        private PlayerAim _aim;
        private PlayerWeapon _weapon;
        private Camera _camera;

        private void Awake()
        {
            _aim = GetComponent<PlayerAim>();
            _weapon = GetComponent<PlayerWeapon>();
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void OnEnable()
        {
            _aimAction?.action?.Enable();
            _pointAction?.action?.Enable();
            _pressAction?.action?.Enable();
        }

        private void Update()
        {
            Vector2 aim = Read(_aimAction);
            if (aim == Vector2.zero && IsPointerPressed())
            {
                aim = PointerAim();
            }

            _aim.AimInput = aim;
        }

        private bool IsPointerPressed()
        {
            InputAction press = _pressAction != null ? _pressAction.action : null;
            return press != null && press.IsPressed() && _pointAction != null && _pointAction.action != null;
        }

        private Vector2 PointerAim()
        {
            if (_camera == null)
            {
                return Vector2.zero;
            }

            Vector2 screen = _pointAction.action.ReadValue<Vector2>();
            Vector2 world = _camera.ScreenToWorldPoint(screen);
            Vector2 origin = _weapon != null ? _weapon.MuzzlePosition(Vector2.zero) : (Vector2)transform.position;
            return PlayerAimMath.PointerAim(origin, world);
        }

        private static Vector2 Read(InputActionReference reference)
        {
            InputAction action = reference != null ? reference.action : null;
            return action != null ? action.ReadValue<Vector2>() : Vector2.zero;
        }
    }
}
