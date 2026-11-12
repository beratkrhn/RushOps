using RushOps.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RushOps.Arcade
{
    /// <summary>
    /// The big yellow dash button of the cabinet (F-ARC-04, F-STE-03, F-STE-04): pressed in while a thumb holds it,
    /// dimmed while the dash cools down or is locked. The press itself goes through the
    /// <see cref="UnityEngine.InputSystem.OnScreen.OnScreenButton"/> on the same object, as in the normal mode.
    /// </summary>
    public class ArcadeDashButtonView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private ArcadeConfig _config;
        [SerializeField] private PlayerDash _dash;
        [SerializeField] private Image _image;

        private int _pointerId = int.MinValue;
        private bool _shownHeld;
        private bool _shownReady;
        private bool _shown;

        /// <summary>True while a thumb holds the button.</summary>
        public bool IsHeld { get; private set; }

        /// <summary>True while the button shows the dash as ready (not dimmed).</summary>
        public bool ShowsReady => _shownReady;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsHeld)
            {
                IsHeld = true;
                _pointerId = eventData.pointerId;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (IsHeld && eventData.pointerId == _pointerId)
            {
                IsHeld = false;
            }
        }

        private void OnDisable()
        {
            IsHeld = false;
            _shown = false;
        }

        private void Update()
        {
            if (_config == null || _image == null)
            {
                return;
            }

            bool ready = _dash == null || (!_dash.IsLocked && _dash.CooldownProgress >= 1f);
            if (_shown && ready == _shownReady && IsHeld == _shownHeld)
            {
                return;
            }

            _shown = true;
            _shownReady = ready;
            _shownHeld = IsHeld;
            _image.sprite = IsHeld ? _config.DashPressedSprite : _config.DashSprite;
            _image.color = ready ? Color.white : _config.DashCooldownTint;
        }
    }
}
