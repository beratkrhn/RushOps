using RushOps.Characters;
using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Drives the player's sprite animations (issue #102): walks while moving, with the cycle as fast as the figure
    /// moves (stick deflection, perks, carrying), rolls head over heels during a dash (issue #142), jerks back on each
    /// lost heart and plays the death once.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private DirectionalSpriteAnimator _view;

        private PlayerMovement _movement;
        private PlayerHealth _health;
        private PlayerDash _dash;
        private int _hearts = -1;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _health = GetComponent<PlayerHealth>();
            _dash = GetComponent<PlayerDash>();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.HealthChanged += OnHealthChanged;
                _health.Died += OnDied;
            }

            if (_dash != null)
            {
                _dash.Dashed += OnDashed;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.HealthChanged -= OnHealthChanged;
                _health.Died -= OnDied;
            }

            if (_dash != null)
            {
                _dash.Dashed -= OnDashed;
            }
        }

        private void Update()
        {
            PlayerConfig config = _movement.Config;
            if (_view == null || config == null || (_health != null && _health.IsDead))
            {
                return;
            }

            float rate = PlayerAnimationMath.WalkRate(
                _movement.Velocity.magnitude, config.MoveSpeed, config.WalkAnimationThreshold, config.MaxWalkAnimationRate);
            _view.SetMoving(rate > 0f, rate);
        }

        private void OnDashed(Vector2 direction)
        {
            PlayerConfig config = _movement.Config;
            if (_view != null && config != null)
            {
                _view.PlayDash(direction, config.DashDuration);
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            if (_view != null && _hearts >= 0 && current < _hearts && current > 0)
            {
                _view.PlayHit();
            }

            _hearts = current;
        }

        private void OnDied()
        {
            if (_view != null)
            {
                _view.SetMoving(false);
                _view.PlayDeath();
            }
        }
    }
}
