using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>Values of the Mega-Boomer (issue #159, decisions C9a, C9b): puddles and the split into two halves.</summary>
    [CreateAssetMenu(fileName = "MegaBoomerDefinition", menuName = "RushOps/Mega-Boomer Definition")]
    public class MegaBoomerDefinition : BossDefinition
    {
        [Header("Puddles")]
        [Tooltip("Seconds from one puddle to the next while walking.")]
        [SerializeField, Min(0.1f)] private float _puddleInterval = 2.5f;

        [Tooltip("Seconds a new puddle glows before it hurts (C9b).")]
        [SerializeField, Min(0f)] private float _puddleWarmUp = 1f;

        [Tooltip("Seconds from a puddle appearing until it is gone (C9b).")]
        [SerializeField, Min(0f)] private float _puddleLifetime = 6f;

        [Tooltip("Hearts the player loses when touching a glowing-hot puddle (C9b).")]
        [SerializeField, Min(0)] private int _puddleDamage = 1;

        [Header("Split")]
        [Tooltip("At or below this share of its health (0..1) it splits into two halves (C9a).")]
        [SerializeField, Range(0f, 1f)] private float _splitHealthShare = 0.5f;

        [Tooltip("Seconds it stands still while it tears apart.")]
        [SerializeField, Min(0f)] private float _splitTime = 0.7f;

        [Tooltip("Size of a half compared to the whole boss.")]
        [SerializeField, Range(0.2f, 1f)] private float _halfScale = 0.65f;

        [Tooltip("Speed of a half compared to the whole boss.")]
        [SerializeField, Min(0f)] private float _halfSpeedFactor = 1.3f;

        [Tooltip("The halves appear this far to the left and right of the boss's feet.")]
        [SerializeField, Min(0f)] private float _halfOffset = 1f;

        /// <summary>Seconds from one puddle to the next.</summary>
        public float PuddleInterval => _puddleInterval;

        /// <summary>Seconds a puddle is harmless after appearing.</summary>
        public float PuddleWarmUp => _puddleWarmUp;

        /// <summary>Seconds a puddle lasts in all.</summary>
        public float PuddleLifetime => _puddleLifetime;

        /// <summary>Hearts lost on contact with an active puddle.</summary>
        public int PuddleDamage => _puddleDamage;

        /// <summary>Health share (0..1) at which the boss splits.</summary>
        public float SplitHealthShare => _splitHealthShare;

        /// <summary>Seconds of the split.</summary>
        public float SplitTime => _splitTime;

        /// <summary>Scale of a half.</summary>
        public float HalfScale => _halfScale;

        /// <summary>Speed factor of a half.</summary>
        public float HalfSpeedFactor => _halfSpeedFactor;

        /// <summary>Sideways distance of each half from the boss's feet.</summary>
        public float HalfOffset => _halfOffset;
    }
}
