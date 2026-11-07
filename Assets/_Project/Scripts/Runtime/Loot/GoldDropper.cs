using RushOps.Enemies;
using RushOps.Navigation;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Loot
{
    /// <summary>
    /// Lets killed enemies drop gold (F-GLD-01, F-GLD-03): every death drops the gold of its
    /// <see cref="EnemyDefinition"/> as single coins scattered around the death position, also after a nuke, an
    /// explosion or insta-kill. A coin on a blocked cell moves to the nearest walkable cell, or is lost.
    /// </summary>
    public class GoldDropper : MonoBehaviour
    {
        private const int MaxCoinsPerDrop = 64;

        [SerializeField] private GoldConfig _config;
        [Tooltip("Loot config whose placement radius says how far a coin may move off a blocked cell.")]
        [SerializeField] private LootConfig _loot;
        [Tooltip("Pools of all enemy types in the level.")]
        [SerializeField] private EnemyPool[] _pools = System.Array.Empty<EnemyPool>();
        [SerializeField] private GoldPickupPool _coins;
        [Tooltip("Knows which cells are walkable; without it coins land exactly where they were scattered.")]
        [SerializeField] private FlowFieldNavigator _navigator;

        private readonly Vector2[] _spots = new Vector2[MaxCoinsPerDrop];
        private IRandomSource _random;

        /// <summary>Replaces the random source, e.g. for reproducible tests.</summary>
        public void SetRandomSource(IRandomSource random)
        {
            _random = random;
        }

        /// <summary>Drops <paramref name="count"/> coins around <paramref name="position"/>. Returns how many landed.</summary>
        public int Drop(Vector2 position, int count)
        {
            if (_coins == null || count <= 0)
            {
                return 0;
            }

            _random ??= new SystemRandomSource();
            float radius = _config != null ? _config.ScatterRadius : 0f;
            int scattered = GoldMath.Scatter(position, radius, count, _random, _spots);
            int placement = _loot != null ? _loot.PlacementRadius : 0;
            int landed = 0;
            for (int i = 0; i < scattered; i++)
            {
                Vector2 spot = _spots[i];
                if (_navigator != null && !_navigator.TryFindWalkablePosition(spot, placement, out spot))
                {
                    continue;
                }

                if (_coins.Spawn(spot) != null)
                {
                    landed++;
                }
            }

            return landed;
        }

        private void OnEnable()
        {
            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    pool.EnemyDied += OnEnemyDied;
                }
            }
        }

        private void OnDisable()
        {
            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    pool.EnemyDied -= OnEnemyDied;
                }
            }
        }

        private void OnEnemyDied(Enemy enemy)
        {
            if (enemy.Definition != null)
            {
                Drop(enemy.transform.position, enemy.Definition.Gold);
            }
        }
    }
}
