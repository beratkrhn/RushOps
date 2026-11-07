using RushOps.Loot;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Gold of the current level or run below score and multiplier (F-GLD-04, F-HUD-04, issue #235). Thin adapter on the
    /// <see cref="GoldWallet"/>: the text changes only on its <see cref="GoldWallet.BalanceChanged"/> event, and a pickup
    /// lights it up in the flash color for a moment.
    /// </summary>
    public class GoldView : MonoBehaviour
    {
        [SerializeField] private HudConfig _config;
        [SerializeField] private GoldCollector _collector;
        [SerializeField] private Text _label;

        private GoldWallet _wallet;
        private Color _color = Color.white;
        private float _sinceGain = -1f;
        private int _shown;

        /// <summary>Gold text shown.</summary>
        public string TextShown => _label != null ? _label.text : string.Empty;

        /// <summary>True while the display lights up after a pickup.</summary>
        public bool IsFlashing => _sinceGain >= 0f;

        private void Awake()
        {
            if (_label != null)
            {
                _color = _label.color;
            }
        }

        private void OnEnable()
        {
            _wallet = _collector != null ? _collector.Wallet : null;
            if (_wallet == null)
            {
                return;
            }

            _wallet.BalanceChanged += OnBalanceChanged;
            Show(_wallet.EarnedThisRun);
        }

        private void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.BalanceChanged -= OnBalanceChanged;
            }

            EndFlash();
        }

        private void Update()
        {
            if (_sinceGain < 0f)
            {
                return;
            }

            // Real time, so the hitstop of the kill does not hold the flash.
            _sinceGain += Time.unscaledDeltaTime;
            if (_sinceGain >= (_config != null ? _config.MultiplierFlashDuration : 0f))
            {
                EndFlash();
            }
        }

        private void OnBalanceChanged(int balance)
        {
            int gold = _wallet.EarnedThisRun;
            bool gained = gold > _shown;
            Show(gold);
            if (gained && _label != null && _config != null)
            {
                _sinceGain = 0f;
                _label.color = _config.MultiplierFlashColor;
            }
        }

        private void Show(int gold)
        {
            _shown = gold;
            if (_label != null)
            {
                _label.text = ScoreText.Gold(gold);
            }
        }

        private void EndFlash()
        {
            _sinceGain = -1f;
            if (_label != null)
            {
                _label.color = _color;
            }
        }
    }
}
