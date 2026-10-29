using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Pure movement rules: analog target speed from the stick and acceleration towards it.
    /// </summary>
    public static class PlayerMovementMath
    {
        /// <summary>
        /// Velocity the player aims for: stick direction times <paramref name="moveSpeed"/>,
        /// scaled by stick deflection. Deflection above 1 (e.g. diagonal keys) is clamped to 1.
        /// </summary>
        public static Vector2 TargetVelocity(Vector2 input, float moveSpeed)
        {
            return Vector2.ClampMagnitude(input, 1f) * moveSpeed;
        }

        /// <summary>
        /// Moves <paramref name="current"/> towards <paramref name="target"/> by one step of length <paramref name="deltaTime"/>.
        /// Uses <paramref name="acceleration"/> while speeding up and <paramref name="deceleration"/> while slowing down.
        /// </summary>
        public static Vector2 StepVelocity(Vector2 current, Vector2 target, float acceleration, float deceleration, float deltaTime)
        {
            bool speedingUp = target.sqrMagnitude > current.sqrMagnitude;
            float rate = speedingUp ? acceleration : deceleration;
            return Vector2.MoveTowards(current, target, rate * deltaTime);
        }
    }
}
