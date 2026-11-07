using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>Start values of the companion perk, a combat drone (F-PRK-05, F-PRK-06, issue #244, K1 to K3).</summary>
    [CreateAssetMenu(fileName = "CompanionConfig", menuName = "RushOps/Companion Config")]
    public class CompanionConfig : ScriptableObject
    {
        [Tooltip("Seconds the drone stays; collecting the perk again restarts it (K1).")]
        [SerializeField, Min(0f)] private float _duration = 15f;

        [Tooltip("Range in tiles in which the drone picks a target (K3).")]
        [SerializeField, Min(0f)] private float _range = 5f;

        [Tooltip("Damage per second as a share of the start weapon's damage per second (K3).")]
        [SerializeField, Min(0f)] private float _damageShare = 0.5f;

        [Tooltip("Distance in tiles the drone keeps from the player.")]
        [SerializeField, Min(0f)] private float _followDistance = 1f;

        [Tooltip("Speed in tiles per second with which the drone follows the player.")]
        [SerializeField, Min(0f)] private float _followSpeed = 6f;

        [Tooltip("Height in tiles at which the drone floats above the floor.")]
        [SerializeField, Min(0f)] private float _hoverHeight = 0.9f;

        [Tooltip("Hover animation frames per second.")]
        [SerializeField, Min(0.1f)] private float _frameRate = 8f;

        [Tooltip("Volume of the drone's shot sound, 0 to 1; below the player's weapon.")]
        [SerializeField, Range(0f, 1f)] private float _shotVolume = 0.35f;

        /// <summary>Seconds the drone stays.</summary>
        public float Duration => _duration;

        /// <summary>Target range in tiles.</summary>
        public float Range => _range;

        /// <summary>Damage per second as a share of the start weapon's.</summary>
        public float DamageShare => _damageShare;

        /// <summary>Distance in tiles the drone keeps from the player.</summary>
        public float FollowDistance => _followDistance;

        /// <summary>Follow speed in tiles per second.</summary>
        public float FollowSpeed => _followSpeed;

        /// <summary>Height in tiles above the floor.</summary>
        public float HoverHeight => _hoverHeight;

        /// <summary>Hover animation frames per second.</summary>
        public float FrameRate => _frameRate;

        /// <summary>Volume of the shot sound.</summary>
        public float ShotVolume => _shotVolume;
    }
}
