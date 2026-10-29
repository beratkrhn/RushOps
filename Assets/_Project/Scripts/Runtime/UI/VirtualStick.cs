using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace RushOps.UI
{
    /// <summary>
    /// Virtual stick as an on-screen control of the Input System (F-STE-05): writes its value to a stick of the
    /// virtual gamepad (e.g. <c>&lt;Gamepad&gt;/leftStick</c>), so the player reads it through the usual actions.
    /// Sits on the touch zone of its screen half; each stick follows only the first touch that took it, so both
    /// sticks work at the same time. The rules live in <see cref="VirtualStickMath"/>.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VirtualStick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private TouchControlsConfig _config;
        [SerializeField] private StickSide _side;
        [Tooltip("Base of the stick; centered on the stick's origin.")]
        [SerializeField] private RectTransform _base;
        [Tooltip("Knob of the stick; a child of the base.")]
        [SerializeField] private RectTransform _knob;
        [Tooltip("Fades the stick while no thumb touches it.")]
        [SerializeField] private CanvasGroup _visual;

        [InputControl(layout = "Vector2")]
        [SerializeField] private string _controlPath;

        private RectTransform _zone;
        private bool _pressed;
        private int _pointerId;
        private Vector2 _origin;

        /// <summary>Raised when the home position or sizes change, e.g. after the safe area changed.</summary>
        public event Action LayoutChanged;

        /// <summary>Config with mode and sizes.</summary>
        public TouchControlsConfig Config => _config;

        /// <summary>Resting position of the stick in zone space (from the zone's bottom-left corner).</summary>
        public Vector2 Home { get; private set; }

        /// <summary>True while a touch holds the stick.</summary>
        public bool IsPressed => _pressed;

        /// <summary>Current value, length 0..1.</summary>
        public Vector2 Value { get; private set; }

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        /// <summary>Re-applies mode and sizes from the config, e.g. after the settings changed.</summary>
        public void Refresh()
        {
            ApplyLayout();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pressed || _config == null)
            {
                return;
            }

            Vector2 press = ToZone(eventData);
            float radius = _config.StickRadius;
            if (!VirtualStickMath.AcceptsPress(_config.Mode, Home, press, radius * _config.FixedGrabScale))
            {
                return;
            }

            _pressed = true;
            _pointerId = eventData.pointerId;
            _origin = VirtualStickMath.Origin(_config.Mode, Home, press, ZoneSize, radius);
            SetVisualAlpha(1f);
            UpdateValue(press);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_pressed && eventData.pointerId == _pointerId)
            {
                UpdateValue(ToZone(eventData));
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pressed && eventData.pointerId == _pointerId)
            {
                Release();
            }
        }

        private void Awake()
        {
            _zone = (RectTransform)transform;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ApplyLayout();
        }

        protected override void OnDisable()
        {
            if (_pressed)
            {
                Release();
            }

            base.OnDisable();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (isActiveAndEnabled)
            {
                ApplyLayout();
            }
        }

        private Vector2 ZoneSize => _zone.rect.size;

        private void ApplyLayout()
        {
            if (_config == null || _zone == null)
            {
                return;
            }

            float radius = _config.StickRadius;
            Home = VirtualStickMath.HomePosition(ZoneSize, _side, radius, _config.EdgeMargin);
            if (_base != null)
            {
                _base.sizeDelta = new Vector2(_config.StickSize, _config.StickSize);
            }

            if (_knob != null)
            {
                _knob.sizeDelta = new Vector2(_config.KnobSize, _config.KnobSize);
            }

            if (!_pressed)
            {
                ShowAtRest();
            }

            LayoutChanged?.Invoke();
        }

        private void UpdateValue(Vector2 pointer)
        {
            float radius = _config.StickRadius;
            Value = VirtualStickMath.Value(_origin, pointer, radius);
            SendValueToControl(Value);
            PlaceVisual(_origin, Value * radius);
        }

        private void Release()
        {
            _pressed = false;
            Value = Vector2.zero;
            SendValueToControl(Vector2.zero);
            ShowAtRest();
        }

        private void ShowAtRest()
        {
            PlaceVisual(Home, Vector2.zero);
            SetVisualAlpha(_config.IdleAlpha);
        }

        private void PlaceVisual(Vector2 origin, Vector2 knobOffset)
        {
            // Base and knob are anchored to the zone's bottom-left corner and the base's center respectively.
            if (_base != null)
            {
                _base.anchoredPosition = origin;
            }

            if (_knob != null)
            {
                _knob.anchoredPosition = knobOffset;
            }
        }

        private void SetVisualAlpha(float alpha)
        {
            if (_visual != null)
            {
                _visual.alpha = alpha;
            }
        }

        private Vector2 ToZone(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _zone, eventData.position, eventData.pressEventCamera, out Vector2 local);
            return local - _zone.rect.min;
        }
    }
}
