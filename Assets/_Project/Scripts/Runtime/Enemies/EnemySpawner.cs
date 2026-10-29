using System;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Keeps up to <see cref="SpawnerConfig.MaxActive"/> enemies in the level (F-GEG-03): every
    /// <see cref="SpawnerConfig.SpawnInterval"/> it announces a spawn on a random allowed <see cref="SpawnPoint"/>
    /// (F-GEG-02, see <see cref="SpawnRules"/>), blinks the marker there and then takes an enemy from the pool.
    /// If the point came into view or close to the player meanwhile, the spawn is dropped.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private SpawnerConfig _config;
        [SerializeField] private EnemyPool _pool;
        [SerializeField] private SpawnPoint[] _points;
        [SerializeField] private Transform _player;
        [Tooltip("Camera whose view excludes spawn points; the main camera if empty.")]
        [SerializeField] private Camera _camera;

        private float[] _announced;
        private int[] _candidates;
        private int _announcedCount;
        private float _cooldown;

        /// <summary>Raised when an enemy appears, with the spawn point it came from.</summary>
        public event Action<Enemy, SpawnPoint> Spawned;

        /// <summary>Spawner values.</summary>
        public SpawnerConfig Config => _config;

        /// <summary>Number of spawns announced but not yet happened.</summary>
        public int AnnouncedCount => _announcedCount;

        private void Awake()
        {
            int count = _points != null ? _points.Length : 0;
            _announced = new float[count];
            _candidates = new int[count];
            for (int i = 0; i < count; i++)
            {
                _announced[i] = -1f;
            }
        }

        private void Start()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void Update()
        {
            if (_config == null || _pool == null || _points == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            TickAnnounced(dt);
            _cooldown -= dt;
            if (_cooldown > 0f || SpawnRules.FreeSlots(_config.MaxActive, _pool.CountActive, _announcedCount) == 0)
            {
                return;
            }

            if (Announce())
            {
                _cooldown = _config.SpawnInterval;
            }
        }

        private void TickAnnounced(float dt)
        {
            if (_announcedCount == 0)
            {
                return;
            }

            Rect view = CurrentView();
            Vector2 player = PlayerPosition(view);
            for (int i = 0; i < _announced.Length; i++)
            {
                if (_announced[i] < 0f)
                {
                    continue;
                }

                _announced[i] += dt;
                SpawnPoint point = _points[i];
                if (_announced[i] < _config.TelegraphDuration)
                {
                    // Inverted, so the marker shows at once and then blinks.
                    point.SetMarkerVisible(!BlinkMath.IsVisible(_announced[i], _config.MarkerBlinkInterval));
                    continue;
                }

                _announced[i] = -1f;
                _announcedCount--;
                point.SetMarkerVisible(false);
                if (!SpawnRules.IsAllowed(point.Position, view, player, _config.MinPlayerDistance))
                {
                    continue;
                }

                Enemy enemy = _pool.Spawn(point.Position);
                if (enemy != null)
                {
                    Spawned?.Invoke(enemy, point);
                }
            }
        }

        private bool Announce()
        {
            Rect view = CurrentView();
            Vector2 player = PlayerPosition(view);
            int count = 0;
            for (int i = 0; i < _points.Length; i++)
            {
                if (_points[i] != null && _announced[i] < 0f
                    && SpawnRules.IsAllowed(_points[i].Position, view, player, _config.MinPlayerDistance))
                {
                    _candidates[count++] = i;
                }
            }

            int chosen = SpawnRules.Pick(_candidates, count, UnityEngine.Random.value);
            if (chosen < 0)
            {
                return false;
            }

            _announced[chosen] = 0f;
            _announcedCount++;
            return true;
        }

        private Vector2 PlayerPosition(Rect view)
        {
            return _player != null ? (Vector2)_player.position : view.center;
        }

        private Rect CurrentView()
        {
            if (_camera == null || !_camera.orthographic)
            {
                return new Rect(Vector2.zero, Vector2.zero);
            }

            var half = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);
            return SpawnRules.View(_camera.transform.position, half, _config.ViewMargin);
        }
    }
}
