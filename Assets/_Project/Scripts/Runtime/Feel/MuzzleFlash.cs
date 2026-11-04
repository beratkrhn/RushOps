using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RushOps.Feel
{
    /// <summary>
    /// Muzzle flash of the player's gun (issue #107): on every shot the sprite appears at the muzzle, turned along the
    /// shot, together with a short accent light, and hides again after <see cref="EffectsConfig.MuzzleFlashDuration"/>.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MuzzleFlash : MonoBehaviour
    {
        [SerializeField] private EffectsConfig _config;
        [SerializeField] private Light2D _light;

        private SpriteRenderer _renderer;
        private float _left;

        /// <summary>True while the flash shows.</summary>
        public bool IsShowing => _left > 0f;

        /// <summary>Shows the flash at <paramref name="position"/>, pointing along <paramref name="direction"/>.</summary>
        public void Show(Vector2 position, Vector2 direction)
        {
            float duration = _config != null ? _config.MuzzleFlashDuration : 0f;
            if (duration <= 0f)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
            _left = duration;
            _renderer.enabled = true;
            if (_light != null)
            {
                _light.intensity = _config.MuzzleLightIntensity;
                _light.enabled = _config.MuzzleLightIntensity > 0f;
            }
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            Hide();
        }

        private void Update()
        {
            if (_left <= 0f)
            {
                return;
            }

            _left -= Time.deltaTime;
            if (_left <= 0f)
            {
                Hide();
            }
        }

        private void Hide()
        {
            _left = 0f;
            _renderer.enabled = false;
            if (_light != null)
            {
                _light.enabled = false;
            }
        }
    }
}
