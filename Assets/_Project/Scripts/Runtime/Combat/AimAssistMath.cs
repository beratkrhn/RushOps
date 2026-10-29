using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>Target choice of the aim assist (F-STE-06).</summary>
    public static class AimAssistMath
    {
        /// <summary>
        /// Index of the target the shot should be corrected to: among the first <paramref name="count"/>
        /// <paramref name="targets"/> within <paramref name="range"/> of <paramref name="origin"/> and at most
        /// <paramref name="maxAngle"/> degrees off <paramref name="aim"/>, the nearest one; -1 if none.
        /// </summary>
        public static int Pick(Vector2 origin, Vector2 aim, Vector2[] targets, int count, float maxAngle, float range)
        {
            if (aim == Vector2.zero || maxAngle <= 0f || range <= 0f)
            {
                return -1;
            }

            Vector2 direction = aim.normalized;
            float minCos = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
            float rangeSquared = range * range;
            int best = -1;
            float bestDistanceSquared = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Vector2 toTarget = targets[i] - origin;
                float distanceSquared = toTarget.sqrMagnitude;
                if (distanceSquared > rangeSquared || distanceSquared < 1e-8f || distanceSquared >= bestDistanceSquared)
                {
                    continue;
                }

                float cos = Vector2.Dot(toTarget / Mathf.Sqrt(distanceSquared), direction);
                if (cos >= minCos)
                {
                    best = i;
                    bestDistanceSquared = distanceSquared;
                }
            }

            return best;
        }

        /// <summary>Shot direction: towards the picked target, or <paramref name="aim"/> normalized if there is none.</summary>
        public static Vector2 Adjust(Vector2 origin, Vector2 aim, Vector2[] targets, int count, float maxAngle, float range)
        {
            int index = Pick(origin, aim, targets, count, maxAngle, range);
            return index >= 0 ? (targets[index] - origin).normalized : aim.normalized;
        }
    }
}
