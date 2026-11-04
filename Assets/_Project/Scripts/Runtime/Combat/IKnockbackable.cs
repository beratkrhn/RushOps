using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>Something that can be pushed away, e.g. an enemy touching the shield (issue #26).</summary>
    public interface IKnockbackable
    {
        /// <summary>
        /// Pushes along <paramref name="direction"/> with <paramref name="speed"/> units per second, easing out over
        /// <paramref name="duration"/> seconds. Heavy targets may take only a part of the speed.
        /// </summary>
        /// <returns>True if the push was applied.</returns>
        bool Push(Vector2 direction, float speed, float duration);
    }
}
