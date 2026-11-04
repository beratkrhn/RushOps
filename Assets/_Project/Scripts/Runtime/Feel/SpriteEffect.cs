using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RushOps.Feel
{
    /// <summary>
    /// Plays a short frame animation once at a fixed spot, e.g. a blast (issue #22), hit sparks or a death splatter
    /// (issue #107), then hands itself back to its <see cref="SpriteEffectPool"/>. An optional 2D light flares up
    /// with the first frame and fades out with the effect (accent light, design decision G8).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteEffect : MonoBehaviour
    {
        [Tooltip("Accent light that fades out over the effect; none if empty.")]
        [SerializeField] private Light2D _light;

        [Tooltip("Light intensity at the first frame.")]
        [SerializeField, Min(0f)] private float _lightIntensity = 1f;

        private SpriteRenderer _renderer;
        private Sprite[] _frames = Array.Empty<Sprite>();
        private float _frameDuration;
        private float _elapsed;
        private Action<SpriteEffect> _release;

        /// <summary>True while the effect plays.</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Shows <paramref name="frames"/> at <paramref name="position"/>, each for <paramref name="frameDuration"/> seconds,
        /// turned by <paramref name="angle"/> degrees. <paramref name="release"/> is called after the last frame; without it
        /// the effect deactivates itself.
        /// </summary>
        public void Play(Vector2 position, Sprite[] frames, float frameDuration, Action<SpriteEffect> release, float angle = 0f)
        {
            _frames = frames ?? Array.Empty<Sprite>();
            _frameDuration = frameDuration > 0f ? frameDuration : 0.01f;
            _release = release;
            _elapsed = 0f;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
            IsPlaying = true;
            ShowFrame(0);
            SetLight(EffectMath.LightFade(0f, _frames.Length * _frameDuration, _lightIntensity));
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            int frame = EffectMath.FrameAt(_elapsed, _frameDuration, _frames.Length);
            if (frame < 0)
            {
                Finish();
                return;
            }

            ShowFrame(frame);
            SetLight(EffectMath.LightFade(_elapsed, _frames.Length * _frameDuration, _lightIntensity));
        }

        private void ShowFrame(int frame)
        {
            if (_frames.Length == 0)
            {
                Finish();
                return;
            }

            _renderer.sprite = _frames[frame];
        }

        private void SetLight(float intensity)
        {
            if (_light != null)
            {
                _light.intensity = intensity;
                _light.enabled = intensity > 0f;
            }
        }

        private void Finish()
        {
            IsPlaying = false;
            _renderer.sprite = null;
            SetLight(0f);
            Action<SpriteEffect> release = _release;
            _release = null;
            if (release != null)
            {
                release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
