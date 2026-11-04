using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>Values of the timed switches (F-RAE-03 to F-RAE-05, issue #32).</summary>
    [CreateAssetMenu(fileName = "TimedSwitchConfig", menuName = "RushOps/Timed Switch Config")]
    public class TimedSwitchConfig : ScriptableObject
    {
        [Tooltip("Seconds from the first pressed switch to press all of them.")]
        [SerializeField, Min(0f)] private float _timeLimit = 12f;

        [Tooltip("Text in the mission tracker, followed by the pressed count, e.g. „Schalter 1/3“.")]
        [SerializeField] private string _text = "Alle Schalter betreten";

        [Header("Feedback")]
        [Tooltip("Seconds the switches blink red after the timer ran out.")]
        [SerializeField, Min(0f)] private float _resetFlashDuration = 0.8f;

        [Tooltip("Seconds per on or off phase of the red blinking.")]
        [SerializeField, Min(0.01f)] private float _resetFlashInterval = 0.1f;

        [Tooltip("Tint of the switches while they blink after a reset.")]
        [SerializeField] private Color _resetColor = new Color(1f, 0.3f, 0.3f, 1f);

        [SerializeField, Range(0f, 1f)] private float _pressVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _resetVolume = 0.9f;

        /// <summary>Seconds from the first press to press all switches.</summary>
        public float TimeLimit => _timeLimit;

        /// <summary>Text in the mission tracker.</summary>
        public string Text => _text;

        /// <summary>Seconds the switches blink red after a reset.</summary>
        public float ResetFlashDuration => _resetFlashDuration;

        /// <summary>Seconds per phase of the red blinking.</summary>
        public float ResetFlashInterval => _resetFlashInterval;

        /// <summary>Tint while blinking after a reset.</summary>
        public Color ResetColor => _resetColor;

        /// <summary>Volume of the press sound.</summary>
        public float PressVolume => _pressVolume;

        /// <summary>Volume of the reset sound.</summary>
        public float ResetVolume => _resetVolume;
    }
}
