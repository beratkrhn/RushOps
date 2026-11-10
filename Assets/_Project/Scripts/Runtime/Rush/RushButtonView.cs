using RushOps.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Rush
{
    /// <summary>
    /// The rush button at the top left of the HUD (F-HUD-09, issue #271), meter and button in one: a ring fills with
    /// the meter; when full the button lights up, pulses and chimes once; a tap triggers the rush; during the rush a
    /// yellow ring runs down with the time left. Thin adapter on <see cref="PlayerRush"/>; pulse and sound come from the
    /// <see cref="RushConfig"/>. It sits on the HUD canvas above the touch controls, so a tap on it never starts a stick.
    /// </summary>
    public class RushButtonView : MonoBehaviour
    {
        [SerializeField] private PlayerRush _rush;
        [Tooltip("Disc behind the ring; its sprite shows whether the button is full.")]
        [SerializeField] private Image _background;
        [Tooltip("Ring filled radially with the meter or the time left of the rush.")]
        [SerializeField] private Image _ring;
        [Tooltip("Scaled while the full button pulses.")]
        [SerializeField] private RectTransform _visual;
        [SerializeField] private Sprite _emptySprite;
        [SerializeField] private Sprite _fullSprite;
        [SerializeField] private Sprite _fillRing;
        [SerializeField] private Sprite _activeRing;
        [SerializeField] private SfxPlayer _sfx;

        private float _pulseStart;
        private bool _wasFull;

        /// <summary>True while the button shows a full meter.</summary>
        public bool ShowsFull => _background != null && _background.sprite == _fullSprite;

        /// <summary>True while the ring shows the time left of a running rush.</summary>
        public bool ShowsRush => _ring != null && _ring.sprite == _activeRing;

        /// <summary>Fill of the ring, 0 to 1.</summary>
        public float RingFill => _ring != null ? _ring.fillAmount : 0f;

        /// <summary>Triggers the rush if the meter is full; called by the button.</summary>
        public void Tap()
        {
            if (_rush != null)
            {
                _rush.TryTrigger();
            }
        }

        private void OnEnable()
        {
            if (_rush != null)
            {
                _rush.Meter.Filled += OnFilled;
            }
        }

        private void OnDisable()
        {
            if (_rush != null)
            {
                _rush.Meter.Filled -= OnFilled;
            }
        }

        private void Update()
        {
            if (_rush == null)
            {
                return;
            }

            RushMeter meter = _rush.Meter;
            bool full = meter.IsFull;
            if (full && !_wasFull)
            {
                _pulseStart = Time.unscaledTime;
            }

            _wasFull = full;
            SetSprite(_background, full ? _fullSprite : _emptySprite);
            SetSprite(_ring, meter.IsActive ? _activeRing : _fillRing);
            if (_ring != null)
            {
                _ring.fillAmount = meter.IsActive ? meter.RemainingProgress : meter.FillProgress;
            }

            if (_visual != null)
            {
                RushConfig config = _rush.Config;
                float scale = full && config != null
                    ? RushRules.PulseScale(Time.unscaledTime - _pulseStart, config.PulsePeriod, config.PulseScale)
                    : 1f;
                _visual.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void OnFilled()
        {
            RushConfig config = _rush.Config;
            if (_sfx != null && config != null && config.FullClip != null)
            {
                _sfx.Play(config.FullClip, config.FullVolume, 0f);
            }
        }

        private static void SetSprite(Image image, Sprite sprite)
        {
            if (image != null && image.sprite != sprite)
            {
                image.sprite = sprite;
            }
        }
    }
}
