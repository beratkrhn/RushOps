using System;
using System.Collections.Generic;
using RushOps.Cameras;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>
    /// The nuke perk (F-PRK-04, issue #27): when the player collects it, every living enemy inside the camera view
    /// dies at once, except bosses (F-BOS-02). Enemies off screen and the spawn queue stay untouched. The enemies die
    /// like any other, so they count for kill gates and score (F-LVL-03). Feedback hangs on <see cref="Detonated"/>.
    /// </summary>
    public class NukeSystem : MonoBehaviour
    {
        [SerializeField] private NukeConfig _config;
        [Tooltip("Reports the collected perks.")]
        [SerializeField] private PlayerPerks _perks;
        [Tooltip("Its view decides which enemies are visible.")]
        [SerializeField] private CameraFollow _camera;
        [Tooltip("Pools of all enemy types in the level.")]
        [SerializeField] private EnemyPool[] _pools = Array.Empty<EnemyPool>();

        private readonly List<Enemy> _targets = new List<Enemy>(64);

        /// <summary>Raised after a nuke went off, with the number of enemies it killed.</summary>
        public event Action<int> Detonated;

        /// <summary>Strengths of the flash, shake and sound.</summary>
        public NukeConfig Config => _config;

        /// <summary>Kills all living, visible enemies except bosses.</summary>
        /// <returns>The number of enemies killed.</returns>
        public int Detonate()
        {
            Vector2 center = _camera != null ? (Vector2)_camera.transform.position : Vector2.zero;
            Vector2 halfExtents = _camera != null ? _camera.HalfExtents : Vector2.zero;

            // Collected first: deaths may change the pools' active lists, e.g. boomers exploding.
            foreach (EnemyPool pool in _pools)
            {
                if (pool == null)
                {
                    continue;
                }

                IReadOnlyList<Enemy> active = pool.Active;
                for (int i = 0; i < active.Count; i++)
                {
                    Enemy enemy = active[i];
                    bool isBoss = enemy.Definition != null && enemy.Definition.IsBoss;
                    bool inView = NukeTargeting.IsInView(center, halfExtents, enemy.AimPoint);
                    if (NukeTargeting.Kills(!enemy.IsDead, isBoss, inView))
                    {
                        _targets.Add(enemy);
                    }
                }
            }

            int kills = 0;
            for (int i = 0; i < _targets.Count; i++)
            {
                kills += _targets[i].Kill() ? 1 : 0;
            }

            _targets.Clear();
            Detonated?.Invoke(kills);
            return kills;
        }

        private void OnEnable()
        {
            if (_perks != null)
            {
                _perks.Applied += OnApplied;
            }
        }

        private void OnDisable()
        {
            if (_perks != null)
            {
                _perks.Applied -= OnApplied;
            }
        }

        private void OnApplied(PerkDefinition perk)
        {
            if (perk.Effect == PerkEffect.Nuke)
            {
                Detonate();
            }
        }
    }
}
