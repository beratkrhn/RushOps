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

        [Tooltip("Volume of the collapse sound (issue #290).")]
        [SerializeField, Range(0f, 1f)] private float _volume = 0.9f;

        [Tooltip("Height in tiles the hint bubble floats above the wall, and how far it bobs.")]
        [SerializeField] private Vector2 _hintHeightAndBob = new Vector2(1.6f, 0.12f);

        /// <summary>Tiles a boomer explosion may be away from the wall beyond its own radius.</summary>
        public float ExtraReach => _extraReach;

        /// <summary>Seconds between two boomers the zone sends while the wall stands.</summary>
        public float BoomerInterval => _boomerInterval;

        /// <summary>Most boomers the zone sends for the wall at once.</summary>
        public int MaxBoomers => _maxBoomers;

        /// <summary>Volume of the collapse sound.</summary>
        public float Volume => _volume;

        /// <summary>Height of the hint bubble above the wall.</summary>
        public float HintHeight => _hintHeightAndBob.x;

        /// <summary>How far the hint bubble bobs up and down.</summary>
        public float HintBob => _hintHeightAndBob.y;
    }
}
