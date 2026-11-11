using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>Values of the cracked wall (issue #287, F-RAE-11, F-RAE-12, N27).</summary>
    [CreateAssetMenu(fileName = "CrackedWallConfig", menuName = "RushOps/Cracked Wall Config")]
    public class CrackedWallConfig : ScriptableObject
    {
        [Tooltip("Tiles a boomer explosion may be away from the wall beyond its own radius and still break it.")]
        [SerializeField, Min(0f)] private float _extraReach = 0.5f;

        [Tooltip("Seconds between two boomers the zone sends while the wall stands.")]
        [SerializeField, Min(0.5f)] private float _boomerInterval = 10f;

        [Tooltip("Most boomers the zone sends for the wall at once.")]
        [SerializeField, Min(1)] private int _maxBoomers = 2;

        /// <summary>Tiles a boomer explosion may be away from the wall beyond its own radius.</summary>
        public float ExtraReach => _extraReach;

        /// <summary>Seconds between two boomers the zone sends while the wall stands.</summary>
        public float BoomerInterval => _boomerInterval;

        /// <summary>Most boomers the zone sends for the wall at once.</summary>
        public int MaxBoomers => _maxBoomers;
    }
}
