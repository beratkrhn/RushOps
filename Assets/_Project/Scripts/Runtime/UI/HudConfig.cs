using UnityEngine;

namespace RushOps.UI
{
    /// <summary>Values of the HUD (F-HUD-01, F-HUD-03).</summary>
    [CreateAssetMenu(fileName = "HudConfig", menuName = "RushOps/HUD Config")]
    public class HudConfig : ScriptableObject
    {
        [Tooltip("Seconds the hearts blink red after the player lost one.")]
        [SerializeField, Min(0f)] private float _heartFlashDuration = 0.6f;

        [Tooltip("Seconds per on or off phase of the red blinking.")]
        [SerializeField, Min(0.01f)] private float _heartFlashInterval = 0.1f;

        [Tooltip("Tint of the hearts during the red phases.")]
        [SerializeField] private Color _heartFlashColor = new Color(1f, 0.25f, 0.3f, 1f);

        [Tooltip("Seconds the mission tracker shows „Tor offen!“ before it hides.")]
        [SerializeField, Min(0f)] private float _missionDoneDuration = 2.5f;

        [Tooltip("Volume of the chime when a tracked gate opens.")]
        [SerializeField, Range(0f, 1f)] private float _missionDoneVolume = 1f;

        /// <summary>Seconds the hearts blink red after a lost heart.</summary>
        public float HeartFlashDuration => _heartFlashDuration;

        /// <summary>Seconds per on or off phase of the red blinking.</summary>
        public float HeartFlashInterval => _heartFlashInterval;

        /// <summary>Tint of the hearts during the red phases.</summary>
        public Color HeartFlashColor => _heartFlashColor;

        /// <summary>Seconds the mission tracker shows „Tor offen!“ before it hides.</summary>
        public float MissionDoneDuration => _missionDoneDuration;

        /// <summary>Volume of the chime when a tracked gate opens.</summary>
        public float MissionDoneVolume => _missionDoneVolume;
    }
}
