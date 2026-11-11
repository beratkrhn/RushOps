using System;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.Player;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>
    /// Vibrates the phone (issue #300, F-AUD-05, N32): a strong pulse when the player loses a heart, a medium one when
    /// a boomer, rocket or nuke goes off on screen. Off when the player switched „Vibration“ off in the settings.
    /// Thin adapter; the gap between pulses comes from <see cref="HapticRules"/>.
    /// </summary>
    public class Haptics : MonoBehaviour
    {
        [SerializeField] private HapticConfig _config;
        [Tooltip("Save game with the setting „Vibration“; empty counts as on.")]
        [SerializeField] private SaveGameService _save;
        [SerializeField] private PlayerHealth _player;
        [SerializeField] private ExplosionSystem _explosions;
        [SerializeField] private NukeSystem _nuke;
        [SerializeField] private RocketLauncher _rocket;
        [Tooltip("Camera whose view decides whether an explosion is on screen.")]
        [SerializeField] private Camera _camera;

        private HapticRules _rules;
        private int _hearts = -1;

        /// <summary>Raised for every pulse that goes out (also in the editor, where the phone part does nothing).</summary>
        public event Action<HapticKind> Pulsed;

        /// <summary>Wires the rocket launcher, which the scene builder creates after this component.</summary>
        public void SetRocket(RocketLauncher rocket)
        {
            Unsubscribe();
            _rocket = rocket;
            Subscribe();
        }

        private bool Enabled => _save == null || SettingsRules.GetVibration(_save.Data.Settings);

        private void Awake()
        {
            _rules = new HapticRules(_config != null ? _config.MinGapSeconds : 0.1f);
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (_player != null)
            {
                _hearts = _player.Current;
                _player.HealthChanged += OnHealthChanged;
            }

            if (_explosions != null)
            {
                _explosions.Exploded += OnExploded;
            }

            if (_nuke != null)
            {
                _nuke.Detonated += OnNukeDetonated;
            }

            if (_rocket != null)
            {
                _rocket.Exploded += OnRocketExploded;
            }
        }

        private void Unsubscribe()
        {
            if (_player != null)
            {
                _player.HealthChanged -= OnHealthChanged;
            }

            if (_explosions != null)
            {
                _explosions.Exploded -= OnExploded;
            }

            if (_nuke != null)
            {
                _nuke.Detonated -= OnNukeDetonated;
            }

            if (_rocket != null)
            {
                _rocket.Exploded -= OnRocketExploded;
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            if (_hearts >= 0 && current < _hearts)
            {
                Pulse(HapticKind.Damage);
            }

            _hearts = current;
        }

        private void OnExploded(Vector2 center, ExplosionDefinition explosion)
        {
            if (OnScreen(center))
            {
                Pulse(HapticKind.Explosion);
            }
        }

        private void OnRocketExploded(Vector2 center)
        {
            if (OnScreen(center))
            {
                Pulse(HapticKind.Explosion);
            }
        }

        private void OnNukeDetonated(int kills)
        {
            Pulse(HapticKind.Explosion);
        }

        private bool OnScreen(Vector2 point)
        {
            if (_camera == null || !_camera.orthographic)
            {
                return true;
            }

            float margin = _config != null ? _config.ScreenMargin : 1f;
            Vector2 offset = point - (Vector2)_camera.transform.position;
            float halfHeight = _camera.orthographicSize + margin;
            float halfWidth = _camera.orthographicSize * _camera.aspect + margin;
            return Mathf.Abs(offset.x) <= halfWidth && Mathf.Abs(offset.y) <= halfHeight;
        }

        private void Pulse(HapticKind kind)
        {
            if (!Enabled || !_rules.Request(kind, Time.unscaledTime))
            {
                return;
            }

            HapticOutput.Play(kind);
            Pulsed?.Invoke(kind);
        }
    }
}
