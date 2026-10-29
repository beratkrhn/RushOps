using RushOps.Player;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Shows the player's hearts top left (F-HUD-01) through <see cref="PlayerHealth.HealthChanged"/>:
    /// full and empty heart sprites; a lost heart blinks red for a moment before it shows empty.
    /// Images beyond the maximum are hidden.
    /// </summary>
    public class HeartsView : MonoBehaviour
    {
        [SerializeField] private HudConfig _config;
        [SerializeField] private PlayerHealth _player;
        [Tooltip("One image per heart, left to right.")]
        [SerializeField] private Image[] _hearts;
        [SerializeField] private Sprite _full;
        [SerializeField] private Sprite _empty;

        private int _current = -1;
        private int _before;
        private int _max;
        private float _sinceHit = -1f;
        private bool _shownRed;

        /// <summary>Hearts currently shown as full.</summary>
        public int ShownHearts => _current;

        /// <summary>True while the hearts blink red after a lost heart.</summary>
        public bool IsFlashing => _config != null && _sinceHit >= 0f && _sinceHit < _config.HeartFlashDuration;

        private void OnEnable()
        {
            if (_player != null)
            {
                _player.HealthChanged += OnHealthChanged;
                if (_player.Max > 0)
                {
                    OnHealthChanged(_player.Current, _player.Max);
                }
            }
        }

        private void OnDisable()
        {
            if (_player != null)
            {
                _player.HealthChanged -= OnHealthChanged;
            }
        }

        private void Update()
        {
            if (_sinceHit < 0f || _config == null)
            {
                return;
            }

            // Unscaled, so the blinking also ends while a hitstop or the pause holds the game time.
            _sinceHit += Time.unscaledDeltaTime;
            bool red = HudMath.IsFlashRed(_sinceHit, _config.HeartFlashDuration, _config.HeartFlashInterval);
            if (_sinceHit >= _config.HeartFlashDuration)
            {
                _sinceHit = -1f;
                red = false;
            }

            if (red != _shownRed || _sinceHit < 0f)
            {
                Show(red);
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            // Hits during a running blink keep the hearts from before the first hit blinking.
            if (_current >= 0 && current < _current)
            {
                _before = IsFlashing ? _before : _current;
                _sinceHit = 0f;
            }

            _current = current;
            _max = max;
            Show(IsFlashing);
        }

        private void Show(bool red)
        {
            _shownRed = red;
            Color tint = red && _config != null ? _config.HeartFlashColor : Color.white;
            for (int i = 0; i < _hearts.Length; i++)
            {
                Image heart = _hearts[i];
                heart.enabled = i < _max;
                bool lost = i >= _current;
                heart.sprite = HudMath.IsHeartFullWhileFlashing(i, _current, _before, red) ? _full : _empty;
                heart.color = lost ? tint : Color.white;
            }
        }
    }
}
