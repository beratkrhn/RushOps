using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>Values of the enemy spawner (F-GEG-02, F-GEG-03). Wave logic follows in v0.2.0.</summary>
    [CreateAssetMenu(fileName = "SpawnerConfig", menuName = "RushOps/Spawner Config")]
    public class SpawnerConfig : ScriptableObject
    {
        [Tooltip("Most enemies alive at once, including announced ones (F-GEG-03). Bosses will not count.")]
        [SerializeField, Min(0)] private int _maxActive = 10;

        [Tooltip("Seconds between two spawn announcements.")]
        [SerializeField, Min(0f)] private float _spawnInterval = 0.8f;

        [Tooltip("Seconds the marker blinks on the spawn point before the enemy appears.")]
        [SerializeField, Min(0f)] private float _telegraphDuration = 0.6f;

        [Tooltip("Seconds per on or off phase of the blinking marker.")]
        [SerializeField, Min(0.01f)] private float _markerBlinkInterval = 0.08f;

        [Tooltip("Spawn points closer to the player than this distance in units are skipped, even off screen.")]
        [SerializeField, Min(0f)] private float _minPlayerDistance = 4f;

        [Tooltip("The camera view is enlarged by this margin in units; spawn points inside are skipped.")]
        [SerializeField, Min(0f)] private float _viewMargin = 1f;

        /// <summary>Most enemies alive at once, including announced ones.</summary>
        public int MaxActive => _maxActive;

        /// <summary>Seconds between two spawn announcements.</summary>
        public float SpawnInterval => _spawnInterval;

        /// <summary>Seconds the marker shows before the enemy appears.</summary>
        public float TelegraphDuration => _telegraphDuration;

        /// <summary>Seconds per on or off phase of the blinking marker.</summary>
        public float MarkerBlinkInterval => _markerBlinkInterval;

        /// <summary>Spawn points closer to the player than this are skipped.</summary>
        public float MinPlayerDistance => _minPlayerDistance;

        /// <summary>Margin added around the camera view for skipping visible spawn points.</summary>
        public float ViewMargin => _viewMargin;
    }
}
