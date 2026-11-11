using RushOps.Save;
using RushOps.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// The three badge conditions of the level in the pause overlay (issue #288, F-SCO-05, N25): icon bright if the
    /// badge is already saved, text with the condition („Blitz: unter 4:30“) in white while the running try can still
    /// earn it and dimmed once it cannot. Refreshed whenever the overlay opens.
    /// </summary>
    public class BadgeListView : MonoBehaviour
    {
        [SerializeField] private LevelBadges _badges;
        [SerializeField] private LevelProgressRecorder _recorder;
        [Tooltip("One image per badge: Unversehrt, Blitz, Kombo.")]
        [SerializeField] private Image[] _icons = new Image[BadgeRules.PerLevel];
        [Tooltip("One text per badge: Unversehrt, Blitz, Kombo.")]
        [SerializeField] private Text[] _labels = new Text[BadgeRules.PerLevel];
        [SerializeField] private Color _possibleColor = Color.white;
        [SerializeField] private Color _lostColor = Color.gray;

        /// <summary>Badges shown as saved.</summary>
        public BadgeKind Saved { get; private set; }

        /// <summary>Text shown for <paramref name="badge"/>; empty if unknown.</summary>
        public string TextOf(BadgeKind badge)
        {
            int i = BadgeRules.IndexOf(badge);
            return i >= 0 && i < _labels.Length && _labels[i] != null ? _labels[i].text : string.Empty;
        }

        /// <summary>True if <paramref name="badge"/> shows as still possible in this try.</summary>
        public bool ShowsPossible(BadgeKind badge)
        {
            int i = BadgeRules.IndexOf(badge);
            return i >= 0 && i < _labels.Length && _labels[i] != null && _labels[i].color == _possibleColor;
        }

        /// <summary>Shows the saved badges and the state of the running try.</summary>
        public void Refresh()
        {
            Saved = _recorder != null ? _recorder.SavedBadges : BadgeKind.None;
            BadgeConfig config = _badges != null ? _badges.Config : null;
            float target = _badges != null ? _badges.TargetSeconds : 0f;
            int combo = _badges != null ? _badges.ComboTarget : 5;
            BadgeTracker tracker = _badges != null ? _badges.Tracker : null;
            for (int i = 0; i < BadgeRules.Order.Length; i++)
            {
                BadgeKind badge = BadgeRules.Order[i];
                bool saved = BadgeRules.Has(Saved, badge);
                if (i < _icons.Length && _icons[i] != null && config != null)
                {
                    _icons[i].sprite = config.Icon(badge, saved);
                }

                if (i < _labels.Length && _labels[i] != null)
                {
                    _labels[i].text = BadgeText.Condition(badge, target, combo);
                    bool possible = tracker == null
                        || BadgeText.StillPossible(badge, tracker.HeartsLost, tracker.Seconds, target);
                    _labels[i].color = possible ? _possibleColor : _lostColor;
                }
            }
        }

        private void OnEnable()
        {
            Refresh();
        }
    }
}
