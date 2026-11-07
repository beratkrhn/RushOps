using UnityEngine;

namespace RushOps.Loot
{
    /// <summary>Gold coins (F-GLD-01 to F-GLD-05): points, life on the floor, magnet and scatter. All values are starting values.</summary>
    [CreateAssetMenu(fileName = "GoldConfig", menuName = "RushOps/Gold Config")]
    public class GoldConfig : ScriptableObject
    {
        [Tooltip("Score points per coin, times the current multiplier.")]
        [SerializeField, Min(0)] private int _pointsPerCoin = 25;

        [Tooltip("Seconds a coin stays on the floor before it vanishes.")]
        [SerializeField, Min(0f)] private float _lifetime = 8f;

        [Tooltip("Seconds before the end during which a coin blinks.")]
        [SerializeField, Min(0f)] private float _blinkDuration = 2f;

        [Tooltip("Distance in units (tiles) within which a coin flies to the player.")]
        [SerializeField, Min(0f)] private float _magnetRange = 1f;

        [Tooltip("Speed in units per second of a coin flying to the player.")]
        [SerializeField, Min(0f)] private float _flightSpeed = 8f;

        [Tooltip("Radius in units around the death position within which coins are scattered.")]
        [SerializeField, Min(0f)] private float _scatterRadius = 0.5f;

        [Tooltip("Picking up a coin restarts the combo window.")]
        [SerializeField] private bool _restartsComboWindow = true;

        /// <summary>Score points per coin before the multiplier.</summary>
        public int PointsPerCoin => _pointsPerCoin;

        /// <summary>Seconds a coin stays on the floor.</summary>
        public float Lifetime => _lifetime;

        /// <summary>Seconds before the end during which a coin blinks.</summary>
        public float BlinkDuration => _blinkDuration;

        /// <summary>Magnet range in units.</summary>
        public float MagnetRange => _magnetRange;

        /// <summary>Flight speed to the player in units per second.</summary>
        public float FlightSpeed => _flightSpeed;

        /// <summary>Scatter radius around the death position in units.</summary>
        public float ScatterRadius => _scatterRadius;

        /// <summary>True if picking up a coin restarts the combo window.</summary>
        public bool RestartsComboWindow => _restartsComboWindow;
    }
}
