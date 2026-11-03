using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace RushOps.Perks
{
    /// <summary>
    /// Reuses <see cref="PerkPickup"/> instances (NF-02) and reports when the player collects a perk, e.g. for the
    /// perk effects (issue #24) and the pickup sound.
    /// </summary>
    public class PerkPickupPool : MonoBehaviour
    {
        [SerializeField] private PerkPickup _prefab;
        [SerializeField] private LootConfig _config;
        [SerializeField, Min(0)] private int _defaultCapacity = 4;
        [SerializeField, Min(1)] private int _maxSize = 16;

        private readonly List<PerkPickup> _active = new List<PerkPickup>();
        private ObjectPool<PerkPickup> _pool;
        private Action<PerkPickup> _release;

        /// <summary>Raised when a perk appeared on the floor.</summary>
        public event Action<PerkPickup> Dropped;

        /// <summary>Raised when the player collected a perk.</summary>
        public event Action<PerkPickup> Collected;

        /// <summary>Raised when a perk disappeared uncollected.</summary>
        public event Action<PerkPickup> Expired;

        /// <summary>Drop settings.</summary>
        public LootConfig Config => _config;

        /// <summary>Number of perks currently on the floor.</summary>
        public int CountActive => _pool != null ? _pool.CountActive : 0;

        /// <summary>Perks currently on the floor.</summary>
        public IReadOnlyList<PerkPickup> Active => _active;

        /// <summary>Drops <paramref name="perk"/> at <paramref name="position"/>; null without prefab or perk.</summary>
        public PerkPickup Spawn(Vector2 position, PerkDefinition perk)
        {
            if (_pool == null || perk == null)
            {
                return null;
            }

            PerkPickup pickup = _pool.Get();
            pickup.Spawn(position, perk, _config, _release);
            Dropped?.Invoke(pickup);
            return pickup;
        }

        private void Awake()
        {
            _release = Release;
            if (_prefab != null)
            {
                _pool = new ObjectPool<PerkPickup>(Create, OnGet, OnRelease, OnDestroyPooled, true, _defaultCapacity, _maxSize);
            }
        }

        private void OnDestroy()
        {
            _pool?.Dispose();
        }

        private void Release(PerkPickup pickup)
        {
            _pool.Release(pickup);
        }

        private PerkPickup Create()
        {
            PerkPickup pickup = Instantiate(_prefab, transform);
            pickup.gameObject.SetActive(false);
            pickup.Collected += OnCollected;
            pickup.Expired += OnExpired;
            return pickup;
        }

        private void OnCollected(PerkPickup pickup)
        {
            Collected?.Invoke(pickup);
        }

        private void OnExpired(PerkPickup pickup)
        {
            Expired?.Invoke(pickup);
        }

        private void OnGet(PerkPickup pickup)
        {
            pickup.gameObject.SetActive(true);
            _active.Add(pickup);
        }

        private void OnRelease(PerkPickup pickup)
        {
            _active.Remove(pickup);
            pickup.gameObject.SetActive(false);
        }

        private void OnDestroyPooled(PerkPickup pickup)
        {
            if (pickup != null)
            {
                pickup.Collected -= OnCollected;
                pickup.Expired -= OnExpired;
                Destroy(pickup.gameObject);
            }
        }
    }
}
