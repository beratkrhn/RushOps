using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>Values of the gates between zones (F-LVL-02).</summary>
    [CreateAssetMenu(fileName = "GateConfig", menuName = "RushOps/Gate Config")]
    public class GateConfig : ScriptableObject
    {
        [Tooltip("Seconds the opening animation takes; the gate blocks until it ends.")]
        [SerializeField, Min(0f)] private float _openDuration = 0.6f;

        [Tooltip("Seconds the bars shake before they sink, so the opening catches the eye.")]
        [SerializeField, Min(0f)] private float _shakeDuration = 0.15f;

        [Tooltip("Sideways shake of the bars in units.")]
        [SerializeField, Min(0f)] private float _shakeAmplitude = 0.04f;

        [Tooltip("Volume of the opening sound.")]
        [SerializeField, Range(0f, 1f)] private float _openVolume = 1f;

        /// <summary>Seconds the opening takes.</summary>
        public float OpenDuration => _openDuration;

        /// <summary>Seconds the bars shake at the start of the opening.</summary>
        public float ShakeDuration => _shakeDuration;

        /// <summary>Sideways shake of the bars in units.</summary>
        public float ShakeAmplitude => _shakeAmplitude;

        /// <summary>Volume of the opening sound.</summary>
        public float OpenVolume => _openVolume;
    }
}
