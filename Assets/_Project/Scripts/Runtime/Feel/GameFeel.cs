using RushOps.Audio;
using RushOps.Cameras;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>
    /// Connects game events to the hit feedback (issue #12): camera shake when the player takes damage,
    /// hitstop when an enemy dies, and the placeholder sounds for shots, hits and deaths. Enemies flash and
    /// get knocked back by themselves (<see cref="Enemy"/>).
    /// </summary>
    public class GameFeel : MonoBehaviour
    {
        [SerializeField] private FeelConfig _config;
        [SerializeField] private PlayerHealth _player;
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private EnemyPool _enemies;
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

            if (_enemies != null)
            {
                _enemies.EnemyHit += OnEnemyHit;
                _enemies.EnemyDied += OnEnemyDied;
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

            if (_enemies != null)
            {
                _enemies.EnemyHit -= OnEnemyHit;
                _enemies.EnemyDied -= OnEnemyDied;
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

        private void Play(AudioClip clip, float volume)
        {
            if (_sfx != null && _config != null)
            {
                _sfx.Play(clip, volume, _config.PitchVariation);
            }
        }
    }
}
