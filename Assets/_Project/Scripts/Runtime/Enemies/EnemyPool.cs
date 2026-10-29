using System;
using RushOps.Navigation;
using UnityEngine;
using UnityEngine.Pool;

namespace RushOps.Enemies
{
    /// <summary>
    /// Reuses <see cref="Enemy"/> instances of one prefab (NF-02). Spawned enemies chase via the navigator and
    /// return to the pool by themselves after their death effect.
    /// </summary>
    public class EnemyPool : MonoBehaviour
    {
        [SerializeField] private Enemy _prefab;
        [SerializeField] private FlowFieldNavigator _navigator;
        [SerializeField, Min(0)] private int _defaultCapacity = 10;
        [SerializeField, Min(1)] private int _maxSize = 32;

        private ObjectPool<Enemy> _pool;
        private Action<Enemy> _release;

        /// <summary>Raised when a spawned enemy dies.</summary>
        public event Action<Enemy> EnemyDied;

        /// <summary>Number of enemies currently in the level, including dying ones.</summary>
        public int CountActive => _pool != null ? _pool.CountActive : 0;

        /// <summary>Takes an enemy from the pool and spawns it at <paramref name="position"/>; null without prefab.</summary>
        public Enemy Spawn(Vector2 position)
        {
            if (_pool == null)
            {
                return null;
            }

            Enemy enemy = _pool.Get();
            enemy.Spawn(position, _navigator, _release);
            return enemy;
        }

        private void Awake()
        {
            // Cached so spawning does not allocate a delegate per enemy.
            _release = Release;
            if (_prefab != null)
            {
                _pool = new ObjectPool<Enemy>(Create, OnGet, OnRelease, OnDestroyPooled, true, _defaultCapacity, _maxSize);
            }
        }

        private void OnDestroy()
        {
            _pool?.Dispose();
        }

        private void Release(Enemy enemy)
        {
            _pool.Release(enemy);
        }

        private Enemy Create()
        {
            Enemy enemy = Instantiate(_prefab, transform);
            enemy.gameObject.SetActive(false);
            enemy.Died += OnEnemyDied;
            return enemy;
        }

        private void OnEnemyDied(Enemy enemy)
        {
            EnemyDied?.Invoke(enemy);
        }

        private static void OnGet(Enemy enemy)
        {
            enemy.gameObject.SetActive(true);
        }

        private static void OnRelease(Enemy enemy)
        {
            enemy.gameObject.SetActive(false);
        }

        private void OnDestroyPooled(Enemy enemy)
        {
            if (enemy != null)
            {
                enemy.Died -= OnEnemyDied;
                Destroy(enemy.gameObject);
            }
        }
    }
}
