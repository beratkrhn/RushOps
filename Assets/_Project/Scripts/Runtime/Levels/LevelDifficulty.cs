using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// Difficulty knobs of an island (decision C3): enemy values stay the same everywhere, so enemies stay learnable;
    /// instead waves come denser, hold zones take longer and timed switches give less time. Level builders hand the
    /// asset to the wave director and the puzzles of a level. Rules in <see cref="DifficultyMath"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDifficulty", menuName = "RushOps/Level Difficulty")]
    public class LevelDifficulty : ScriptableObject
    {
        [Tooltip("How much denser waves come: spawn interval and pause between waves are divided by it. 1 = as set in the waves.")]
        [SerializeField, Min(0.1f)] private float _waveDensity = 1f;

        [Tooltip("Factor on the time limit of timed switches. 1 = as in the switch config.")]
        [SerializeField, Min(0.1f)] private float _switchTimeFactor = 1f;

        [Tooltip("Seconds added to the duration of hold zones.")]
        [SerializeField, Min(0f)] private float _extraHoldSeconds;

        /// <summary>Divisor for spawn interval and pause between waves.</summary>
        public float WaveDensity => _waveDensity;

        /// <summary>Factor on the time limit of timed switches.</summary>
        public float SwitchTimeFactor => _switchTimeFactor;

        /// <summary>Seconds added to the duration of hold zones.</summary>
        public float ExtraHoldSeconds => _extraHoldSeconds;

        /// <summary>Sets all knobs; for builders and tests.</summary>
        public void Configure(float waveDensity, float switchTimeFactor, float extraHoldSeconds)
        {
            _waveDensity = Mathf.Max(0.1f, waveDensity);
            _switchTimeFactor = Mathf.Max(0.1f, switchTimeFactor);
            _extraHoldSeconds = Mathf.Max(0f, extraHoldSeconds);
        }
    }
}
