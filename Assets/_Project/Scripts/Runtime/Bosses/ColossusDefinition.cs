using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>Values of the Koloss (issue #151, decisions L7, C7): times and speeds of its charge.</summary>
    [CreateAssetMenu(fileName = "ColossusDefinition", menuName = "RushOps/Colossus Definition")]
    public class ColossusDefinition : BossDefinition
    {
        [Header("Charge")]
        [Tooltip("Seconds of chasing before the next charge up.")]
        [SerializeField, Min(0f)] private float _chaseTime = 2.5f;

        [Tooltip("Seconds the charge up is visible before the rush (L7).")]
        [SerializeField, Min(0f)] private float _windupTime = 1f;

        [Tooltip("Speed of the rush in units per second.")]
        [SerializeField, Min(0f)] private float _chargeSpeed = 11f;

        [Tooltip("The rush ends after this many seconds even without a wall.")]
        [SerializeField, Min(0.1f)] private float _maxChargeTime = 2.5f;

        [Tooltip("Seconds dazed after hitting a wall (C7).")]
        [SerializeField, Min(0f)] private float _stunTime = 1.5f;

        [Tooltip("Length of the floor line in units if no wall is in the way.")]
        [SerializeField, Min(1f)] private float _lineLength = 14f;

        /// <summary>Seconds of chasing before charging up.</summary>
        public float ChaseTime => _chaseTime;

        /// <summary>Seconds of visible charge up.</summary>
        public float WindupTime => _windupTime;

        /// <summary>Rush speed in units per second.</summary>
        public float ChargeSpeed => _chargeSpeed;

        /// <summary>Longest rush in seconds.</summary>
        public float MaxChargeTime => _maxChargeTime;

        /// <summary>Seconds dazed after a wall.</summary>
        public float StunTime => _stunTime;

        /// <summary>Longest floor line in units.</summary>
        public float LineLength => _lineLength;
    }
}
