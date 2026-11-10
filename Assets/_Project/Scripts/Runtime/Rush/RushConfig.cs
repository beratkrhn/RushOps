using UnityEngine;

namespace RushOps.Rush
{
    /// <summary>
    /// Values of the rush meter (F-RSH-01 to F-RSH-05, decisions RM2, RM4, RM6, N8). All are start values;
    /// <see cref="RushMeter"/> and <see cref="RushRules"/> apply them.
    /// </summary>
    [CreateAssetMenu(fileName = "RushConfig", menuName = "RushOps/Rush Config")]
    public class RushConfig : ScriptableObject
    {
        [Header("Meter")]
        [Tooltip("Fill at which the meter is full; every kill adds the current combo multiplier (RM4, N15).")]
        [SerializeField, Min(1)] private int _killsToFill = 25;

        [Tooltip("Seconds a triggered rush lasts (RM2).")]
        [SerializeField, Min(0f)] private float _duration = 5f;

        [Header("Effect")]
        [Tooltip("Speed factor of normal enemies and their timers during the rush (RM2).")]
        [SerializeField, Range(0f, 1f)] private float _enemySlowFactor = 0.5f;

        [Tooltip("Speed factor of bosses and their timers during the rush; weaker than for normal enemies (RM6).")]
        [SerializeField, Range(0f, 1f)] private float _bossSlowFactor = 0.75f;

        [Tooltip("Factor on every damage the player deals during the rush; multiplies with double damage (N8, N17).")]
        [SerializeField, Min(1f)] private float _damageFactor = 1.5f;

        /// <summary>Fill at which the meter is full, at least 1.</summary>
        public int KillsToFill => Mathf.Max(1, _killsToFill);

        /// <summary>Seconds a triggered rush lasts.</summary>
        public float Duration => _duration;

        /// <summary>Speed factor of normal enemies during the rush.</summary>
        public float EnemySlowFactor => _enemySlowFactor;

        /// <summary>Speed factor of bosses during the rush.</summary>
        public float BossSlowFactor => _bossSlowFactor;

        /// <summary>Factor on the player's damage during the rush.</summary>
        public float DamageFactor => _damageFactor;
    }
}
