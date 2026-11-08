using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// Something area weapons can hit through its hurtbox, enemies and bosses alike, e.g. the flamethrower
    /// (issue #254). Hits go through <see cref="IDamageable"/>.
    /// </summary>
    public interface IAreaTarget : IDamageable
    {
        /// <summary>Center of the body; the transform sits at the feet.</summary>
        Vector2 AimPoint { get; }

        /// <summary>True while a hit could count: alive and in the game.</summary>
        bool CanBeHit { get; }
    }
}
