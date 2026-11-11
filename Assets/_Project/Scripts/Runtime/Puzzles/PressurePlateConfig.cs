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

        [Tooltip("Radius in tiles around the plate's center in which a brute's feet press it (issue #289).")]
        [SerializeField, Min(0.1f)] private float _radius = 0.8f;

        [Tooltip("Volume of the sink and lock sounds.")]
        [SerializeField, Range(0f, 1f)] private float _volume = 0.8f;

        [Tooltip("Height in tiles the hint bubble floats above the plate, and how far it bobs.")]
        [SerializeField] private Vector2 _hintHeightAndBob = new Vector2(1.4f, 0.12f);

        /// <summary>Seconds a brute has to stand on the plate until it locks.</summary>
        public float HoldSeconds => _holdSeconds;

        /// <summary>Seconds between two brutes the zone sends while the plate is not locked.</summary>
        public float BruteInterval => _bruteInterval;

        /// <summary>Most brutes the zone sends for the plate at once.</summary>
        public int MaxBrutes => _maxBrutes;

        /// <summary>Radius in tiles in which a brute's feet press the plate.</summary>
        public float Radius => _radius;

        /// <summary>Volume of the sink and lock sounds.</summary>
        public float Volume => _volume;

        /// <summary>Height of the hint bubble above the plate.</summary>
        public float HintHeight => _hintHeightAndBob.x;

        /// <summary>How far the hint bubble bobs up and down.</summary>
        public float HintBob => _hintHeightAndBob.y;
    }
}
