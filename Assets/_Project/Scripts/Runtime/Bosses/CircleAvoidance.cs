using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// Steering of a round body around one round obstacle (decision N2, the fountain of the endless arena): as long as
    /// the obstacle lies between the body and its target, the body heads along a tangent of the obstacle, on the side
    /// the target lies, so it slides along the round edge instead of pressing into it. Pure math, no physics.
    /// </summary>
    public static class CircleAvoidance
    {
        private const float Epsilon = 1e-5f;

        // How strongly a body already inside the clearance is pushed back out, per unit of overlap.
        private const float PushOut = 4f;

        /// <summary>
        /// Whether the obstacle at <paramref name="center"/> with <paramref name="clearance"/> (its radius plus the
        /// body's) blocks the way from <paramref name="position"/> to <paramref name="target"/> while moving along
        /// <paramref name="direction"/>.
        /// </summary>
        public static bool Blocks(Vector2 position, Vector2 direction, Vector2 target, Vector2 center, float clearance)
        {
            if (clearance <= 0f || direction.sqrMagnitude < Epsilon)
            {
                return false;
            }

            Vector2 toCenter = center - position;
            if (Vector2.Dot(direction, toCenter) <= 0f)
            {
                // Moving away from the obstacle (or past it).
                return false;
            }

            Vector2 path = target - position;
            float length = path.sqrMagnitude;
            float t = length > Epsilon ? Mathf.Clamp01(Vector2.Dot(toCenter, path) / length) : 0f;
            Vector2 closest = position + path * t;
            return (closest - center).sqrMagnitude < clearance * clearance;
        }

        /// <summary>
        /// The direction to move in from <paramref name="position"/> towards <paramref name="target"/>, given the wanted
        /// unit <paramref name="direction"/>: unchanged if the obstacle is not in the way, otherwise along the tangent
        /// past the obstacle on the target's side (counterclockwise if the target lies exactly behind its center).
        /// Inside the clearance the tangent is bent outwards. Returns a unit vector, or zero for a zero direction.
        /// </summary>
        public static Vector2 Steer(Vector2 position, Vector2 direction, Vector2 target, Vector2 center, float clearance)
        {
            if (!Blocks(position, direction, target, center, clearance))
            {
                return direction;
            }

            Vector2 toCenter = center - position;
            float distance = toCenter.magnitude;
            if (distance < Epsilon)
            {
                return direction;
            }

            Vector2 inward = toCenter / distance;
            // Left of the line to the center is counterclockwise around the obstacle.
            float cross = inward.x * (target - position).y - inward.y * (target - position).x;
            float side = cross < 0f ? -1f : 1f;

            if (distance > clearance)
            {
                // Outside: head for the tangent point, the shortest way past.
                float angle = Mathf.Asin(Mathf.Clamp01(clearance / distance)) * side;
                return Rotate(inward, angle);
            }

            // Touching or inside: follow the edge, pushed back out by the overlap.
            Vector2 tangent = new Vector2(-inward.y, inward.x) * side;
            Vector2 steered = tangent - inward * ((clearance - distance) * PushOut);
            return steered.normalized;
        }

        private static Vector2 Rotate(Vector2 vector, float radians)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
        }
    }
}
