using UnityEngine;

namespace RushOps.Navigation
{
    /// <summary>
    /// Flow field on a cell grid (ADR 0003, F-GEG-01): <see cref="Compute"/> runs Dijkstra from one target cell
    /// over all walkable cells and stores for each cell the direction to its neighbour closest to the target.
    /// Moves go to the 8 neighbours; diagonal moves need both adjacent orthogonal cells free, so paths do not
    /// cut wall corners. Cells are (0, 0)..(width-1, height-1). Computing allocates nothing.
    /// </summary>
    public class FlowField
    {
        /// <summary>Distance of cells the target cannot be reached from.</summary>
        public const int Unreachable = int.MaxValue;

        /// <summary>Cost of an orthogonal step; a diagonal step costs <see cref="DiagonalCost"/>.</summary>
        public const int OrthogonalCost = 10;

        /// <summary>Cost of a diagonal step, about OrthogonalCost * sqrt(2).</summary>
        public const int DiagonalCost = 14;

        // Orthogonal first, so ties between equally short moves prefer straight steps.
        private static readonly Vector2Int[] Steps =
        {
            new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1),
        };

        private readonly bool[] _walkable;
        private readonly int[] _distance;
        private readonly Vector2[] _direction;
        // Binary min-heap with lazy deletion: a cell may be pushed once per improvement, at most once per incoming edge.
        private readonly int[] _heapCell;
        private readonly int[] _heapKey;
        private int _heapCount;
        private Vector2Int _target;
        private bool _hasTarget;

        /// <summary>Creates a grid of <paramref name="width"/> x <paramref name="height"/> walkable cells.</summary>
        public FlowField(int width, int height)
        {
            Width = width > 0 ? width : 1;
            Height = height > 0 ? height : 1;
            int count = Width * Height;
            _walkable = new bool[count];
            _distance = new int[count];
            _direction = new Vector2[count];
            _heapCell = new int[count * Steps.Length + 1];
            _heapKey = new int[_heapCell.Length];
            for (int i = 0; i < count; i++)
            {
                _walkable[i] = true;
                _distance[i] = Unreachable;
            }
        }

        /// <summary>Number of columns.</summary>
        public int Width { get; }

        /// <summary>Number of rows.</summary>
        public int Height { get; }

        /// <summary>Target cell of the last <see cref="Compute"/>.</summary>
        public Vector2Int Target => _target;

