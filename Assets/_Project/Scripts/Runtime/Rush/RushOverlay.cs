using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Rush
{
    /// <summary>
    /// Screen feedback while the rush runs (issue #270): a light full-screen tint and a stronger frame along the screen
    /// edges in a palette colour. Both blink in the last second as a warning and disappear when the rush ends. Thin
    /// adapter on <see cref="PlayerRush"/>; colour, opacities and blink come from the <see cref="RushConfig"/>.
    /// </summary>
    public class RushOverlay : MonoBehaviour
    {
        [SerializeField] private PlayerRush _rush;
        [Tooltip("Covers the whole screen; never catches touches.")]
        [SerializeField] private Image _tint;
        [Tooltip("Bars along the screen edges; never catch touches.")]
        [SerializeField] private Image[] _edges = System.Array.Empty<Image>();

        private bool _visible;

        /// <summary>True while the tint shows.</summary>
        public bool IsVisible => _visible;

        private void Awake()
        {
            Show(false);
        }

        private void Update()
        {
            if (_rush == null)
            {
                return;
            }

            RushConfig config = _rush.Config;
            RushMeter meter = _rush.Meter;
            bool visible = meter.IsActive && config != null
                && RushRules.IsTintVisible(meter.Remaining, config.WarningTime, config.BlinkInterval);
            if (visible == _visible)
            {
                return;
            }

            if (visible)
            {
                Paint(_tint, config.TintColor, config.TintAlpha);
                foreach (Image edge in _edges)
                {
                    Paint(edge, config.TintColor, config.EdgeAlpha);
                }
            }

            Show(visible);
        }

        private void Show(bool visible)
        {
            _visible = visible;
            Enable(_tint, visible);
            foreach (Image edge in _edges)
            {
                Enable(edge, visible);
            }
        }

        private static void Paint(Image image, Color color, float alpha)
        {
            if (image != null)
            {
                color.a = alpha;
                image.color = color;
            }
        }

        private static void Enable(Image image, bool enabled)
        {
            if (image != null)
            {
                image.raycastTarget = false;
                image.enabled = enabled;
            }
        }
    }
}
