using UnityEngine;

namespace RushOps.UI
{
    /// <summary>
    /// Values of the on-screen touch controls (F-STE-05, F-HUD-07). Sizes are in canvas units of the
    /// 360x640 reference resolution. The settings menu (v0.5.0) will change mode and size at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "TouchControlsConfig", menuName = "RushOps/Touch Controls Config")]
    public class TouchControlsConfig : ScriptableObject
    {
        [Tooltip("Floating: the stick appears where the thumb touches its screen half. Fixed: it stays in the bottom corner.")]
        [SerializeField] private StickMode _mode = StickMode.Floating;

        [Tooltip("Diameter of a stick's base in reference units; the knob travels up to half of it.")]
        [SerializeField, Min(16f)] private float _stickSize = 96f;

        [Tooltip("Knob diameter relative to the stick size.")]
        [SerializeField, Range(0.1f, 1f)] private float _knobScale = 0.5f;

        [Tooltip("Distance of the sticks' home positions from the safe area edges in reference units.")]
        [SerializeField, Min(0f)] private float _edgeMargin = 24f;

        [Tooltip("Fixed mode: a touch grabs the stick within this multiple of its radius around the home position.")]
        [SerializeField, Min(1f)] private float _fixedGrabScale = 1.5f;

        [Tooltip("Opacity of a stick while no thumb touches it.")]
        [SerializeField, Range(0f, 1f)] private float _idleAlpha = 0.5f;

        [Tooltip("Diameter of the dash button in reference units.")]
        [SerializeField, Min(16f)] private float _dashButtonSize = 72f;

        [Tooltip("Gap between the right stick's base and the dash button above it in reference units.")]
        [SerializeField, Min(0f)] private float _dashButtonGap = 16f;

        /// <summary>Floating or fixed sticks.</summary>
        public StickMode Mode => _mode;

        /// <summary>Diameter of a stick's base in reference units.</summary>
        public float StickSize => _stickSize;

        /// <summary>Radius the knob can travel from the stick's origin; full deflection at this distance.</summary>
        public float StickRadius => _stickSize * 0.5f;

        /// <summary>Knob diameter in reference units.</summary>
        public float KnobSize => _stickSize * _knobScale;

        /// <summary>Distance of the sticks' home positions from the safe area edges.</summary>
        public float EdgeMargin => _edgeMargin;

        /// <summary>Fixed mode: grab radius around the home position as a multiple of <see cref="StickRadius"/>.</summary>
        public float FixedGrabScale => _fixedGrabScale;

        /// <summary>Opacity of an untouched stick.</summary>
        public float IdleAlpha => _idleAlpha;

        /// <summary>Diameter of the dash button in reference units.</summary>
        public float DashButtonSize => _dashButtonSize;

        /// <summary>Gap between the right stick's base and the dash button.</summary>
        public float DashButtonGap => _dashButtonGap;
    }
}
