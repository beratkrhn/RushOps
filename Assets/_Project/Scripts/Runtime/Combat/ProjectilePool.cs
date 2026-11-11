using System;
using System.Collections.Generic;
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

        private readonly List<Projectile> _active = new List<Projectile>();
        private ObjectPool<Projectile> _pool;
        private Action<Projectile> _release;
        private Action<Projectile, Vector2> _onImpacted;

        /// <summary>Raised when one of the pool's projectiles hits a collider: hit point and flight direction (issue #107).</summary>
        public event Action<Vector2, Vector2> Impacted;

        /// <summary>Number of projectiles currently in flight.</summary>
        public int CountActive => _pool != null ? _pool.CountActive : 0;

        /// <summary>
        /// Takes a projectile from the pool and launches it; see <see cref="Projectile.Launch"/>.
        /// Returns null if no prefab is assigned.
        /// </summary>
        public Projectile Launch(Vector2 origin, Vector2 direction, float speed, float range, LayerMask hitLayers, int damage = 0,
            bool lethal = false, ShotLook look = ShotLook.Normal, Sprite normalSprite = null,
            Action<Projectile, Vector2> impact = null, int bounces = 0)
        {
            if (_pool == null)
            {
                return null;
            }

            Projectile projectile = _pool.Get();
            projectile.Launch(origin, direction, speed, range, hitLayers, damage, lethal, look, _release, normalSprite, impact,
                bounces);
            return projectile;
        }

        /// <summary>Number of projectiles in flight that were launched with <paramref name="impact"/>.</summary>
        public int CountCarrying(Action<Projectile, Vector2> impact)
        {
            int count = 0;
            for (int i = 0; i < _active.Count; i++)
            {
                count += _active[i].Carries(impact) ? 1 : 0;
            }

            return count;
        }

        /// <summary>
        /// Ends the flight of every projectile launched with <paramref name="impact"/>, without a hit, e.g. a super
        /// weapon's rockets at death or level end (issue #255). Other projectiles fly on.
        /// </summary>
        public void CancelAll(Action<Projectile, Vector2> impact)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (i < _active.Count && _active[i].Carries(impact))
                {
                    _active[i].Cancel();
                }
            }
        }

        private void Awake()
        {
            // Cached so launching does not allocate a delegate per shot.
            _release = Release;
            _onImpacted = OnImpacted;
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
            projectile.Impacted += _onImpacted;
            return projectile;
        }

        private void OnImpacted(Projectile projectile, Vector2 point)
        {
            Impacted?.Invoke(point, projectile.Direction);
        }

        private void OnGet(Projectile projectile)
        {
            _active.Add(projectile);
            projectile.gameObject.SetActive(true);
        }

        private void OnRelease(Projectile projectile)
        {
            _active.Remove(projectile);
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
