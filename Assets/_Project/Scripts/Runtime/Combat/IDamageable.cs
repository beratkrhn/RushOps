using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>Something projectiles can hurt, e.g. an enemy.</summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies <paramref name="damage"/> from a hit travelling along <paramref name="direction"/>. A
        /// <paramref name="lethal"/> hit (insta-kill, issue #25) kills, unless the target is a boss (F-BOS-02).
        /// </summary>
        /// <returns>True if the hit counted.</returns>
        bool TakeHit(int damage, Vector2 direction, bool lethal = false);
    }
}
