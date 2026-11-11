using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>Values of the pressure plate (issue #287, F-RAE-09, F-RAE-10, N26).</summary>
    [CreateAssetMenu(fileName = "PressurePlateConfig", menuName = "RushOps/Pressure Plate Config")]
    public class PressurePlateConfig : ScriptableObject
    {
        [Tooltip("Seconds a brute has to stand on the plate until it locks.")]
        [SerializeField, Min(0f)] private float _holdSeconds = 1.5f;

        [Tooltip("Seconds between two brutes the zone sends while the plate is not locked.")]
        [SerializeField, Min(0.5f)] private float _bruteInterval = 12f;

        [Tooltip("Most brutes the zone sends for the plate at once.")]
        [SerializeField, Min(1)] private int _maxBrutes = 2;

        /// <summary>Seconds a brute has to stand on the plate until it locks.</summary>
        public float HoldSeconds => _holdSeconds;

        /// <summary>Seconds between two brutes the zone sends while the plate is not locked.</summary>
        public float BruteInterval => _bruteInterval;

        /// <summary>Most brutes the zone sends for the plate at once.</summary>
        public int MaxBrutes => _maxBrutes;
    }
}
