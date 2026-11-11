using RushOps.Enemies;
using RushOps.Navigation;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// Lets killed enemies drop perks (F-PRK-02): every death rolls <see cref="LootConfig.DropChance"/> and picks a
    /// perk from the weighted <see cref="PerkPool"/>. The perk lands where the enemy died, or on the nearest walkable
    /// cell if that spot is blocked (F-PRK-01).
    /// </summary>
    public class PerkDropper : MonoBehaviour
    {
        [SerializeField] private LootConfig _config;
        [Tooltip("Pools of all enemy types in the level.")]
        [SerializeField] private EnemyPool[] _pools = System.Array.Empty<EnemyPool>();
        [SerializeField] private PerkPickupPool _pickups;
        [Tooltip("Knows which cells are walkable; without it perks drop exactly where the enemy died.")]
        [SerializeField] private FlowFieldNavigator _navigator;

        private PerkPool _pool;
        private float _chanceBonus;

        /// <summary>
        /// Added to the drop chance of the config as probability, e.g. 0.03 with the perk luck upgrade of the endless run
        /// (F-END-12); 0 by default.
        /// </summary>
        public float ChanceBonus
        {
            get => _chanceBonus;
            set => _chanceBonus = Mathf.Max(0f, value);
        }

        /// <summary>Drop chance per death: the config's plus <see cref="ChanceBonus"/>, at most 1.</summary>
        public float DropChance => Mathf.Min(1f, (_config != null ? _config.DropChance : 0f) + _chanceBonus);

        /// <summary>The weighted pool, e.g. for tests.</summary>
        public PerkPool Pool => _pool ??= CreatePool(new SystemRandomSource());

        /// <summary>Replaces the random source, e.g. so tests can force or prevent drops.</summary>
        public void SetRandomSource(IRandomSource random)
        {
            _pool = CreatePool(random);
        }

        /// <summary>
        /// Drops <paramref name="perk"/> at <paramref name="position"/> or the nearest walkable cell.
        /// Null if no walkable cell is near or the perk is null.
        /// </summary>
        public PerkPickup Drop(Vector2 position, PerkDefinition perk)
        {
            if (_pickups == null || perk == null)
            {
                return null;
            }

            Vector2 spot = position;
            int radius = _config != null ? _config.PlacementRadius : 0;
            if (_navigator != null && !_navigator.TryFindWalkablePosition(position, radius, out spot))
            {
                return null;
            }

            return _pickups.Spawn(spot, perk);
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
            if (_config != null && Pool.TryDrop(DropChance, out PerkDefinition perk))
            {
                Drop(enemy.transform.position, perk);
            }
        }

        private PerkPool CreatePool(IRandomSource random)
        {
            return new PerkPool(_config != null ? _config.Perks : null, random);
        }
    }
}
