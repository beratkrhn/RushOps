using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>Rules for picking spawn points (F-GEG-02): no spawning in sight of or right next to the player.</summary>
    public static class SpawnRules
    {
        /// <summary>
        /// True if an enemy may appear at <paramref name="point"/>: outside <paramref name="view"/> (the camera view in
        /// world units, already including any margin) and at least <paramref name="minDistance"/> from the player.
        /// </summary>
        public static bool IsAllowed(Vector2 point, Rect view, Vector2 player, float minDistance)
        {
            return !view.Contains(point) && (point - player).sqrMagnitude >= minDistance * minDistance;
        }

        /// <summary>Camera view rectangle around <paramref name="center"/>, grown by <paramref name="margin"/> on each side.</summary>
        public static Rect View(Vector2 center, Vector2 halfExtents, float margin)
        {
            Vector2 half = halfExtents + new Vector2(margin, margin);
            return new Rect(center - half, half * 2f);
        }

        /// <summary>Number of enemies that may still be announced: the cap minus alive and announced ones, never negative.</summary>
        public static int FreeSlots(int maxActive, int alive, int announced)
        {
            int free = maxActive - alive - announced;
            return free > 0 ? free : 0;
        }

        /// <summary>
        /// Picks one of the <paramref name="count"/> candidate indices in <paramref name="candidates"/> by
        /// <paramref name="random01"/> in [0, 1); -1 if there is none.
        /// </summary>
        public static int Pick(int[] candidates, int count, float random01)
        {
            if (count <= 0)
            {
                return -1;
            }

            int slot = Mathf.Clamp((int)(random01 * count), 0, count - 1);
            return candidates[slot];
        }
    }
}
