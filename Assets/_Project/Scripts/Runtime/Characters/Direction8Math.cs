using UnityEngine;

namespace RushOps.Characters
{
    /// <summary>
    /// Converts between vectors and <see cref="Direction8"/>.
    /// Each direction covers a 45 degree sector centred on its angle.
    /// </summary>
    public static class Direction8Math
    {
        /// <summary>Number of directions.</summary>
        public const int Count = 8;

        private const float SectorDegrees = 360f / Count;
        private const float MinSqrMagnitude = 1e-6f;

        /// <summary>
        /// Returns the direction whose sector contains <paramref name="vector"/>.
        /// Sector borders belong to the counter-clockwise neighbour (22.5 degrees is NE).
        /// A (near) zero vector returns <paramref name="fallback"/>.
        /// </summary>
        public static Direction8 FromVector(Vector2 vector, Direction8 fallback)
        {
            if (vector.sqrMagnitude < MinSqrMagnitude)
            {
                return fallback;
            }

            float degrees = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
            int index = Mathf.FloorToInt(degrees / SectorDegrees + 0.5f);
            return (Direction8)(((index % Count) + Count) % Count);
        }

        /// <summary>Returns the unit vector pointing in <paramref name="direction"/>.</summary>
        public static Vector2 ToVector(Direction8 direction)
        {
            float radians = (int)direction * SectorDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
    }
}
