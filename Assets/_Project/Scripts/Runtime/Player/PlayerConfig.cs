using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Balancing values for the player character.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "RushOps/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Tooltip("Top speed in units (tiles) per second at full stick deflection.")]
        [SerializeField, Min(0f)] private float _moveSpeed = 5f;

        [Tooltip("How fast the speed rises towards the stick's target speed, in units per second squared.")]
        [SerializeField, Min(0f)] private float _acceleration = 40f;

        [Tooltip("How fast the speed falls when the stick is released or eased off, in units per second squared.")]
        [SerializeField, Min(0f)] private float _deceleration = 50f;

        /// <summary>Top speed in units per second at full stick deflection.</summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>Speed gain towards the target speed in units per second squared.</summary>
        public float Acceleration => _acceleration;

        /// <summary>Speed loss towards a slower target speed in units per second squared.</summary>
        public float Deceleration => _deceleration;
    }
}
