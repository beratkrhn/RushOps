using RushOps.Player;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Places the on-screen dash button above the right stick (F-STE-03) and shows the remaining cooldown
    /// (F-STE-04) as a radial overlay that shrinks until the dash is ready again.
    /// The button press itself goes through an <see cref="UnityEngine.InputSystem.OnScreen.OnScreenButton"/>.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DashButtonView : MonoBehaviour
    {
        [SerializeField] private PlayerDash _dash;
        [Tooltip("Stick the button sits above; also provides the config.")]
        [SerializeField] private VirtualStick _stick;
        [Tooltip("Radially filled image covering the button during the cooldown.")]
        [SerializeField] private Image _cooldownOverlay;

        private RectTransform _rect;
        private float _shownFill = -1f;

        /// <summary>Dash whose cooldown the button shows.</summary>
        public PlayerDash Dash
        {
            get => _dash;
            set => _dash = value;
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

            ApplyLayout();
        }

        private void OnDisable()
        {
            if (_stick != null)
            {
                _stick.LayoutChanged -= ApplyLayout;
            }
        }

        private void Update()
        {
            float fill = _dash != null ? 1f - _dash.CooldownProgress : 0f;
            if (fill == _shownFill || _cooldownOverlay == null)
            {
                return;
            }

            _shownFill = fill;
            _cooldownOverlay.fillAmount = fill;
            _cooldownOverlay.enabled = fill > 0f;
        }

        private void ApplyLayout()
        {
            TouchControlsConfig config = _stick != null ? _stick.Config : null;
            if (config == null || _rect == null)
            {
                return;
            }

            float size = config.DashButtonSize;
            _rect.sizeDelta = new Vector2(size, size);
            _rect.anchoredPosition = VirtualStickMath.DashButtonPosition(_stick.Home, config.StickRadius, size, config.DashButtonGap);
        }
    }
}
