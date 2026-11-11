using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// The burning tiles of the fire trail (F-END-12, issue #278): while the player dashes, a tile is laid every
    /// <c>spacing</c> units along the path and burns for a fixed time. Holds up to a fixed number of tiles; when full,
    /// the oldest goes out for the new one. Allocates nothing after construction.
    /// </summary>
    public sealed class FireTrailTiles
    {
        private readonly Vector2[] _positions;
        private readonly float[] _remaining;
        private int _count;
        private bool _hasLast;
        private Vector2 _last;

        /// <param name="capacity">Most tiles at once, at least 1.</param>
        public FireTrailTiles(int capacity)
        {
            capacity = Mathf.Max(1, capacity);
            _positions = new Vector2[capacity];
            _remaining = new float[capacity];
        }

        /// <summary>Tiles burning now, oldest first.</summary>
        public int Count => _count;

        /// <summary>Most tiles at once.</summary>
        public int Capacity => _positions.Length;

        /// <summary>Position of the tile at <paramref name="index"/>, 0..<see cref="Count"/>-1.</summary>
        public Vector2 Position(int index)
        {
            return _positions[index];
        }

        /// <summary>Seconds the tile at <paramref name="index"/> still burns.</summary>
        public float Remaining(int index)
        {
            return _remaining[index];
        }

        /// <summary>
        /// Lays a tile at <paramref name="position"/> for <paramref name="duration"/> seconds if it is the first of a dash
        /// or at least <paramref name="spacing"/> away from the last one. Returns true if a tile was laid.
        /// </summary>
        public bool TryLay(Vector2 position, float spacing, float duration)
        {
            if (duration <= 0f || (_hasLast && (position - _last).sqrMagnitude < spacing * spacing))
            {
                return false;
            }

            if (_count == _positions.Length)
            {
                RemoveAt(0);
            }

            _positions[_count] = position;
            _remaining[_count] = duration;
            _count++;
            _last = position;
            _hasLast = true;
            return true;
        }

        /// <summary>Ends the chain of the current dash, so the next dash lays its first tile at once.</summary>
        public void BreakChain()
        {
            _hasLast = false;
        }

        /// <summary>Burns the tiles down by <paramref name="deltaTime"/>; tiles that ran out go out.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            int kept = 0;
            for (int i = 0; i < _count; i++)
            {
                float left = _remaining[i] - deltaTime;
                if (left > 1e-4f)
                {
                    _positions[kept] = _positions[i];
                    _remaining[kept] = left;
                    kept++;
                }
            }

            _count = kept;
        }

        /// <summary>
        /// Whether <paramref name="point"/> lies within <paramref name="radius"/> of any burning tile, e.g. the player,
        /// for tests; the trail itself never hurts the player.
        /// </summary>
        public bool IsBurning(Vector2 point, float radius)
        {
            float squared = radius * radius;
            for (int i = 0; i < _count; i++)
            {
                if ((_positions[i] - point).sqrMagnitude <= squared)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Puts out every tile, e.g. on death or a new run.</summary>
        public void Clear()
        {
            _count = 0;
            _hasLast = false;
        }

        private void RemoveAt(int index)
        {
            for (int i = index + 1; i < _count; i++)
            {
                _positions[i - 1] = _positions[i];
                _remaining[i - 1] = _remaining[i];
            }

            _count--;
        }
    }
}
