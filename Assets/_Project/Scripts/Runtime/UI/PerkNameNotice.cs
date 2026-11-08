using RushOps.Feel;
using RushOps.Perks;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Flashes the short name of a perk when the player collects it, e.g. "Begleiter" or "Strohmann" (issue #247).
    /// The text fades out over <see cref="HudConfig.PerkNoticeDuration"/> seconds of real time.
    /// </summary>
    public class PerkNameNotice : MonoBehaviour
    {
        [SerializeField] private PlayerPerks _perks;
        [SerializeField] private HudConfig _config;
        [Tooltip("Text that shows the name; hidden while no notice runs.")]
        [SerializeField] private Text _label;

        private float _elapsed = -1f;

        /// <summary>Opacity of the name, 0 while hidden.</summary>
        public float CurrentAlpha => _label != null && _label.enabled ? _label.color.a : 0f;

        /// <summary>Text currently shown.</summary>
        public string Shown => _label != null ? _label.text : string.Empty;

        private void Awake()
        {
            if (_label != null)
            {
                _label.enabled = false;
            }
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
            if (_label == null || _config == null || perk == null || string.IsNullOrEmpty(perk.DisplayName))
            {
                return;
            }

            _label.text = perk.DisplayName;
            _elapsed = 0f;
            SetAlpha(1f);
        }

        private void Update()
        {
            if (_elapsed < 0f)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            float alpha = FeelMath.FlashFade(_elapsed, _config.PerkNoticeDuration, 1f);
            if (alpha <= 0f)
            {
                _elapsed = -1f;
            }

            SetAlpha(alpha);
        }

        private void SetAlpha(float alpha)
        {
            Color color = _label.color;
            color.a = alpha;
            _label.color = color;
            _label.enabled = alpha > 0f;
        }
    }
}
