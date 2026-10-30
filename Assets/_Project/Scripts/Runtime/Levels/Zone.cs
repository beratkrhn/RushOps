using System;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Levels
{
    /// <summary>
    /// A section of a level (F-LVL-02): a rectangle around the transform. It becomes active when the player
    /// enters it and completes when its exit gate opens. The <see cref="WaveDirector"/> runs the zone's waves from
    /// its own spawn points in between (F-GEG-02).
    /// </summary>
    public class Zone : MonoBehaviour
    {
        [Tooltip("Width and height of the zone in units, centered on the transform.")]
        [SerializeField] private Vector2 _size = new Vector2(10f, 10f);
        [SerializeField] private Transform _player;
        [Tooltip("Gate that leads out of this zone; the zone completes when it opens. Empty for the last zone.")]
        [SerializeField] private Gate _exitGate;
        [Tooltip("Spawn points of this zone; enemies of its waves appear only here.")]
        [SerializeField] private SpawnPoint[] _spawnPoints = Array.Empty<SpawnPoint>();
        [Tooltip("Waves that run while the zone is active. Empty for a zone without enemies.")]
        [SerializeField] private WaveDefinition _waves;

        private readonly ZoneLogic _logic = new ZoneLogic();

        /// <summary>Raised once when the player enters the zone (OnZoneActivated).</summary>
        public event Action<Zone> Activated;

        /// <summary>Raised once when the zone's exit gate opened.</summary>
        public event Action<Zone> Completed;

        /// <summary>Current state.</summary>
        public ZoneState State => _logic.State;

        /// <summary>Area of the zone in world units.</summary>
        public Rect Area => new Rect((Vector2)transform.position - _size * 0.5f, _size);

        /// <summary>Gate that leads out of this zone, or null.</summary>
        public Gate ExitGate => _exitGate;

        /// <summary>Spawn points of this zone.</summary>
        public SpawnPoint[] SpawnPoints => _spawnPoints ?? Array.Empty<SpawnPoint>();

        /// <summary>Waves of this zone, or null.</summary>
        public WaveDefinition Waves => _waves;

        /// <summary>The player whose position activates the zone.</summary>
        public Transform Player
        {
            get => _player;
            set => _player = value;
        }

        /// <summary>True if <paramref name="point"/> lies inside the zone.</summary>
        public bool Contains(Vector2 point)
        {
            return Area.Contains(point);
        }

        private void Awake()
        {
            _logic.Activated += () => Activated?.Invoke(this);
            _logic.Completed += () => Completed?.Invoke(this);
        }

        private void OnEnable()
        {
            if (_exitGate != null)
            {
                _exitGate.Opened += OnExitGateOpened;
            }
        }

        private void OnDisable()
        {
            if (_exitGate != null)
            {
                _exitGate.Opened -= OnExitGateOpened;
            }
        }

        private void Update()
        {
            if (_logic.State == ZoneState.Inactive && _player != null && Contains(_player.position))
            {
                _logic.Enter();
            }
        }

        private void OnExitGateOpened(Gate gate)
        {
            _logic.Complete();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = State == ZoneState.Active ? Color.green : State == ZoneState.Completed ? Color.gray : Color.cyan;
            Gizmos.DrawWireCube(transform.position, _size);
        }
    }
}
