using UnityEngine;

namespace RushOps.Navigation
{
    /// <summary>Steering rules for enemies following a <see cref="FlowField"/> (ADR 0003).</summary>
    public static class SteeringMath
    {
        // Golden angle in radians: consecutive indices get well spread, never opposite directions.
        private const float GoldenAngle = 2.39996323f;

        /// <summary>
        /// Push away from neighbours closer than <paramref name="radius"/>: each contributes its direction away
        /// from <paramref name="self"/>, weighted 1 at zero distance down to 0 at the radius. Entry
        /// <paramref name="selfIndex"/> of <paramref name="positions"/> is skipped. The result is not normalized.
        /// </summary>
        public static Vector2 Separation(Vector2 self, Vector2[] positions, int count, int selfIndex, float radius)
        {
            Vector2 push = Vector2.zero;
            if (radius <= 0f)
            {
                return push;
            }

            float radiusSquared = radius * radius;
            for (int i = 0; i < count; i++)
            {
                if (i == selfIndex)
                {
                    continue;
                }

                Vector2 away = self - positions[i];
                float distanceSquared = away.sqrMagnitude;
                if (distanceSquared >= radiusSquared)
                {
                    continue;
                }

                if (distanceSquared < 1e-8f)
                {
                    // Exactly on top of each other there is no away direction. Each agent picks its own by index;
                    // opposite pushes would not do, physics cancels them for coincident bodies and they stay stuck.
                    float angle = selfIndex * GoldenAngle;
                    push += new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    continue;
                }

                float distance = Mathf.Sqrt(distanceSquared);
                push += away / distance * (1f - distance / radius);
            }

            return push;
        }

        /// <summary>
        /// Final move direction: the path direction plus the weighted separation, at most length 1.
        /// Separation alone may slow an enemy down but never makes it faster than its speed.
        /// </summary>
        public static Vector2 Combine(Vector2 path, Vector2 separation, float separationWeight)
        {
            return Vector2.ClampMagnitude(path + separation * separationWeight, 1f);
        }
    }
}
