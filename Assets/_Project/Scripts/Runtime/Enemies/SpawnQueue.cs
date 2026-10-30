using System;

namespace RushOps.Enemies
{
    /// <summary>
    /// Spawns waiting for a free slot or a usable spawn point (F-GEG-03), oldest first. Allocates only when it
    /// has to grow.
    /// </summary>
    public class SpawnQueue
    {
        private SpawnRequest[] _items = new SpawnRequest[16];

        /// <summary>Number of waiting spawns.</summary>
        public int Count { get; private set; }

        /// <summary>The waiting spawn at <paramref name="index"/>, 0 being the oldest.</summary>
        public SpawnRequest this[int index] => _items[index];

        /// <summary>Adds a spawn at the end.</summary>
        public void Enqueue(SpawnRequest request)
        {
            EnsureCapacity();
            _items[Count++] = request;
        }

        /// <summary>Puts a spawn back at the front, e.g. when its spawn point turned out to be unusable.</summary>
        public void PushFront(SpawnRequest request)
        {
            EnsureCapacity();
            Array.Copy(_items, 0, _items, 1, Count);
            _items[0] = request;
            Count++;
        }

        /// <summary>Removes and returns the spawn at <paramref name="index"/>.</summary>
        public SpawnRequest RemoveAt(int index)
        {
            SpawnRequest request = _items[index];
            Count--;
            Array.Copy(_items, index + 1, _items, index, Count - index);
            _items[Count] = default;
            return request;
        }

        /// <summary>Drops all spawns of zone <paramref name="owner"/>, e.g. when it was completed. Returns how many.</summary>
        public int RemoveOwner(int owner)
        {
            int kept = 0;
            for (int i = 0; i < Count; i++)
            {
                if (_items[i].Owner != owner)
                {
                    _items[kept++] = _items[i];
                }
            }

            int removed = Count - kept;
            for (int i = kept; i < Count; i++)
            {
                _items[i] = default;
            }

            Count = kept;
            return removed;
        }

        /// <summary>
        /// Index of the oldest spawn that may happen now, or -1: bosses always may, other enemies only while
        /// <paramref name="freeSlots"/> is above zero; in both cases <paramref name="canPlace"/> must accept it.
        /// </summary>
        public int FindReady(int freeSlots, Func<SpawnRequest, bool> canPlace)
        {
            for (int i = 0; i < Count; i++)
            {
                SpawnRequest request = _items[i];
                if ((request.IsBoss || freeSlots > 0) && (canPlace == null || canPlace(request)))
                {
                    return i;
                }
            }

            return -1;
        }

        private void EnsureCapacity()
        {
            if (Count == _items.Length)
            {
                Array.Resize(ref _items, _items.Length * 2);
            }
        }
    }
}