        /// <summary>True if <paramref name="cell"/> lies on the grid.</summary>
        public bool InBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        }

        /// <summary>True if <paramref name="cell"/> lies on the grid and is walkable.</summary>
        public bool IsWalkable(Vector2Int cell)
        {
            return InBounds(cell) && _walkable[Index(cell)];
        }

        /// <summary>Marks a cell as walkable or blocked, e.g. a wall or a closed gate. Takes effect on the next <see cref="Compute"/>.</summary>
        public void SetWalkable(Vector2Int cell, bool walkable)
        {
            if (InBounds(cell))
            {
                _walkable[Index(cell)] = walkable;
            }
        }

        /// <summary>
        /// Path cost from <paramref name="cell"/> to the target in steps of <see cref="OrthogonalCost"/>;
        /// <see cref="Unreachable"/> for blocked, enclosed or off-grid cells and before the first compute.
        /// </summary>
        public int Distance(Vector2Int cell)
        {
            return InBounds(cell) ? _distance[Index(cell)] : Unreachable;
        }

        /// <summary>True if the target can be reached from <paramref name="cell"/>.</summary>
        public bool IsReachable(Vector2Int cell)
        {
            return Distance(cell) != Unreachable;
        }

        /// <summary>Unit vector towards the next cell on the shortest path; zero on the target cell and where unreachable.</summary>
        public Vector2 Direction(Vector2Int cell)
        {
            return InBounds(cell) ? _direction[Index(cell)] : Vector2.zero;
        }

        /// <summary>Recomputes distances and directions towards <paramref name="target"/>. The target may be blocked, e.g. a player hugging a wall.</summary>
        public void Compute(Vector2Int target)
        {
            _target = target;
            _hasTarget = InBounds(target);
            for (int i = 0; i < _distance.Length; i++)
            {
                _distance[i] = Unreachable;
                _direction[i] = Vector2.zero;
            }

            if (!_hasTarget)
            {
                return;
            }

            _heapCount = 0;
            int start = Index(target);
            _distance[start] = 0;
            Push(start, 0);
            while (_heapCount > 0)
            {
                Pop(out int index, out int key);
                if (key > _distance[index])
                {
                    continue;
                }

                var cell = new Vector2Int(index % Width, index / Width);
                for (int s = 0; s < Steps.Length; s++)
                {
                    Vector2Int next = cell + Steps[s];
                    if (!CanStep(cell, Steps[s]) || !IsWalkable(next))
                    {
                        continue;
                    }

                    int nextIndex = Index(next);
                    int nextKey = key + StepCost(Steps[s]);
                    if (nextKey < _distance[nextIndex])
                    {
                        _distance[nextIndex] = nextKey;
                        Push(nextIndex, nextKey);
                    }
                }
            }

            ComputeDirections(start);
        }

        private void ComputeDirections(int targetIndex)
        {
            for (int index = 0; index < _distance.Length; index++)
            {
                if (index == targetIndex || _distance[index] == Unreachable)
                {
                    continue;
                }

                var cell = new Vector2Int(index % Width, index / Width);
                int best = _distance[index];
                Vector2Int bestStep = Vector2Int.zero;
                for (int s = 0; s < Steps.Length; s++)
                {
                    Vector2Int next = cell + Steps[s];
                    if (!InBounds(next) || !CanStep(cell, Steps[s]))
                    {
                        continue;
                    }

                    int distance = _distance[Index(next)];
                    if (distance < best)
                    {
                        best = distance;
                        bestStep = Steps[s];
                    }
                }

                _direction[index] = ((Vector2)bestStep).normalized;
            }
        }

        // Diagonal steps need both orthogonal neighbours free; the cells themselves are checked by the caller.
        private bool CanStep(Vector2Int from, Vector2Int step)
        {
            if (step.x == 0 || step.y == 0)
            {
                return true;
            }

            return IsWalkable(new Vector2Int(from.x + step.x, from.y)) && IsWalkable(new Vector2Int(from.x, from.y + step.y));
        }

        private static int StepCost(Vector2Int step)
        {
            return step.x != 0 && step.y != 0 ? DiagonalCost : OrthogonalCost;
        }

        private int Index(Vector2Int cell)
        {
            return cell.y * Width + cell.x;
        }

        private void Push(int cell, int key)
        {
            int i = _heapCount++;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_heapKey[parent] <= key)
                {
                    break;
                }

                _heapCell[i] = _heapCell[parent];
                _heapKey[i] = _heapKey[parent];
                i = parent;
            }

            _heapCell[i] = cell;
            _heapKey[i] = key;
        }

        private void Pop(out int cell, out int key)
        {
            cell = _heapCell[0];
            key = _heapKey[0];
            _heapCount--;
            if (_heapCount == 0)
            {
                return;
            }

            int lastCell = _heapCell[_heapCount];
            int lastKey = _heapKey[_heapCount];
            int i = 0;
            while (true)
            {
                int child = 2 * i + 1;
                if (child >= _heapCount)
                {
                    break;
                }

                if (child + 1 < _heapCount && _heapKey[child + 1] < _heapKey[child])
                {
                    child++;
                }

                if (_heapKey[child] >= lastKey)
                {
                    break;
                }

                _heapCell[i] = _heapCell[child];
                _heapKey[i] = _heapKey[child];
                i = child;
            }

            _heapCell[i] = lastCell;
            _heapKey[i] = lastKey;
        }
    }
}
