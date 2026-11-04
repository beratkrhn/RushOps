using UnityEngine;

namespace RushOps.Perks
{
    /// <summary>Which enemies the nuke kills (F-PRK-04, F-BOS-02, issue #27): living, visible ones that are no boss.</summary>
    public static class NukeTargeting
    {
        /// <summary>
        /// True if <paramref name="point"/> lies in the view around <paramref name="viewCenter"/> with
        /// <paramref name="halfExtents"/>, edges included.
        /// </summary>
        public static bool IsInView(Vector2 viewCenter, Vector2 halfExtents, Vector2 point)
        {
            Vector2 offset = point - viewCenter;
            return Mathf.Abs(offset.x) <= halfExtents.x && Mathf.Abs(offset.y) <= halfExtents.y;
        }

        /// <summary>True if the nuke kills an enemy: alive, not a boss and inside the view.</summary>
        public static bool Kills(bool isAlive, bool isBoss, bool isInView)
        {
            return isAlive && !isBoss && isInView;
        }
    }
}
