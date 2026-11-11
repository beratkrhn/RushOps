using UnityEngine;

namespace RushOps.Feel
{
    /// <summary>Values of the vibration (issue #300, F-AUD-05, N32).</summary>
    [CreateAssetMenu(fileName = "HapticConfig", menuName = "RushOps/Haptic Config")]
    public class HapticConfig : ScriptableObject
    {
        [Tooltip("Shortest time between two pulses in seconds, so chains of explosions do not buzz without pause.")]
        [SerializeField, Min(0f)] private float _minGapSeconds = 0.1f;

        [Tooltip("Explosions further than this outside the camera view do not vibrate (units).")]
        [SerializeField, Min(0f)] private float _screenMargin = 1f;

        /// <summary>Shortest time between two pulses in seconds.</summary>
        public float MinGapSeconds => _minGapSeconds;

        /// <summary>Margin around the camera view in which explosions still vibrate.</summary>
        public float ScreenMargin => _screenMargin;
    }
}
