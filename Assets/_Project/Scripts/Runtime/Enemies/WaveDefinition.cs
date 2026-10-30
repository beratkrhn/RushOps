using System;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// The waves of one zone (F-GEG-02, NF-03): their composition, the spawn interval, the pause between
    /// waves and what happens after the last one. A wave counts as cleared once all its enemies died.
    /// </summary>
    [CreateAssetMenu(fileName = "Waves", menuName = "RushOps/Wave Definition")]
    public class WaveDefinition : ScriptableObject
    {
        [Tooltip("Waves in order.")]
        [SerializeField] private Wave[] _waves = Array.Empty<Wave>();

        [Tooltip("Seconds between two enemies of a wave.")]
        [SerializeField, Min(0f)] private float _spawnInterval = 0.8f;

        [Tooltip("Seconds between clearing a wave and the start of the next one.")]
        [SerializeField, Min(0f)] private float _pauseBetweenWaves = 3f;

        [Tooltip("What happens once the last wave is cleared.")]
        [SerializeField] private AfterLastWave _afterLastWave = AfterLastWave.RepeatAll;

        /// <summary>Waves in order.</summary>
        public Wave[] Waves => _waves ?? Array.Empty<Wave>();

        /// <summary>Seconds between two enemies of a wave.</summary>
        public float SpawnInterval => _spawnInterval;

        /// <summary>Seconds between clearing a wave and the start of the next one.</summary>
        public float PauseBetweenWaves => _pauseBetweenWaves;

        /// <summary>What happens once the last wave is cleared.</summary>
        public AfterLastWave AfterLastWave => _afterLastWave;

        /// <summary>Sets all values at once, for editor scripts and tests.</summary>
        public void Configure(Wave[] waves, float spawnInterval, float pauseBetweenWaves, AfterLastWave afterLastWave)
        {
            _waves = waves ?? Array.Empty<Wave>();
            _spawnInterval = Mathf.Max(0f, spawnInterval);
            _pauseBetweenWaves = Mathf.Max(0f, pauseBetweenWaves);
            _afterLastWave = afterLastWave;
        }
    }
}
