using RushOps.Levels;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Scoring
{
    /// <summary>
    /// Follows the running try of a campaign level for its badges (issue #288, F-SCO-04): thin adapter on
    /// <see cref="BadgeTracker"/>. Hearts lost come from <see cref="PlayerHealth"/>, the multiplier from the
    /// <see cref="LevelScore"/>, the play time from the scaled frame time, so pause and the result screens do not count.
    /// A restart reloads the scene and so starts a new try.
    /// </summary>
    public class LevelBadges : MonoBehaviour
    {
        [SerializeField] private BadgeConfig _config;
        [SerializeField] private LevelDefinition _level;
        [SerializeField] private PlayerHealth _player;
        [SerializeField] private LevelScore _score;

        private readonly BadgeTracker _tracker = new BadgeTracker();
        private int _lastHearts = -1;

        /// <summary>The badge values of this level.</summary>
        public BadgeConfig Config => _config;

        /// <summary>The level this scene plays.</summary>
        public LevelDefinition Level => _level;

        /// <summary>The running try.</summary>
        public BadgeTracker Tracker => _tracker;

        /// <summary>Target time of „Blitz“ in seconds; 0 if the level has none.</summary>
        public float TargetSeconds => _level != null ? _level.BadgeSeconds : 0f;

        /// <summary>Multiplier to reach for „Kombo“.</summary>
        public int ComboTarget => _config != null ? _config.ComboTarget : 5;

        /// <summary>Badges the running try earns if the level is finished now.</summary>
        public BadgeKind Earned => _tracker.Earned(true, TargetSeconds, ComboTarget);

        private void OnEnable()
        {
            if (_player != null)
            {
                _lastHearts = _player.Max > 0 ? _player.Current : -1;
                _player.HealthChanged += OnHealthChanged;
            }

            if (_score != null && _score.Keeper != null)
            {
                _score.Keeper.MultiplierChanged += OnMultiplierChanged;
            }
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.HealthChanged -= OnHealthChanged;
            }

            if (_score != null && _score.Keeper != null)
            {
                _score.Keeper.MultiplierChanged -= OnMultiplierChanged;
            }
        }

        private void Update()
        {
            _tracker.Advance(Time.deltaTime);
        }

        private void OnHealthChanged(int current, int max)
        {
            // Health may not be set up yet when this component is enabled; the first report only sets the baseline.
            if (_lastHearts >= 0 && current < _lastHearts)
            {
                _tracker.NotifyHeartsLost(_lastHearts - current);
            }

            _lastHearts = current;
        }

        private void OnMultiplierChanged(int multiplier)
        {
            _tracker.NotifyMultiplier(multiplier);
        }
    }
}
