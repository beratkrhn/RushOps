using UnityEngine;

namespace RushOps.Scoring
{
    /// <summary>Assumptions behind the proposed star thresholds (issue #301, F-SCO-03, N33).</summary>
    [CreateAssetMenu(fileName = "StarEstimateConfig", menuName = "RushOps/Star Estimate Config")]
    public class StarEstimateConfig : ScriptableObject
    {
        [Tooltip("Average combo multiplier of a solid run (2 stars).")]
        [SerializeField, Min(1f)] private float _solidMultiplier = 1.5f;

        [Tooltip("Average combo multiplier of a strong run (3 stars).")]
        [SerializeField, Min(1f)] private float _strongMultiplier = 2.5f;

        [Tooltip("Share of the enemies the level sends that the player kills, 0..1.")]
        [SerializeField, Range(0f, 1f)] private float _killShare = 0.9f;

        [Tooltip("Share of the dropped coins the player picks up, 0..1.")]
        [SerializeField, Range(0f, 1f)] private float _coinShare = 0.8f;

        [Tooltip("Extra rounds of the last wave in zones whose waves repeat until their puzzle is solved.")]
        [SerializeField, Min(0f)] private float _repeatRounds = 1f;

        [Tooltip("Thresholds are rounded to this step.")]
        [SerializeField, Min(1)] private int _roundTo = 50;

        /// <summary>Average multiplier of a solid run.</summary>
        public float SolidMultiplier => _solidMultiplier;

        /// <summary>Average multiplier of a strong run.</summary>
        public float StrongMultiplier => _strongMultiplier;

        /// <summary>Share of enemies killed.</summary>
        public float KillShare => _killShare;

        /// <summary>Share of coins picked up.</summary>
        public float CoinShare => _coinShare;

        /// <summary>Extra rounds of a repeating last wave.</summary>
        public float RepeatRounds => _repeatRounds;

        /// <summary>Rounding step of the thresholds.</summary>
        public int RoundTo => _roundTo;
    }
}
