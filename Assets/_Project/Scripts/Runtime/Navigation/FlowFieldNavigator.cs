using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RushOps.Navigation
{
    /// <summary>
    /// Runs one <see cref="FlowField"/> towards the target (the player) for all enemies (ADR 0003, F-GEG-01).
    /// Cells with a tile on the obstacle tilemap are blocked. The field is recomputed when the target changes
    /// cell, at most every <see cref="NavigationConfig.RecomputeInterval"/>. Enemies register themselves and ask
    /// <see cref="Steer"/> for their move direction, which adds separation from the other registered enemies.
    /// </summary>
    public class FlowFieldNavigator : MonoBehaviour
    {
        [SerializeField] private NavigationConfig _config;
        [Tooltip("Tilemap whose tiles block movement, e.g. walls.")]
        [SerializeField] private Tilemap _obstacles;
        [SerializeField] private Transform _target;

        private readonly List<Transform> _agents = new List<Transform>();
        private Vector2[] _positions = new Vector2[16];
        private FlowField _field;
        private Vector3Int _origin;
        private Vector2Int _targetCell;
        private float _lastCompute = float.NegativeInfinity;

        /// <summary>The field, e.g. for tests and debugging; null without obstacle tilemap.</summary>
        public FlowField Field => _field;

        /// <summary>The followed target.</summary>
        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        /// <summary>Number of computations so far.</summary>
        public int ComputeCount { get; private set; }

        /// <summary>Adds an enemy to the separation.</summary>
        public void Register(Transform agent)
        {
            if (!_agents.Contains(agent))
            {
                _agents.Add(agent);
            }
        }

        /// <summary>Removes an enemy from the separation, e.g. when it dies.</summary>
        public void Unregister(Transform agent)
        {
            _agents.Remove(agent);
        }

        /// <summary>Grid cell of a world position.</summary>
        public Vector2Int CellOf(Vector2 world)
        {
            Vector3Int cell = _obstacles != null ? _obstacles.WorldToCell(world) : Vector3Int.FloorToInt(world);
            return new Vector2Int(cell.x - _origin.x, cell.y - _origin.y);
        }

        /// <summary>
        /// Move direction (length 0..1) for <paramref name="agent"/> whose body center is at <paramref name="position"/>:
        /// along the field, straight at the target when close or off the field, plus separation from the other
        /// registered enemies.
        /// </summary>
        public Vector2 Steer(Transform agent, Vector2 position)
        {
            Vector2 path = PathDirection(position);
            if (_config == null)
            {
                return path;
            }

            RefreshPositions();
            int self = _agents.IndexOf(agent);
            Vector2 separation = SteeringMath.Separation(agent.position, _positions, _agents.Count, self, _config.SeparationRadius);
            return SteeringMath.Combine(path, separation, _config.SeparationWeight);
        }

        /// <summary>
        /// Path direction at <paramref name="position"/> without separation. Aims at the center of the next cell on
        /// the path rather than along the cell's direction, so bodies keep off wall corners instead of catching on them.
        /// </summary>
        public Vector2 PathDirection(Vector2 position)
        {
            if (_target == null)
            {
                return Vector2.zero;
            }

            Vector2 toTarget = (Vector2)_target.position - position;
            if (_field == null)
            {
                return toTarget.normalized;
            }

            Vector2Int cell = CellOf(position);
            int distance = _field.Distance(cell);
            int direct = _config != null ? _config.DirectChaseDistance : FlowField.DiagonalCost;
            if (distance == FlowField.Unreachable || distance <= direct)
            {
                return toTarget.normalized;
            }

            Vector2 step = _field.Direction(cell);
            var next = new Vector3Int(cell.x + Mathf.RoundToInt(step.x) + _origin.x, cell.y + Mathf.RoundToInt(step.y) + _origin.y, 0);
            Vector2 toNext = (Vector2)_obstacles.GetCellCenterWorld(next) - position;
            return toNext.sqrMagnitude > 1e-6f ? toNext.normalized : step;
        }

        /// <summary>Re-reads the obstacle tilemap and recomputes the field, e.g. after a gate opened.</summary>
        public void Rebuild()
        {
            BuildGrid();
            Recompute();
        }

        private void Awake()
        {
            BuildGrid();
        }

        private void Start()
        {
            Recompute();
        }

        private void Update()
        {
            if (_field == null || _target == null)
            {
                return;
            }

            float interval = _config != null ? _config.RecomputeInterval : 0f;
            if (CellOf(_target.position) != _targetCell && Time.time - _lastCompute >= interval)
            {
                Recompute();
            }
        }

        private void BuildGrid()
        {
            if (_obstacles == null)
            {
                return;
            }

            _obstacles.CompressBounds();
            BoundsInt bounds = _obstacles.cellBounds;
            _origin = bounds.min;
            _field = new FlowField(bounds.size.x, bounds.size.y);
            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    bool blocked = _obstacles.HasTile(new Vector3Int(_origin.x + x, _origin.y + y, 0));
                    _field.SetWalkable(new Vector2Int(x, y), !blocked);
                }
            }
        }

        private void Recompute()
        {
            if (_field == null || _target == null)
            {
                return;
            }

            _targetCell = CellOf(_target.position);
            _field.Compute(_targetCell);
            _lastCompute = Time.time;
            ComputeCount++;
        }

        // With at most a few dozen enemies, copying all positions per call is cheaper than tracking changes.
        private void RefreshPositions()
        {
            if (_positions.Length < _agents.Count)
            {
                _positions = new Vector2[_agents.Count * 2];
            }

            for (int i = 0; i < _agents.Count; i++)
            {
                _positions[i] = _agents[i].position;
            }
        }
    }
}
