using RushOps.Combat;
using RushOps.Player;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Small swap button beside the dash button (issue #175, decision W10): hidden until the player carries two
    /// weapons, then it shows the icon of the other weapon; a tap makes that one active.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class WeaponSwapButton : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon _weapon;
        [Tooltip("Stick the dash button sits above; provides the layout.")]
        [SerializeField] private VirtualStick _stick;
        [Tooltip("Background and hit area of the button.")]
        [SerializeField] private Image _background;
        [Tooltip("Icon of the weapon a tap switches to.")]
        [SerializeField] private Image _icon;

        private RectTransform _rect;
        private WeaponSlots _slots;

        /// <summary>True while the button shows.</summary>
        public bool IsShown => _background != null && _background.enabled;

        /// <summary>Icon shown on the button.</summary>
        public Sprite IconShown => _icon != null ? _icon.sprite : null;

        /// <summary>Makes the other weapon active.</summary>
        public void Swap()
        {
            if (_weapon != null)
            {
                _weapon.Slots.Swap();
            }
        }

        private void Awake()
        {
            _rect = (RectTransform)transform;
        }

        private void OnEnable()
        {
            if (_stick != null)
            {
                _stick.LayoutChanged += ApplyLayout;
            }

            if (_weapon != null)
            {
                _slots = _weapon.Slots;
                _slots.Changed += Refresh;
            }

            ApplyLayout();
            Refresh();
        }

        private void OnDisable()
        {
            if (_stick != null)
            {
                _stick.LayoutChanged -= ApplyLayout;
            }

            if (_slots != null)
            {
                _slots.Changed -= Refresh;
                _slots = null;
            }
        }

        private void Refresh()
        {
            WeaponDefinition other = _slots != null ? _slots.Inactive : null;
            bool show = other != null;
            if (_background != null)
            {
                _background.enabled = show;
            }

            if (_icon != null)
            {
                _icon.enabled = show && other.Icon != null;
                _icon.sprite = show ? other.Icon : null;
            }
        }

        private void ApplyLayout()
        {
            TouchControlsConfig config = _stick != null ? _stick.Config : null;
            if (config == null || _rect == null)
            {
                return;
            }

            Vector2 dash = VirtualStickMath.DashButtonPosition(
                _stick.Home, config.StickRadius, config.DashButtonSize, config.DashButtonGap);
            _rect.sizeDelta = new Vector2(config.SwapButtonSize, config.SwapButtonSize);
            _rect.anchoredPosition = VirtualStickMath.SwapButtonPosition(
                dash, config.DashButtonSize, config.SwapButtonSize, config.SwapButtonGap);
        }
    }
}
