using System;
using UnityEngine;
using UnityEngine.Pool;

namespace RushOps.Feel
{
    /// <summary>Reuses <see cref="ExplosionEffect"/> instances (NF-02), so chain reactions do not instantiate.</summary>
    public class ExplosionEffectPool : MonoBehaviour
    {
        [SerializeField] private ExplosionEffect _prefab;
        [SerializeField, Min(0)] private int _defaultCapacity = 4;
        [SerializeField, Min(1)] private int _maxSize = 16;

        private ObjectPool<ExplosionEffect> _pool;
        private Action<ExplosionEffect> _release;

        /// <summary>Number of effects currently playing.</summary>
        public int CountActive => _pool != null ? _pool.CountActive : 0;

        /// <summary>Plays <paramref name="frames"/> at <paramref name="position"/>; does nothing without prefab or frames.</summary>
        public void Play(Vector2 position, Sprite[] frames, float frameDuration)
        {
            if (_pool == null || frames == null || frames.Length == 0)
            {
                return;
            }

            _pool.Get().Play(position, frames, frameDuration, _release);
        }

        private void Awake()
        {
            _release = Release;
            if (_prefab != null)
            {
                _pool = new ObjectPool<ExplosionEffect>(Create, OnGet, OnRelease, OnDestroyPooled, true, _defaultCapacity, _maxSize);
            }
        }

        private void OnDestroy()
        {
            _pool?.Dispose();
        }

        private void Release(ExplosionEffect effect)
        {
            _pool.Release(effect);
        }

        private ExplosionEffect Create()
        {
            ExplosionEffect effect = Instantiate(_prefab, transform);
            effect.gameObject.SetActive(false);
            return effect;
        }

        private static void OnGet(ExplosionEffect effect)
        {
            effect.gameObject.SetActive(true);
        }

        private static void OnRelease(ExplosionEffect effect)
        {
            effect.gameObject.SetActive(false);
        }

        private static void OnDestroyPooled(ExplosionEffect effect)
        {
            if (effect != null)
            {
                Destroy(effect.gameObject);
            }
        }
    }
}
