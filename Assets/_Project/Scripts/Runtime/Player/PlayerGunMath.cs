using UnityEngine;

namespace RushOps.Player
{
    /// <summary>
    /// Rules for the gun in the player's hand: the grip sits at hand height in front of the body along the facing
    /// direction, the muzzle one barrel length further. Projectiles and the muzzle flash start at the muzzle.
    /// </summary>
    public static class PlayerGunMath
    {
        /// <summary>Facing directions with a larger upward share than this hold the gun behind the body.</summary>
        public const float BehindBodyThreshold = 0.35f;

        /// <summary>Grip position relative to the feet for <paramref name="facing"/> (any length; zero points nowhere).</summary>
        public static Vector2 Grip(Vector2 facing, float height, float distance)
        {
            return Vector2.up * height + facing.normalized * distance;
        }

        /// <summary>Muzzle position relative to the feet for <paramref name="facing"/>.</summary>
        public static Vector2 Muzzle(Vector2 facing, float height, float gripDistance, float barrelLength)
        {
            return Grip(facing, height, gripDistance) + facing.normalized * barrelLength;
        }

        /// <summary>Pose of the gun for <paramref name="facing"/>; a zero vector points right.</summary>
        public static GunPose Pose(Vector2 facing, float height, float gripDistance)
        {
            Vector2 unit = facing.normalized;
            if (unit == Vector2.zero)
            {
                unit = Vector2.right;
            }

            float angle = Mathf.Atan2(unit.y, unit.x) * Mathf.Rad2Deg;
            return new GunPose(Grip(unit, height, gripDistance), angle, unit.x < 0f, unit.y > BehindBodyThreshold);
        }
    }
}
