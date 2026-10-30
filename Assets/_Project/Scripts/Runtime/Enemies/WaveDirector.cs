using System;
using System.Collections.Generic;
using RushOps.Combat;
using RushOps.Levels;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Runs the waves of all zones (F-GEG-02, F-GEG-03): a zone's <see cref="WaveSequence"/> starts when the zone
    /// becomes active and stops when it is completed. Requested enemies wait in a <see cref="SpawnQueue"/> until
    /// fewer than <see cref="SpawnerConfig.MaxActive"/> non-boss enemies are alive or announced and one of the
    /// zone's spawn points is allowed (<see cref="SpawnRules"/>). A spawn is announced by a blinking marker; if the
    /// point became unusable meanwhile, the enemy goes back to the front of the queue.
    /// </summary>
    public class WaveDirector : MonoBehaviour
    {
        [SerializeField] private SpawnerConfig _config;
        [Tooltip("One pool per enemy type; waves take their enemies from the pool of the matching type.")]
        [SerializeField] private EnemyPool[] _pools = Array.Empty<EnemyPool>();
        [SerializeField] private Zone[] _zones = Array.Empty<Zone>();
        [SerializeField] private Transform _player;
        [Tooltip("Camera whose view excludes spawn points; the main camera if empty.")]
        [SerializeField] private Camera _camera;

        private readonly SpawnQueue _queue = new SpawnQueue();
        private readonly Dictionary<Enemy, int> _owners = new Dictionary<Enemy, int>();
        private WaveSequence[] _sequences;
        // Spawn points of all zones in one list, with the zone each belongs to.
        private SpawnPoint[] _points;
        private int[] _pointZone;
        private float[] _announced;
        private SpawnRequest[] _announcedRequest;
        private int[] _candidates;
        private int _announcedCount;
        private int _announcedBosses;
        private Func<SpawnRequest, bool> _canPlace;
        private Rect _view;
        private Vector2 _playerPosition;

        /// <summary>Raised when an enemy appears, with the spawn point it came from.</summary>
        public event Action<Enemy, SpawnPoint> Spawned;

        /// <summary>Raised when a zone's wave starts, with the wave index.</summary>
        public event Action<Zone, int> WaveStarted;

        /// <summary>Shared spawn values.</summary>
        public SpawnerConfig Config => _config;

        /// <summary>Number of spawns announced but not yet happened.</summary>
        public int AnnouncedCount => _announcedCount;

        /// <summary>Number of enemies waiting for a free slot or spawn point.</summary>
        public int QueuedCount => _queue.Count;

        /// <summary>Number of non-boss enemies in the level, including dying ones.</summary>
        public int AliveCount
        {
            get
            {
                int alive = 0;
                foreach (EnemyPool pool in _pools)
                {
                    if (pool != null && (pool.Definition == null || !pool.Definition.IsBoss))
                    {
                        alive += pool.CountActive;
                    }
                }

                return alive;
            }
        }

        /// <summary>The wave sequence of <paramref name="zone"/>, or null if it is not one of the director's zones.</summary>
        public WaveSequence SequenceOf(Zone zone)
        {
            int index = Array.IndexOf(_zones, zone);
            return index >= 0 ? _sequences[index] : null;
        }

        private void Awake()
        {
            _canPlace = CanPlace;
            _sequences = new WaveSequence[_zones.Length];
            var points = new List<SpawnPoint>();
            var pointZone = new List<int>();
            for (int z = 0; z < _zones.Length; z++)
            {
                int owner = z;
                _sequences[z] = new WaveSequence(_zones[z] != null ? _zones[z].Waves : null);
                _sequences[z].SpawnRequested += enemy => OnSpawnRequested(owner, enemy);
                _sequences[z].WaveStarted += wave => WaveStarted?.Invoke(_zones[owner], wave);
                if (_zones[z] == null)
                {
                    continue;
                }

                foreach (SpawnPoint point in _zones[z].SpawnPoints)
                {
                    if (point != null)
                    {
                        points.Add(point);
                        pointZone.Add(z);
                    }
                }
            }

            _points = points.ToArray();
            _pointZone = pointZone.ToArray();
            _announced = new float[_points.Length];
            _announcedRequest = new SpawnRequest[_points.Length];
            _candidates = new int[_points.Length];
            for (int i = 0; i < _announced.Length; i++)
            {
                _announced[i] = -1f;
            }
        }

        private void OnEnable()
        {
            foreach (Zone zone in _zones)
            {
                if (zone != null)
                {
                    zone.Activated += OnZoneActivated;
                    zone.Completed += OnZoneCompleted;
                }
            }

            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    pool.EnemyDied += OnEnemyDied;
                }
            }
        }

        private void OnDisable()
        {
            foreach (Zone zone in _zones)
            {
                if (zone != null)
                {
                    zone.Activated -= OnZoneActivated;
                    zone.Completed -= OnZoneCompleted;
                }
            }

            foreach (EnemyPool pool in _pools)
            {
                if (pool != null)
                {
                    pool.EnemyDied -= OnEnemyDied;
                }
            }
        }

        private void Start()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            // Zones that became active before this component was enabled.
            for (int z = 0; z < _zones.Length; z++)
            {
                if (_zones[z] != null && _zones[z].State == ZoneState.Active)
                {
                    _sequences[z].Start();
                }
            }
        }

        private void Update()
        {
            if (_config == null)
            {
                return;
            }

            float dt = Time.deltaTime;
            _view = CurrentView();
            _playerPosition = _player != null ? (Vector2)_player.position : _view.center;
            TickAnnounced(dt);
            for (int z = 0; z < _sequences.Length; z++)
            {
                _sequences[z].Tick(dt);
            }

            AnnounceReady();
        }

        private void OnZoneActivated(Zone zone)
        {
            int index = Array.IndexOf(_zones, zone);
            if (index >= 0)
            {
                _sequences[index].Start();
            }
        }

        private void OnZoneCompleted(Zone zone)
        {
            int index = Array.IndexOf(_zones, zone);
            if (index >= 0)
            {
                _sequences[index].Stop();
                _queue.RemoveOwner(index);
            }
        }

        private void OnSpawnRequested(int owner, EnemyDefinition enemy)
        {
            if (PoolFor(enemy) == null)
            {
                Debug.LogWarning($"{name}: no EnemyPool for {enemy.name}; the wave skips it", this);
                _sequences[owner].NotifyKilled();
                return;
            }

            _queue.Enqueue(new SpawnRequest(owner, enemy));
        }

        private void OnEnemyDied(Enemy enemy)
        {
            if (_owners.TryGetValue(enemy, out int owner))
            {
                _owners.Remove(enemy);
                _sequences[owner].NotifyKilled();
            }
        }

        private void TickAnnounced(float dt)
        {
            if (_announcedCount == 0)
            {
                return;
            }

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

                SpawnRequest request = _announcedRequest[i];
                _announced[i] = -1f;
                _announcedRequest[i] = default;
                _announcedCount--;
                if (request.IsBoss)
                {
                    _announcedBosses--;
                }

                point.SetMarkerVisible(false);
                // The zone was completed while the marker blinked: its enemies are no longer wanted.
                if (_zones[request.Owner].State == ZoneState.Completed)
                {
                    continue;
                }

                if (!SpawnRules.IsAllowed(point.Position, _view, _playerPosition, _config.MinPlayerDistance))
                {
                    _queue.PushFront(request);
                    continue;
                }

                Enemy enemy = PoolFor(request.Enemy).Spawn(point.Position);
                if (enemy == null)
                {
                    _queue.PushFront(request);
                    continue;
                }

                _owners[enemy] = request.Owner;
                Spawned?.Invoke(enemy, point);
            }
        }

        private void AnnounceReady()
        {
            while (true)
            {
                int free = SpawnRules.FreeSlots(_config.MaxActive, AliveCount, _announcedCount - _announcedBosses);
                int index = _queue.FindReady(free, _canPlace);
                if (index < 0)
                {
                    return;
                }

                SpawnRequest request = _queue.RemoveAt(index);
                int point = PickPoint(request.Owner);
                if (point < 0)
                {
                    _queue.PushFront(request);
                    return;
                }

                _announced[point] = 0f;
                _announcedRequest[point] = request;
                _announcedCount++;
                if (request.IsBoss)
                {
                    _announcedBosses++;
                }
            }
        }

        private bool CanPlace(SpawnRequest request)
        {
            return PickPoint(request.Owner) >= 0;
        }

        // A random free and allowed spawn point of the zone, or -1.
        private int PickPoint(int owner)
        {
            int count = 0;
            for (int i = 0; i < _points.Length; i++)
            {
                if (_pointZone[i] == owner && _announced[i] < 0f
                    && SpawnRules.IsAllowed(_points[i].Position, _view, _playerPosition, _config.MinPlayerDistance))
                {
                    _candidates[count++] = i;
                }
            }

            // Random per call: CanPlace and the following pick may choose different points, both are allowed.
            return SpawnRules.Pick(_candidates, count, UnityEngine.Random.value);
        }

        private EnemyPool PoolFor(EnemyDefinition enemy)
        {
            foreach (EnemyPool pool in _pools)
            {
                if (pool != null && pool.Definition == enemy)
                {
                    return pool;
                }
            }

            return null;
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
