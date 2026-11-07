using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace RushOps.Loot
{
    /// <summary>
    /// Reuses <see cref="GoldPickup"/> instances (NF-02) and reports when the player collects a coin, e.g. for the
    /// wallet, the score and the pickup sound.
    /// </summary>
    public class GoldPickupPool : MonoBehaviour
    {
        [SerializeField] private GoldPickup _prefab;
        [SerializeField] private GoldConfig _config;
        [Tooltip("The coins fly to this transform.")]
        [SerializeField] private Transform _player;
        [SerializeField, Min(0)] private int _defaultCapacity = 32;
        [SerializeField, Min(1)] private int _maxSize = 128;

        private readonly List<GoldPickup> _active = new List<GoldPickup>();
        private ObjectPool<GoldPickup> _pool;
        private Action<GoldPickup> _release;

        /// <summary>Raised when a coin appeared on the floor.</summary>
        public event Action<GoldPickup> Dropped;

        /// <summary>Raised when the player collected a coin.</summary>
        public event Action<GoldPickup> Collected;

        /// <summary>Raised when a coin disappeared uncollected.</summary>
        public event Action<GoldPickup> Expired;

        /// <summary>Gold settings.</summary>
        public GoldConfig Config => _config;

        /// <summary>Number of coins currently on the floor.</summary>
        public int CountActive => _pool != null ? _pool.CountActive : 0;

        /// <summary>Coins currently on the floor.</summary>
        public IReadOnlyList<GoldPickup> Active => _active;

        /// <summary>Drops one coin at <paramref name="position"/>; null without prefab.</summary>
        public GoldPickup Spawn(Vector2 position)
        {
            if (_pool == null)
            {
                return null;
            }

            GoldPickup coin = _pool.Get();
            coin.Spawn(position, _config, _player, _release);
            Dropped?.Invoke(coin);
            return coin;
        }

        private void Awake()
        {
            _release = Release;
            if (_prefab != null)
            {
                _pool = new ObjectPool<GoldPickup>(Create, OnGet, OnRelease, OnDestroyPooled, true, _defaultCapacity, _maxSize);
            }
        }

        private void OnDestroy()
        {
            _pool?.Dispose();
        }

        private void Release(GoldPickup coin)
        {
            _pool.Release(coin);
        }

        private GoldPickup Create()
        {
            GoldPickup coin = Instantiate(_prefab, transform);
            coin.gameObject.SetActive(false);
            coin.Collected += OnCollected;
            coin.Expired += OnExpired;
            return coin;
        }

        private void OnCollected(GoldPickup coin)
        {
            Collected?.Invoke(coin);
        }

        private void OnExpired(GoldPickup coin)
        {
            Expired?.Invoke(coin);
        }

        private void OnGet(GoldPickup coin)
        {
            coin.gameObject.SetActive(true);
            _active.Add(coin);
        }

        private void OnRelease(GoldPickup coin)
        {
            _active.Remove(coin);
            coin.gameObject.SetActive(false);
        }

        private void OnDestroyPooled(GoldPickup coin)
        {
            if (coin != null)
            {
                coin.Collected -= OnCollected;
                coin.Expired -= OnExpired;
                Destroy(coin.gameObject);
            }
        }
    }
}
