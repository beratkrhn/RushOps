using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>Something an explosion can hit, e.g. an enemy (issue #22).</summary>
    public interface IExplosionTarget
    {
        /// <summary>Center of the body in world units; it must lie inside the radius to be hit.</summary>
        Vector2 ExplosionPoint { get; }

        /// <summary>False for targets that cannot be hit any more, e.g. dying enemies.</summary>
        bool CanBeHitByExplosion { get; }
    }
}
