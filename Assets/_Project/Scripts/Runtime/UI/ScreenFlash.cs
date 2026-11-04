using RushOps.Feel;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// A full-screen color that lights up and fades out, e.g. the white flash of the nuke (issue #27). Runs in real
    /// time, so hitstops do not hold it.
    /// </summary>
    public class ScreenFlash : MonoBehaviour
    {
        [Tooltip("Covers the whole screen; its alpha is driven by the flash.")]
        [SerializeField] private Image _image;

        private float _elapsed = -1f;
        private float _duration;
        private float _alpha;

        /// <summary>Current opacity of the flash, 0 when off.</summary>
        public float CurrentAlpha => _image != null && _image.enabled ? _image.color.a : 0f;

        /// <summary>Lights up at <paramref name="alpha"/> and fades out over <paramref name="duration"/> seconds.</summary>
        public void Play(float alpha, float duration)
        {
            if (_image == null || alpha <= 0f || duration <= 0f)
            {
                return;
            }

            _alpha = alpha;
            _duration = duration;
            _elapsed = 0f;
            Apply(alpha);
        }

        private void Awake()
        {
            if (_image != null)
            {
                _image.raycastTarget = false;
                _image.enabled = false;
            }
        }

        private void Update()
        {
            if (_elapsed < 0f)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            float alpha = FeelMath.FlashFade(_elapsed, _duration, _alpha);
            if (alpha <= 0f)
            {
                _elapsed = -1f;
            }

            Apply(alpha);
        }

        private void Apply(float alpha)
        {
            Color color = _image.color;
            color.a = alpha;
            _image.color = color;
            _image.enabled = alpha > 0f;
        }
    }
}
