using System;
using System.Collections.Generic;
using RushOps.Combat;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Blows up explosive enemies (boomer, F-GEG-04, issue #22): when an enemy with an
    /// <see cref="EnemyDefinition.Explosion"/> dies, its explosion hurts the player and every other enemy in the
    /// radius. The player's damage goes through <see cref="PlayerHealth.TakeDamage"/>, so dash i-frames and the hit
    /// invulnerability protect. Enemies killed by the blast die like any other, so they count for kill gates and score;
    /// explosive ones among them go off after the chain delay, so chain reactions run visibly one after another.
    /// Feedback (shake, sound, effect) hangs on <see cref="Exploded"/>.
    /// </summary>
    public class ExplosionSystem : MonoBehaviour
    {
        [Tooltip("Pools of all enemy types in the level: their enemies can explode and be hit.")]
        [SerializeField] private EnemyPool[] _pools = Array.Empty<EnemyPool>();
        [SerializeField] private PlayerHealth _player;

        private readonly ExplosionQueue _queue = new ExplosionQueue();
        private readonly List<Enemy> _targets = new List<Enemy>(32);
        private Collider2D _playerBody;
        // Deaths while a blast deals its damage are chain reactions.
        private bool _dealingDamage;

        /// <summary>Raised after an explosion dealt its damage, with its center and definition.</summary>
        public event Action<Vector2, ExplosionDefinition> Exploded;

        /// <summary>Explosions waiting to go off, e.g. chain reactions.</summary>
        public int Pending => _queue.Count;

        /// <summary>Sets off <paramref name="definition"/> at <paramref name="center"/> right now.</summary>
        public void Explode(Vector2 center, ExplosionDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            _dealingDamage = true;
            try
            {
                HitPlayer(center, definition);
                HitEnemies(center, definition);
            }
            finally
            {
                _dealingDamage = false;
                _targets.Clear();
            }

            Exploded?.Invoke(center, definition);
        }

        private void Awake()
        {
            if (_player != null)
            {
                _playerBody = _player.GetComponent<Collider2D>();
            }
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

            _queue.Clear();
        }

        private void Update()
        {
            _queue.Advance(Time.deltaTime);
            while (_queue.TryTakeDue(out PendingExplosion explosion))
            {
                Explode(explosion.Position, explosion.Definition);
            }
        }

        private void OnEnemyDied(Enemy enemy)
        {
            ExplosionDefinition explosion = enemy.Definition != null ? enemy.Definition.Explosion : null;
            if (explosion != null)
            {
                _queue.Schedule(enemy.ExplosionPoint, explosion, _dealingDamage ? explosion.ChainDelay : 0f);
            }
        }

        private void HitPlayer(Vector2 center, ExplosionDefinition definition)
        {
            if (_player == null || definition.PlayerDamage <= 0)
            {
                return;
            }

            Vector2 point = _playerBody != null ? (Vector2)_playerBody.bounds.center : (Vector2)_player.transform.position;
            if (ExplosionTargeting.IsInRadius(center, definition.Radius, point))
            {
                _player.TakeDamage(definition.PlayerDamage);
            }
        }

        private void HitEnemies(Vector2 center, ExplosionDefinition definition)
        {
            if (definition.EnemyDamage <= 0)
            {
                return;
            }

            // Collected first: hits kill enemies and may schedule new explosions while we go.
            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    ExplosionTargeting.Collect(center, definition.Radius, pool.Active, _targets);
                }
            }

            for (int i = 0; i < _targets.Count; i++)
            {
                Enemy enemy = _targets[i];
                Vector2 away = enemy.ExplosionPoint - center;
                enemy.TakeHit(definition.EnemyDamage, away.sqrMagnitude > 1e-6f ? away : Vector2.up);
            }
        }
    }
}
