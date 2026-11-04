using RushOps.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Score and multiplier at the top center of the HUD (F-HUD-04, issue #28). The texts change only when the values
    /// do; when the multiplier rises it lights up in the flash color and pops for a moment.
    /// </summary>
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private HudConfig _config;
        [SerializeField] private LevelScore _score;
        [SerializeField] private Text _scoreLabel;
        [SerializeField] private Text _multiplierLabel;

        private ScoreKeeper _keeper;
        private Color _multiplierColor = Color.white;
        private Vector3 _multiplierScale = Vector3.one;
        private float _sinceRise = -1f;
        private int _shownMultiplier = 1;

        /// <summary>Score text shown.</summary>
        public string ScoreTextShown => _scoreLabel != null ? _scoreLabel.text : string.Empty;

        /// <summary>Multiplier text shown.</summary>
        public string MultiplierTextShown => _multiplierLabel != null ? _multiplierLabel.text : string.Empty;

        /// <summary>True while the multiplier lights up after a rise.</summary>
        public bool IsFlashing => _sinceRise >= 0f;

        private void Awake()
        {
            if (_multiplierLabel != null)
            {
                _multiplierColor = _multiplierLabel.color;
                _multiplierScale = _multiplierLabel.rectTransform.localScale;
            }
        }

        private void OnEnable()
        {
            _keeper = _score != null ? _score.Keeper : null;
            if (_keeper == null)
            {
                return;
            }

            _keeper.ScoreChanged += OnScoreChanged;
            _keeper.MultiplierChanged += OnMultiplierChanged;
            OnScoreChanged(_keeper.Score);
            SetMultiplierText(_keeper.Multiplier);
        }

        private void OnDisable()
        {
            if (_keeper != null)
            {
                _keeper.ScoreChanged -= OnScoreChanged;
                _keeper.MultiplierChanged -= OnMultiplierChanged;
            }

            EndFlash();
        }

        private void Update()
        {
            if (_sinceRise < 0f || _multiplierLabel == null)
            {
                return;
            }

            // Real time, so the hitstop of the kill that raised the multiplier does not hold the flash.
            _sinceRise += Time.unscaledDeltaTime;
            float duration = _config != null ? _config.MultiplierFlashDuration : 0f;
            if (_sinceRise >= duration)
            {
                EndFlash();
                return;
            }

            float peak = _config != null ? _config.MultiplierFlashScale : 1f;
            _multiplierLabel.rectTransform.localScale = _multiplierScale * ScoreText.PulseScale(_sinceRise, duration, peak);
        }

        private void OnScoreChanged(int score)
        {
            if (_scoreLabel != null)
            {
                _scoreLabel.text = ScoreText.Score(score);
            }
        }

        private void OnMultiplierChanged(int multiplier)
        {
            int previous = _shownMultiplier;
            SetMultiplierText(multiplier);
            if (multiplier > previous && _multiplierLabel != null && _config != null)
            {
                _sinceRise = 0f;
                _multiplierLabel.color = _config.MultiplierFlashColor;
                _multiplierLabel.rectTransform.localScale = _multiplierScale * _config.MultiplierFlashScale;
            }
            else if (multiplier < previous)
            {
                EndFlash();
            }
        }

        private void SetMultiplierText(int multiplier)
        {
            _shownMultiplier = multiplier;
            if (_multiplierLabel != null)
            {
                _multiplierLabel.text = ScoreText.Multiplier(multiplier);
            }
        }

        private void EndFlash()
        {
            _sinceRise = -1f;
            if (_multiplierLabel != null)
            {
                _multiplierLabel.color = _multiplierColor;
                _multiplierLabel.rectTransform.localScale = _multiplierScale;
            }
        }
    }
}
