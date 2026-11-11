using System;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Rules of the cracked wall (issue #287, F-RAE-11, N27): only a boomer's explosion breaks it, if its center is
    /// closer to the wall than the explosion radius plus an extra reach. Other explosions and shots never do.
    /// </summary>
    public sealed class CrackedWallLogic
    {
        private readonly Rect _bounds;
        private readonly float _extraReach;

        /// <param name="bounds">The wall in world units.</param>
        /// <param name="extraReach">Tiles beyond the explosion radius that still count.</param>
        public CrackedWallLogic(Rect bounds, float extraReach)
        {
            _bounds = bounds;
            _extraReach = Mathf.Max(0f, extraReach);
        }

        /// <summary>Raised once when the wall breaks.</summary>
        public event Action Broken;

        /// <summary>True once the wall is broken.</summary>
        public bool IsBroken { get; private set; }

        /// <summary>Distance from <paramref name="point"/> to the wall, 0 inside it.</summary>
        public float DistanceTo(Vector2 point)
        {
            float dx = Mathf.Max(_bounds.xMin - point.x, 0f, point.x - _bounds.xMax);
            float dy = Mathf.Max(_bounds.yMin - point.y, 0f, point.y - _bounds.yMax);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>True if an explosion at <paramref name="center"/> with <paramref name="radius"/> is in reach.</summary>
        public bool InReach(Vector2 center, float radius)
        {
            return DistanceTo(center) <= Mathf.Max(0f, radius) + _extraReach;
        }

        /// <summary>
        /// Reports an explosion; breaks the wall if it came <paramref name="fromBoomer"/> and is in reach.
        /// </summary>
        /// <returns>True if this explosion broke the wall.</returns>
        public bool NotifyExplosion(Vector2 center, float radius, bool fromBoomer)
        {
            if (IsBroken || !fromBoomer || !InReach(center, radius))
            {
                return false;
            }

            IsBroken = true;
            Broken?.Invoke();
            return true;
        }
    }
}
