using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// Explosions waiting to go off (issue #22): a boomer shot dead blows up right away, one killed by another
    /// explosion after a short delay, so chain reactions run visibly one after another.
    /// </summary>
    public class ExplosionQueue
    {
        private readonly List<PendingExplosion> _pending;

        /// <param name="capacity">Expected number of explosions waiting at once; more still work.</param>
        public ExplosionQueue(int capacity = 16)
        {
            _pending = new List<PendingExplosion>(capacity);
        }

        /// <summary>Number of explosions waiting.</summary>
        public int Count => _pending.Count;

        /// <summary>Adds an explosion at <paramref name="position"/> that goes off after <paramref name="delay"/> seconds.</summary>
        public void Schedule(Vector2 position, ExplosionDefinition definition, float delay)
        {
            _pending.Add(new PendingExplosion(position, definition, delay > 0f ? delay : 0f));
        }

        /// <summary>Advances all waiting explosions by <paramref name="deltaTime"/> seconds.</summary>
        public void Advance(float deltaTime)
        {
            for (int i = 0; i < _pending.Count; i++)
            {
                PendingExplosion pending = _pending[i];
                pending.Remaining -= deltaTime;
                _pending[i] = pending;
            }
        }

        /// <summary>Takes the oldest explosion that is due, if any.</summary>
        public bool TryTakeDue(out PendingExplosion explosion)
        {
            for (int i = 0; i < _pending.Count; i++)
            {
                if (_pending[i].Remaining <= 0f)
                {
                    explosion = _pending[i];
                    _pending.RemoveAt(i);
                    return true;
                }
            }

            explosion = default;
            return false;
        }

        /// <summary>Drops all waiting explosions.</summary>
        public void Clear()
        {
            _pending.Clear();
        }
    }
}
