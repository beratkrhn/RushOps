using RushOps.Audio;
using RushOps.Cameras;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>
    /// Connects game events to the hit feedback (issue #12): camera shake when the player takes damage,
    /// hitstop when an enemy dies, and the placeholder sounds for shots, hits and deaths, plus the spawn and approach
    /// sounds an enemy type brings in its <see cref="EnemyDefinition"/> (issue #20), and for every explosion a strong
    /// shake, its sound and its blast effect (issue #22). Enemies flash and
    /// get knocked back by themselves (<see cref="Enemy"/>).
    /// </summary>
    public class GameFeel : MonoBehaviour
    {
        [SerializeField] private FeelConfig _config;
        [SerializeField] private PlayerHealth _player;
        [SerializeField] private PlayerWeapon _weapon;
        [Tooltip("Pools of all enemy types in the level.")]
        [SerializeField] private EnemyPool[] _pools = System.Array.Empty<EnemyPool>();
        [SerializeField] private ExplosionSystem _explosions;
        [SerializeField] private ExplosionEffectPool _explosionEffects;
        [SerializeField] private CameraFollow _camera;
        [SerializeField] private Hitstop _hitstop;
        [SerializeField] private SfxPlayer _sfx;
        [SerializeField] private AudioClip _shotClip;
        [SerializeField] private AudioClip _hitClip;
        [SerializeField] private AudioClip _deathClip;

        private int _hearts = -1;

        private void OnEnable()
        {
            if (_player != null)
            {
                _player.HealthChanged += OnHealthChanged;
            }

            if (_weapon != null)
            {
                _weapon.Fired += OnFired;
            }

            if (_explosions != null)
            {
                _explosions.Exploded += OnExploded;
            }

            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    pool.EnemySpawned += OnEnemySpawned;
                    pool.EnemyApproached += OnEnemyApproached;
                    pool.EnemyHit += OnEnemyHit;
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

            if (_weapon != null)
            {
                _weapon.Fired -= OnFired;
            }

            if (_explosions != null)
            {
                _explosions.Exploded -= OnExploded;
            }

            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    pool.EnemySpawned -= OnEnemySpawned;
                    pool.EnemyApproached -= OnEnemyApproached;
                    pool.EnemyHit -= OnEnemyHit;
                    pool.EnemyDied -= OnEnemyDied;
                }
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            if (_hearts >= 0 && current < _hearts && _camera != null && _config != null)
            {
                _camera.Shake(_config.ShakeStrength, _config.ShakeDuration, _config.ShakeFrequency);
            }

            _hearts = current;
        }

        private void OnFired(Vector2 direction)
        {
            Play(_shotClip, _config != null ? _config.ShotVolume : 0f);
        }

        private void OnEnemySpawned(Enemy enemy)
        {
            if (enemy.Definition != null)
            {
                Play(enemy.Definition.SpawnClip, _config != null ? _config.EnemySpawnVolume : 0f);
            }
        }

        private void OnEnemyApproached(Enemy enemy)
        {
            if (enemy.Definition != null)
            {
                Play(enemy.Definition.ApproachClip, _config != null ? _config.EnemyApproachVolume : 0f);
            }
        }

        private void OnEnemyHit(Enemy enemy)
        {
            // The killing hit plays the death sound instead.
            if (!enemy.IsDead)
            {
                Play(_hitClip, _config != null ? _config.HitVolume : 0f);
            }
        }

        private void OnEnemyDied(Enemy enemy)
        {
            Play(_deathClip, _config != null ? _config.DeathVolume : 0f);
            if (_config != null && _config.HitstopEnabled && _hitstop != null)
            {
                _hitstop.Trigger(_config.HitstopDuration);
            }
        }

        private void OnExploded(Vector2 center, ExplosionDefinition explosion)
        {
            if (_camera != null)
            {
                _camera.Shake(explosion.ShakeStrength, explosion.ShakeDuration, explosion.ShakeFrequency);
            }

            if (_explosionEffects != null)
            {
                _explosionEffects.Play(center, explosion.EffectFrames, explosion.EffectFrameDuration);
            }

            Play(explosion.Clip, explosion.Volume);
        }

        private void Play(AudioClip clip, float volume)
        {
            if (_sfx != null && _config != null)
            {
                _sfx.Play(clip, volume, _config.PitchVariation);
            }
        }
    }
}
