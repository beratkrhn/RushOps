using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// Feeds the level's <see cref="KillCounter"/> (F-LVL-03) from the deaths of all pooled enemies. Every death
    /// goes through <see cref="Enemy.Died"/>, so kills by shots, explosions and the nuke all count.
    /// </summary>
    public class LevelKills : MonoBehaviour
    {
        [Tooltip("Pools of all enemy types in the level.")]
        [SerializeField] private EnemyPool[] _pools = System.Array.Empty<EnemyPool>();

        /// <summary>Kills since level start.</summary>
        public KillCounter Counter { get; } = new KillCounter();

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
            Counter.Register();
        }
    }
}
