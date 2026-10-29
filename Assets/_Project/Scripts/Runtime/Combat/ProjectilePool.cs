using System;
using UnityEngine;
using UnityEngine.Pool;

namespace RushOps.Combat
{
    /// <summary>
    /// Reuses <see cref="Projectile"/> instances of one prefab (NF-02). Projectiles launched through
    /// <see cref="Launch"/> return to the pool by themselves when they expire.
    /// </summary>
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private Projectile _prefab;
        [SerializeField, Min(0)] private int _defaultCapacity = 32;
        [SerializeField, Min(1)] private int _maxSize = 128;

        private ObjectPool<Projectile> _pool;
        private Action<Projectile> _release;

        /// <summary>Number of projectiles currently in flight.</summary>
        public int CountActive => _pool != null ? _pool.CountActive : 0;

        /// <summary>
        /// Takes a projectile from the pool and launches it; see <see cref="Projectile.Launch"/>.
        /// Returns null if no prefab is assigned.
        /// </summary>
        public Projectile Launch(Vector2 origin, Vector2 direction, float speed, float range, LayerMask obstacles)
        {
            if (_pool == null)
            {
                return null;
            }

            Projectile projectile = _pool.Get();
            projectile.Launch(origin, direction, speed, range, obstacles, _release);
            return projectile;
        }

        private void Awake()
        {
            // Cached so launching does not allocate a delegate per shot.
            _release = Release;
            if (_prefab != null)
            {
                _pool = new ObjectPool<Projectile>(
                    Create, OnGet, OnRelease, OnDestroyPooled, true, _defaultCapacity, _maxSize);
            }
        }

        private void OnDestroy()
        {
            _pool?.Dispose();
        }

        private void Release(Projectile projectile)
        {
            _pool.Release(projectile);
        }

        private Projectile Create()
        {
            Projectile projectile = Instantiate(_prefab, transform);
            projectile.gameObject.SetActive(false);
            return projectile;
        }

        private static void OnGet(Projectile projectile)
        {
            projectile.gameObject.SetActive(true);
        }

        private static void OnRelease(Projectile projectile)
        {
            projectile.gameObject.SetActive(false);
        }

        private static void OnDestroyPooled(Projectile projectile)
        {
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }
    }
}
