using System;
using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>
    /// Plays a blast's frames once at a fixed spot (issue #22), then hands itself back to its
    /// <see cref="ExplosionEffectPool"/>.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExplosionEffect : MonoBehaviour
    {
        private SpriteRenderer _renderer;
        private Sprite[] _frames = Array.Empty<Sprite>();
        private float _frameDuration;
        private float _elapsed;
        private Action<ExplosionEffect> _release;

        /// <summary>True while the effect plays.</summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Shows <paramref name="frames"/> at <paramref name="position"/>, each for <paramref name="frameDuration"/> seconds.
        /// <paramref name="release"/> is called after the last frame; without it the effect deactivates itself.
        /// </summary>
        public void Play(Vector2 position, Sprite[] frames, float frameDuration, Action<ExplosionEffect> release)
        {
            _frames = frames ?? Array.Empty<Sprite>();
            _frameDuration = frameDuration > 0f ? frameDuration : 0.01f;
            _release = release;
            _elapsed = 0f;
            transform.position = position;
            IsPlaying = true;
            ShowFrame(0);
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
            int frame = (int)(_elapsed / _frameDuration);
            if (frame >= _frames.Length)
            {
                Finish();
                return;
            }

            ShowFrame(frame);
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

        private void Finish()
        {
            IsPlaying = false;
            _renderer.sprite = null;
            Action<ExplosionEffect> release = _release;
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
