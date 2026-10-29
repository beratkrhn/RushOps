using UnityEngine;

namespace RushOps.Navigation
{
    /// <summary>Values of the enemy navigation (ADR 0003).</summary>
    [CreateAssetMenu(fileName = "NavigationConfig", menuName = "RushOps/Navigation Config")]
    public class NavigationConfig : ScriptableObject
    {
        [Tooltip("Minimum seconds between two flow field computations; a new one starts when the target changes cell.")]
        [SerializeField, Min(0f)] private float _recomputeInterval = 0.1f;

        [Tooltip("Enemies closer than this distance in units push each other apart.")]
        [SerializeField, Min(0f)] private float _separationRadius = 0.9f;

        [Tooltip("Strength of the separation push relative to the path direction.")]
        [SerializeField, Min(0f)] private float _separationWeight = 1.2f;

        [Tooltip("Within this path cost of the target (10 = one cell) enemies steer straight at it instead of along the field.")]
        [SerializeField, Min(0)] private int _directChaseDistance = 14;

        /// <summary>Minimum seconds between two flow field computations.</summary>
        public float RecomputeInterval => _recomputeInterval;

        /// <summary>Distance in units below which enemies push each other apart.</summary>
        public float SeparationRadius => _separationRadius;

        /// <summary>Strength of the separation push relative to the path direction.</summary>
        public float SeparationWeight => _separationWeight;

        /// <summary>Path cost to the target below which enemies steer straight at it.</summary>
        public int DirectChaseDistance => _directChaseDistance;
    }
}
