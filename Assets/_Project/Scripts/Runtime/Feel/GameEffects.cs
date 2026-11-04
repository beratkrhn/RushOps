using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>
    /// Connects combat events to the visual effects (issue #107): the muzzle flash on every shot, sparks where a shot
    /// hits, and a splatter where an enemy without explosion dies (explosive enemies show their blast instead). All
    /// effects come from pools (NF-02).
    /// </summary>
    public class GameEffects : MonoBehaviour
    {
        [SerializeField] private EffectsConfig _config;
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private MuzzleFlash _muzzleFlash;
        [SerializeField] private ProjectilePool _projectiles;
        [Tooltip("Pools of all enemy types in the level.")]
        [SerializeField] private EnemyPool[] _enemies = System.Array.Empty<EnemyPool>();
        [SerializeField] private SpriteEffectPool _hitSparks;
        [SerializeField] private SpriteEffectPool _deaths;

        private void OnEnable()
        {
            if (_weapon != null)
            {
                _weapon.Fired += OnFired;
            }

            if (_projectiles != null)
            {
                _projectiles.Impacted += OnImpacted;
            }

            foreach (EnemyPool pool in _enemies)
            {
                if (pool != null)
                {
                    pool.EnemyDied += OnEnemyDied;
                }
            }
        }

        private void OnDisable()
        {
            if (_weapon != null)
            {
                _weapon.Fired -= OnFired;
            }

            if (_projectiles != null)
            {
                _projectiles.Impacted -= OnImpacted;
            }

            foreach (EnemyPool pool in _enemies)
            {
                if (pool != null)
                {
                    pool.EnemyDied -= OnEnemyDied;
                }
            }
        }

        private void OnFired(Vector2 direction)
        {
            if (_muzzleFlash != null && _weapon != null)
            {
                _muzzleFlash.Show(_weapon.MuzzlePosition(direction), direction);
            }
        }

        private void OnImpacted(Vector2 point, Vector2 direction)
        {
            if (_hitSparks != null && _config != null)
            {
                // Sparks fly back towards the shooter.
                float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
                _hitSparks.Play(point, _config.HitSparkFrames, _config.HitSparkFrameDuration, angle);
            }
        }

        private void OnEnemyDied(Enemy enemy)
        {
            if (_deaths == null || _config == null || (enemy.Definition != null && enemy.Definition.Explosion != null))
            {
                return;
            }

            _deaths.Play(enemy.AimPoint, _config.DeathFrames, _config.DeathFrameDuration);
        }
    }
}
