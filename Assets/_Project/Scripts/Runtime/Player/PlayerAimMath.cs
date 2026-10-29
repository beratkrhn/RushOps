using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Pure aiming rules: dead zone of the aim stick, pointer aiming and which way the player faces.
    /// </summary>
    public static class PlayerAimMath
    {
        private const float MinSqrMagnitude = 1e-6f;

        /// <summary>
        /// True if the aim stick is deflected beyond <paramref name="deadzone"/> (0..1),
        /// which also means the weapon fires.
        /// </summary>
        public static bool IsAiming(Vector2 aim, float deadzone)
        {
            float magnitude = aim.magnitude;
            return magnitude > MinSqrMagnitude && magnitude > deadzone;
        }

        /// <summary>
        /// Direction the character sprite faces: the aim direction while aiming, otherwise the move direction.
        /// A zero result means "keep the current facing".
        /// </summary>
        public static Vector2 FacingVector(Vector2 move, Vector2 aim, float deadzone)
        {
            return IsAiming(aim, deadzone) ? aim : move;
        }

        /// <summary>
        /// Full-deflection aim vector from <paramref name="origin"/> towards <paramref name="target"/>
        /// (e.g. the mouse cursor in world space). Returns zero if both points coincide.
        /// </summary>
        public static Vector2 PointerAim(Vector2 origin, Vector2 target)
        {
            Vector2 offset = target - origin;
            return offset.sqrMagnitude < MinSqrMagnitude ? Vector2.zero : offset.normalized;
        }
    }
}
