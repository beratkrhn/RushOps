using System;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Scoring
{
    /// <summary>
    /// The level's score (F-SCO-01, F-SCO-02, issue #28): every enemy death adds the points of its
    /// <see cref="EnemyDefinition"/>, also kills by explosions and the nuke; a heart lost breaks the combo. Thin
    /// adapter; the rules live in <see cref="ScoreKeeper"/>, the values in <see cref="ScoreConfig"/>.
    /// </summary>
    public class LevelScore : MonoBehaviour
    {
        [SerializeField] private ScoreConfig _config;
        [SerializeField] private PlayerHealth _player;
        [Tooltip("Pools of all enemy types in the level.")]
        [SerializeField] private EnemyPool[] _pools = Array.Empty<EnemyPool>();

        private ScoreKeeper _keeper;
        private int _hearts = -1;

        /// <summary>Score, multiplier and combo of the level.</summary>
        public ScoreKeeper Keeper => _keeper ??= CreateKeeper();

        private ScoreKeeper CreateKeeper()
        {
            return _config != null
                ? new ScoreKeeper(_config.ComboWindow, _config.KillsPerStep, _config.MaxMultiplier)
                : new ScoreKeeper(0f, 1, 1);
        }

        private void OnEnable()
        {
            if (_player != null)
            {
                _player.HealthChanged += OnHealthChanged;
            }

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
            if (_player != null)
            {
                _player.HealthChanged -= OnHealthChanged;
            }

            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    pool.EnemyDied -= OnEnemyDied;
                }
            }
        }

        private void Update()
        {
            Keeper.Tick(Time.deltaTime);
        }

        private void OnEnemyDied(Enemy enemy)
        {
            Keeper.RegisterKill(enemy.Definition != null ? enemy.Definition.Points : 0);
        }

        private void OnHealthChanged(int current, int max)
        {
            if (_hearts >= 0 && current < _hearts)
            {
                Keeper.BreakCombo();
            }

            _hearts = current;
        }
    }
}
