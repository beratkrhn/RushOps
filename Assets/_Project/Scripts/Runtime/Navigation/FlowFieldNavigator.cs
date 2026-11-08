using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RushOps.Navigation
{
    /// <summary>
    /// Runs one <see cref="FlowField"/> towards the target (the player) for all enemies (ADR 0003, F-GEG-01).
    /// A lure (<see cref="SetLure"/>, e.g. the scarecrow, ADR 0014) overrides the goal for every enemy that steers
    /// through this navigator until <see cref="ClearLure"/>; bosses move on their own and are not affected.
    /// Cells with a tile on the obstacle tilemap are blocked, as are cells of closed gates (<see cref="SetBlocked"/>). The field is recomputed when the target changes
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
        private readonly HashSet<Vector3Int> _blocked = new HashSet<Vector3Int>();
        private Vector2[] _positions = new Vector2[16];
        private FlowField _field;
        private Vector3Int _origin;
        private Vector2Int _targetCell;
        private float _lastCompute = float.NegativeInfinity;
        private bool _hasLure;
        private Vector2 _lure;
        private float _lureHoldRadius;

        /// <summary>The field, e.g. for tests and debugging; null without obstacle tilemap.</summary>
        public FlowField Field => _field;

        /// <summary>The followed target.</summary>
        public Transform Target
        {
            get => _target;
            set => _target = value;
        }

        /// <summary>True while a lure overrides the target (<see cref="SetLure"/>).</summary>
        public bool HasLure => _hasLure;

        /// <summary>Position of the lure; only meaningful while <see cref="HasLure"/>.</summary>
        public Vector2 LurePosition => _lure;

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

        /// <summary>
        /// Lets all enemies walk to <paramref name="position"/> instead of the target, e.g. to a scarecrow (ADR 0014).
        /// Enemies within <paramref name="holdRadius"/> of it stop and only keep their separation, so they stand
        /// around it. The field is computed once towards the lure, which does not move; setting a new lure replaces
        /// the old one.
        /// </summary>
        public void SetLure(Vector2 position, float holdRadius)
        {
            _hasLure = true;
            _lure = position;
            _lureHoldRadius = Mathf.Max(0f, holdRadius);
            Recompute();
        }

        /// <summary>Ends the lure: the enemies chase the target again.</summary>
        public void ClearLure()
        {
            if (!_hasLure)
            {
                return;
            }

            _hasLure = false;
            Recompute();
        }

        /// <summary>Grid cell of a world position.</summary>
        public Vector2Int CellOf(Vector2 world)
        {
            Vector3Int cell = _obstacles != null ? _obstacles.WorldToCell(world) : Vector3Int.FloorToInt(world);
            return new Vector2Int(cell.x - _origin.x, cell.y - _origin.y);
        }

        /// <summary>
        /// A walkable spot for <paramref name="world"/>, e.g. for a perk drop: the position itself if its cell is
        /// walkable, else the center of the nearest walkable cell within <paramref name="maxRadius"/> cells. Walls and
        /// closed gates are not walkable. Without obstacle tilemap every position counts as walkable.
        /// </summary>
        /// <returns>False if no walkable cell is near enough.</returns>
        public bool TryFindWalkablePosition(Vector2 world, int maxRadius, out Vector2 position)
        {
            position = world;
            if (_field == null)
            {
                return true;
            }

            Vector2Int start = CellOf(world);
            if (!_field.TryFindNearestWalkable(start, maxRadius, out Vector2Int cell))
            {
                return false;
            }

            if (cell != start)
            {
                position = _obstacles.GetCellCenterWorld(new Vector3Int(cell.x + _origin.x, cell.y + _origin.y, 0));
            }

            return true;
        }

        /// <summary>
        /// Move direction (length 0..1) for <paramref name="agent"/> whose body center is at <paramref name="position"/>:
        /// along the field, straight at the target when close or off the field, plus separation from the other
        /// registered enemies, scaled by <paramref name="separationScale"/> (0 = ignore the others, e.g. a heavy brute).
        /// </summary>
        public Vector2 Steer(Transform agent, Vector2 position, float separationScale = 1f)
        {
            Vector2 path = PathDirection(position);
            if (_config == null || separationScale <= 0f)
            {
                return path;
            }

            RefreshPositions();
            int self = _agents.IndexOf(agent);
            Vector2 separation = SteeringMath.Separation(agent.position, _positions, _agents.Count, self, _config.SeparationRadius);
            return SteeringMath.Combine(path, separation, _config.SeparationWeight * separationScale);
        }

        /// <summary>
        /// Path direction at <paramref name="position"/> without separation. Aims at the center of the next cell on
        /// the path rather than along the cell's direction, so bodies keep off wall corners instead of catching on them.
        /// </summary>
        public Vector2 PathDirection(Vector2 position)
        {
            if (!_hasLure && _target == null)
            {
                return Vector2.zero;
            }

            Vector2 toTarget = Goal - position;
            if (_hasLure && toTarget.sqrMagnitude <= _lureHoldRadius * _lureHoldRadius)
            {
                return Vector2.zero;
            }

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

        /// <summary>
        /// Blocks or frees the cells whose centers lie in <paramref name="area"/> (world units), e.g. a closed gate,
        /// and rebuilds the field.
        /// </summary>
        public void SetBlocked(Bounds area, bool blocked)
        {
            if (_obstacles == null)
            {
                return;
            }

            Vector3Int min = _obstacles.WorldToCell(area.min);
            Vector3Int max = _obstacles.WorldToCell(area.max);
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (!area.Contains(_obstacles.GetCellCenterWorld(cell)))
                    {
                        continue;
                    }

                    if (blocked)
                    {
                        _blocked.Add(cell);
                    }
                    else
                    {
                        _blocked.Remove(cell);
                    }
                }
            }

            Rebuild();
        }

        /// <summary>True if the tilemap cell <paramref name="cell"/> is blocked via <see cref="SetBlocked"/>.</summary>
        public bool IsBlocked(Vector3Int cell)
        {
            return _blocked.Contains(cell);
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
            // A lure stands still, so its field never needs a recompute.
            if (_field == null || _target == null || _hasLure)
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
                    var cell = new Vector3Int(_origin.x + x, _origin.y + y, 0);
                    bool blocked = _obstacles.HasTile(cell) || _blocked.Contains(cell);
                    _field.SetWalkable(new Vector2Int(x, y), !blocked);
                }
            }
        }

        // Where the enemies walk to: the lure while there is one, else the target.
        private Vector2 Goal => _hasLure ? _lure : (Vector2)_target.position;

        private void Recompute()
        {
            if (_field == null || (!_hasLure && _target == null))
            {
                return;
            }

            // Noted under a lure too, so Update compares against a fresh cell once the chase resumes.
            if (_target != null)
            {
                _targetCell = CellOf(_target.position);
            }

            _field.Compute(_hasLure ? CellOf(_lure) : _targetCell);
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
