using System;
using System.Collections.Generic;
using RushOps.Combat;
using RushOps.Levels;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// Runs the waves of all zones (F-GEG-02, F-GEG-03): a zone's <see cref="WaveSequence"/> starts when the zone
    /// becomes active and stops when it is completed. Requested enemies wait in a <see cref="SpawnQueue"/> until
    /// fewer than <see cref="SpawnerConfig.MaxActive"/> non-boss enemies are alive or announced and one of the
    /// zone's spawn points is allowed (<see cref="SpawnRules"/>). A spawn is announced by a blinking marker; if the
    /// point became unusable meanwhile, the enemy goes back to the front of the queue. A zone's waves can be swapped
    /// while it runs, e.g. for the duration of a puzzle (<see cref="UseWaves"/>, issue #34); enemies always report
    /// their death to the sequence that asked for them.
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
        [Tooltip("Difficulty of the level; its wave density shortens spawn intervals and pauses. Empty = as in the waves.")]
        [SerializeField] private LevelDifficulty _difficulty;

        private readonly SpawnQueue _queue = new SpawnQueue();
        private readonly Dictionary<Enemy, WaveSequence> _owners = new Dictionary<Enemy, WaveSequence>();
        private WaveSequence[] _sequences;
        // Spawn points of all zones in one list, with the zone each belongs to.
        private SpawnPoint[] _points;
        private int[] _pointZone;
        private float[] _announced;
        private SpawnRequest[] _announcedRequest;
        private WaveSequence[] _announcedSequence;
        private int[] _candidates;
        private int _announcedCount;
        private int _announcedSlots;
        private IRandomSource _pointRandom = new SystemRandomSource();
        private IRandomSource _packRandom = new SystemRandomSource();
        private Vector2[] _packPositions = new Vector2[4];
        private Func<SpawnRequest, bool> _canPlace;
        private int _maxActiveOverride = -1;
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

        /// <summary>
        /// Most non-boss enemies alive or announced at once: the value set with <see cref="SetMaxActive"/>, otherwise
        /// <see cref="SpawnerConfig.MaxActive"/>; 0 without config.
        /// </summary>
        public int MaxActive => _maxActiveOverride >= 0 ? _maxActiveOverride : (_config != null ? _config.MaxActive : 0);

        /// <summary>
        /// Enemies that may still appear before the cap of <see cref="MaxActive"/> is reached, counting
        /// living and announced non-boss enemies, every member of an announced pack (H4); 0 without config. Bosses that
        /// spawn enemies themselves check it too.
        /// </summary>
        public int FreeSlots => _config != null
            ? SpawnRules.FreeSlots(MaxActive, AliveCount, _announcedSlots)
            : 0;

        /// <summary>
        /// Replaces the random sources of the spawn points and of the offsets in a pack, e.g. with the streams of the
        /// daily run (F-END-13); null keeps a fresh unseeded source.
        /// </summary>
        public void SetRandomSources(IRandomSource points, IRandomSource packs)
        {
            _pointRandom = points ?? new SystemRandomSource();
            _packRandom = packs ?? new SystemRandomSource();
        }

        /// <summary>
        /// Replaces the cap of <see cref="SpawnerConfig.MaxActive"/> from now on, e.g. with the value of an endless round
        /// (F-GEG-03, F-END-04). A negative value goes back to the config.
        /// </summary>
        public void SetMaxActive(int maxActive)
        {
            _maxActiveOverride = maxActive >= 0 ? maxActive : -1;
        }

        /// <summary>
        /// Runs <paramref name="waves"/> in <paramref name="zone"/> from now on instead of its current waves, e.g. faster
        /// spawns while a puzzle is worked on (issue #34). Enemies still waiting in the queue for the old waves are
        /// dropped; living ones stay. The new waves start at once if the zone is active, otherwise with the zone.
        /// </summary>
        public void UseWaves(Zone zone, WaveDefinition waves)
        {
            int index = Array.IndexOf(_zones, zone);
            if (index < 0 || _sequences == null || zone.State == ZoneState.Completed)
            {
                return;
            }

            _sequences[index].Stop();
            _queue.RemoveOwner(index);
            _sequences[index] = CreateSequence(index, waves);
            if (zone.State == ZoneState.Active)
            {
                _sequences[index].Start();
            }
        }

        /// <summary>Ends the waves of <paramref name="zone"/> for good; living enemies stay.</summary>
        public void StopWaves(Zone zone)
        {
            int index = Array.IndexOf(_zones, zone);
            if (index >= 0 && _sequences != null)
            {
                _sequences[index].Stop();
                _queue.RemoveOwner(index);
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
                _sequences[z] = CreateSequence(z, _zones[z] != null ? _zones[z].Waves : null);
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
            _announcedSequence = new WaveSequence[_points.Length];
            _candidates = new int[_points.Length];
            for (int i = 0; i < _announced.Length; i++)
            {
                _announced[i] = -1f;
            }
        }

        private WaveSequence CreateSequence(int owner, WaveDefinition waves)
        {
            var sequence = new WaveSequence(waves, _difficulty != null ? _difficulty.WaveDensity : 1f);
            sequence.SpawnRequested += enemy => OnSpawnRequested(owner, sequence, enemy);
            sequence.WaveStarted += wave => WaveStarted?.Invoke(_zones[owner], wave);
            return sequence;
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

        private void OnSpawnRequested(int owner, WaveSequence sequence, EnemyDefinition enemy)
        {
            if (PoolFor(enemy) == null)
            {
                Debug.LogWarning($"{name}: no EnemyPool for {enemy.name}; the wave skips it", this);
                sequence.NotifyKilled();
                return;
            }

            _queue.Enqueue(new SpawnRequest(owner, enemy));
        }

        private void OnEnemyDied(Enemy enemy)
        {
            if (_owners.TryGetValue(enemy, out WaveSequence owner))
            {
                _owners.Remove(enemy);
                owner.NotifyKilled();
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
                    point.SetAnnounced(true);
                    continue;
                }

                SpawnRequest request = _announcedRequest[i];
                WaveSequence sequence = _announcedSequence[i];
                _announced[i] = -1f;
                _announcedRequest[i] = default;
                _announcedSequence[i] = null;
                _announcedCount--;
                _announcedSlots -= request.SlotCost;

                point.SetMarkerVisible(false);
                point.SetAnnounced(false);
                // The zone was completed while the marker blinked: its enemies are no longer wanted.
                if (_zones[request.Owner].State == ZoneState.Completed)
                {
                    continue;
                }

                // The zone's waves were swapped while the marker blinked: the old sequence no longer wants it.
                if (sequence != _sequences[request.Owner])
                {
                    continue;
                }

                if (!SpawnRules.IsAllowed(point.Position, _view, _playerPosition, _config.MinPlayerDistance))
                {
                    _queue.PushFront(request);
                    continue;
                }

                SpawnAt(point, request, sequence);
            }
        }

        // Spawns all enemies of the request at the point, a pack with a small offset each (H5); the ones the pool could
        // not give go back to the front of the queue.
        private void SpawnAt(SpawnPoint point, SpawnRequest request, WaveSequence sequence)
        {
            if (_packPositions.Length < request.Count)
            {
                _packPositions = new Vector2[request.Count];
            }

            int count = PackRules.Offsets(point.Position, request.Enemy.PackSpread, request.Count, _packRandom, _packPositions);
            EnemyPool pool = PoolFor(request.Enemy);
            int spawned = 0;
            while (spawned < count)
            {
                Enemy enemy = pool.Spawn(_packPositions[spawned]);
                if (enemy == null)
                {
                    break;
                }

                spawned++;
                _owners[enemy] = sequence;
                if (point.IsBreach)
                {
                    // Crawling out of the breach (F-END-15) before the spawn is reported.
                    enemy.BeginEmerge(_config.EmergeDuration);
                }

                Spawned?.Invoke(enemy, point);
            }

            if (spawned > 0 && point.IsBreach)
            {
                point.HoldOpen(_config.EmergeDuration);
            }

            if (spawned < request.Count)
            {
                _queue.PushFront(new SpawnRequest(request.Owner, request.Enemy, request.Count - spawned));
            }
        }

        private void AnnounceReady()
        {
            while (true)
            {
                int index = _queue.FindReady(FreeSlots, _canPlace);
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
                _announcedSequence[point] = _sequences[request.Owner];
                _announcedCount++;
                _announcedSlots += request.SlotCost;
            }
        }

        // Only whether a point is free, without a roll: the rolls follow the spawns, not the frames (F-END-13).
        private bool CanPlace(SpawnRequest request)
        {
            return Candidates(request.Owner) > 0;
        }

        // A random free and allowed spawn point of the zone, or -1.
        private int PickPoint(int owner)
        {
            int count = Candidates(owner);
            return count > 0 ? SpawnRules.Pick(_candidates, count, _pointRandom.Value()) : -1;
        }

        // Fills the candidates with the free and allowed spawn points of the zone; returns their number.
        private int Candidates(int owner)
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

            return count;
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
