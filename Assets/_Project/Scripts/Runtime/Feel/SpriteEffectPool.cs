using System;
using UnityEngine;
using UnityEngine.Pool;

namespace RushOps.Feel
{
    /// <summary>
    /// Reuses <see cref="SpriteEffect"/> instances of one prefab (NF-02), so chain reactions and rapid fire do not
    /// instantiate. One pool per kind of effect (explosions, hit sparks, death splatter).
    /// </summary>
    public class SpriteEffectPool : MonoBehaviour
    {
        [SerializeField] private SpriteEffect _prefab;
        [SerializeField, Min(0)] private int _defaultCapacity = 4;
        [SerializeField, Min(1)] private int _maxSize = 16;

        private ObjectPool<SpriteEffect> _pool;
        private Action<SpriteEffect> _release;

        /// <summary>Number of effects currently playing.</summary>
        public int CountActive => _pool != null ? _pool.CountActive : 0;

        /// <summary>Plays <paramref name="frames"/> at <paramref name="position"/>, turned by <paramref name="angle"/> degrees; does nothing without prefab or frames.</summary>
        public void Play(Vector2 position, Sprite[] frames, float frameDuration, float angle = 0f)
        {
            if (_pool == null || frames == null || frames.Length == 0)
            {
                return;
            }

            _pool.Get().Play(position, frames, frameDuration, _release, angle);
        }

        private void Awake()
        {
            _release = Release;
            if (_prefab != null)
            {
                _pool = new ObjectPool<SpriteEffect>(Create, OnGet, OnRelease, OnDestroyPooled, true, _defaultCapacity, _maxSize);
            }
        }

        private void OnDestroy()
        {
            _pool?.Dispose();
        }

        private void Release(SpriteEffect effect)
        {
            _pool.Release(effect);
        }

        private SpriteEffect Create()
        {
            SpriteEffect effect = Instantiate(_prefab, transform);
            effect.gameObject.SetActive(false);
            return effect;
        }

        private static void OnGet(SpriteEffect effect)
        {
            effect.gameObject.SetActive(true);
        }

        private static void OnRelease(SpriteEffect effect)
        {
            effect.gameObject.SetActive(false);
        }

        private static void OnDestroyPooled(SpriteEffect effect)
        {
            if (effect != null)
            {
                Destroy(effect.gameObject);
            }
        }
    }
}
