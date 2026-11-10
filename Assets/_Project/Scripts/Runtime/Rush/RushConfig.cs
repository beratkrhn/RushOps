using UnityEngine;

namespace RushOps.Rush
{
    /// <summary>
    /// Values of the rush meter (F-RSH-01 to F-RSH-05, decisions RM2, RM4, RM6, N8). All are start values;
    /// <see cref="RushMeter"/> and <see cref="RushRules"/> apply them.
    /// </summary>
    [CreateAssetMenu(fileName = "RushConfig", menuName = "RushOps/Rush Config")]
    public class RushConfig : ScriptableObject
    {
        [Header("Meter")]
        [Tooltip("Fill at which the meter is full; every kill adds the current combo multiplier (RM4, N15).")]
        [SerializeField, Min(1)] private int _killsToFill = 25;

        [Tooltip("Seconds a triggered rush lasts (RM2).")]
        [SerializeField, Min(0f)] private float _duration = 5f;

        [Header("Effect")]
        [Tooltip("Speed factor of normal enemies and their timers during the rush (RM2).")]
        [SerializeField, Range(0f, 1f)] private float _enemySlowFactor = 0.5f;

        [Tooltip("Speed factor of bosses and their timers during the rush; weaker than for normal enemies (RM6).")]
        [SerializeField, Range(0f, 1f)] private float _bossSlowFactor = 0.75f;

        [Tooltip("Factor on every damage the player deals during the rush; multiplies with double damage (N8, N17).")]
        [SerializeField, Min(1f)] private float _damageFactor = 1.5f;

        [Header("Feedback")]
        [Tooltip("Colour of the screen tint while the rush runs; a palette colour.")]
        [SerializeField] private Color _tintColor = new Color32(232, 134, 47, 255);

        [Tooltip("Opacity of the screen tint while the rush runs.")]
        [SerializeField, Range(0f, 1f)] private float _tintAlpha = 0.16f;

        [Tooltip("Opacity of the frame along the screen edges while the rush runs, so the rush reads at a glance.")]
        [SerializeField, Range(0f, 1f)] private float _edgeAlpha = 0.6f;

        [Tooltip("Last seconds of the rush in which the tint blinks as a warning.")]
        [SerializeField, Min(0f)] private float _warningTime = 1f;

        [Tooltip("Seconds per on or off phase of the warning blink.")]
        [SerializeField, Min(0.01f)] private float _blinkInterval = 0.1f;

        [Tooltip("Sound when the rush starts; none if empty.")]
        [SerializeField] private AudioClip _startClip;

        [Tooltip("Sound when the rush ends; none if empty.")]
        [SerializeField] private AudioClip _endClip;

        [Tooltip("Sound once when the meter becomes full; none if empty.")]
        [SerializeField] private AudioClip _fullClip;

        [SerializeField, Range(0f, 1f)] private float _fullVolume = 0.6f;

        [Header("Button")]
        [Tooltip("Largest scale of the full rush button while it pulses.")]
        [SerializeField, Min(1f)] private float _pulseScale = 1.12f;

        [Tooltip("Seconds of one pulse of the full rush button.")]
        [SerializeField, Min(0.01f)] private float _pulsePeriod = 0.6f;

        [SerializeField, Range(0f, 1f)] private float _startVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _endVolume = 0.6f;

        /// <summary>Fill at which the meter is full, at least 1.</summary>
        public int KillsToFill => Mathf.Max(1, _killsToFill);

        /// <summary>Seconds a triggered rush lasts.</summary>
        public float Duration => _duration;

        /// <summary>Speed factor of normal enemies during the rush.</summary>
        public float EnemySlowFactor => _enemySlowFactor;

        /// <summary>Speed factor of bosses during the rush.</summary>
        public float BossSlowFactor => _bossSlowFactor;

        /// <summary>Factor on the player's damage during the rush.</summary>
        public float DamageFactor => _damageFactor;

        /// <summary>Colour of the screen tint while the rush runs.</summary>
        public Color TintColor => _tintColor;

        /// <summary>Opacity of the screen tint while the rush runs.</summary>
        public float TintAlpha => _tintAlpha;

        /// <summary>Opacity of the frame along the screen edges while the rush runs.</summary>
        public float EdgeAlpha => _edgeAlpha;

        /// <summary>Last seconds of the rush in which the tint blinks.</summary>
        public float WarningTime => _warningTime;

        /// <summary>Seconds per on or off phase of the warning blink.</summary>
        public float BlinkInterval => _blinkInterval;

        /// <summary>Sound when the rush starts.</summary>
        public AudioClip StartClip => _startClip;

        /// <summary>Sound when the rush ends.</summary>
        public AudioClip EndClip => _endClip;

        /// <summary>Sound once when the meter becomes full.</summary>
        public AudioClip FullClip => _fullClip;

        /// <summary>Volume of <see cref="FullClip"/>.</summary>
        public float FullVolume => _fullVolume;

        /// <summary>Largest scale of the full rush button while it pulses.</summary>
        public float PulseScale => _pulseScale;

        /// <summary>Seconds of one pulse of the full rush button.</summary>
        public float PulsePeriod => _pulsePeriod;

        /// <summary>Volume of <see cref="StartClip"/>.</summary>
        public float StartVolume => _startVolume;

        /// <summary>Volume of <see cref="EndClip"/>.</summary>
        public float EndVolume => _endVolume;
    }
}
