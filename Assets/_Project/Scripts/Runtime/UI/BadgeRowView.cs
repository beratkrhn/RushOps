using RushOps.Save;
using RushOps.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// The three badges of the level on „Level geschafft!“ (issue #288, F-SCO-05, N25): earned ones bright, missing ones
    /// dark; the ones new with this try pulse. Filled by <see cref="LevelProgressRecorder.BadgesRecorded"/>. The game is
    /// frozen while the screen shows, so the pulse runs on unscaled time.
    /// </summary>
    public class BadgeRowView : MonoBehaviour
    {
        [SerializeField] private LevelProgressRecorder _recorder;
        [SerializeField] private BadgeConfig _config;
        [Tooltip("One image per badge: Unversehrt, Blitz, Kombo.")]
        [SerializeField] private Image[] _icons = new Image[BadgeRules.PerLevel];

        private float _pulseStart;

        /// <summary>Badges shown as earned.</summary>
        public BadgeKind Shown { get; private set; }

        /// <summary>Badges shown as new.</summary>
        public BadgeKind ShownNew { get; private set; }

        /// <summary>Shows <paramref name="all"/> as earned and lets <paramref name="fresh"/> pulse.</summary>
        public void Show(BadgeKind all, BadgeKind fresh)
        {
            Shown = all;
            ShownNew = fresh;
            _pulseStart = Time.unscaledTime;
            for (int i = 0; i < _icons.Length && i < BadgeRules.Order.Length; i++)
            {
                if (_icons[i] == null)
                {
                    continue;
                }

                BadgeKind badge = BadgeRules.Order[i];
                if (_config != null)
                {
                    _icons[i].sprite = _config.Icon(badge, BadgeRules.Has(all, badge));
                }

                _icons[i].rectTransform.localScale = Vector3.one;
            }
        }

        private void Awake()
        {
            Show(BadgeKind.None, BadgeKind.None);
        }

        private void OnEnable()
        {
            if (_recorder != null)
            {
                _recorder.BadgesRecorded += Show;
            }
        }

        private void OnDisable()
        {
            if (_recorder != null)
            {
                _recorder.BadgesRecorded -= Show;
            }
        }

        private void Update()
        {
            if (ShownNew == BadgeKind.None || _config == null)
            {
                return;
            }

            float scale = BadgeRules.PulseScale(Time.unscaledTime - _pulseStart, _config.NewPulseSeconds, _config.NewPulseScale);
            var size = new Vector3(scale, scale, 1f);
            for (int i = 0; i < _icons.Length && i < BadgeRules.Order.Length; i++)
            {
                if (_icons[i] != null && BadgeRules.Has(ShownNew, BadgeRules.Order[i]))
                {
                    _icons[i].rectTransform.localScale = size;
                }
            }
        }
    }
}
