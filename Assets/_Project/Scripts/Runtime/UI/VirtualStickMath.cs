using UnityEngine;

namespace RushOps.UI
{
    /// <summary>
    /// Rules of the virtual sticks (F-STE-05). All positions are in zone space: (0, 0) is the bottom-left
    /// corner of the stick's touch zone (its half of the safe area), <c>zoneSize</c> its top-right corner.
    /// </summary>
    public static class VirtualStickMath
    {
        /// <summary>Resting position of a stick: in the bottom corner of its zone on its side, <paramref name="margin"/> from the edges.</summary>
        public static Vector2 HomePosition(Vector2 zoneSize, StickSide side, float radius, float margin)
        {
            float offset = margin + radius;
            float x = side == StickSide.Left ? offset : zoneSize.x - offset;
            return ClampToZone(new Vector2(x, offset), zoneSize, radius);
        }

        /// <summary>
        /// Moves <paramref name="point"/> so a stick of <paramref name="radius"/> centered on it lies fully inside the zone.
        /// On an axis shorter than the stick, the stick is centered.
        /// </summary>
        public static Vector2 ClampToZone(Vector2 point, Vector2 zoneSize, float radius)
        {
            return new Vector2(ClampAxis(point.x, zoneSize.x, radius), ClampAxis(point.y, zoneSize.y, radius));
        }

        /// <summary>
        /// True if a touch at <paramref name="press"/> takes the stick: in floating mode anywhere in the zone,
        /// in fixed mode only within <paramref name="grabRadius"/> of the home position.
        /// </summary>
        public static bool AcceptsPress(StickMode mode, Vector2 home, Vector2 press, float grabRadius)
        {
            return mode == StickMode.Floating || (press - home).sqrMagnitude <= grabRadius * grabRadius;
        }

        /// <summary>Center of the stick while a touch holds it: the home position when fixed, the clamped touch point when floating.</summary>
        public static Vector2 Origin(StickMode mode, Vector2 home, Vector2 press, Vector2 zoneSize, float radius)
        {
            return mode == StickMode.Fixed ? home : ClampToZone(press, zoneSize, radius);
        }

        /// <summary>Stick value from the origin to the touch: length 1 at <paramref name="radius"/> and beyond, like a gamepad stick.</summary>
        public static Vector2 Value(Vector2 origin, Vector2 pointer, float radius)
        {
            if (radius <= 0f)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude((pointer - origin) / radius, 1f);
        }

        /// <summary>Knob position for a stick value: the origin plus the value scaled to the radius.</summary>
        public static Vector2 KnobPosition(Vector2 origin, Vector2 value, float radius)
        {
            return origin + value * radius;
        }

        /// <summary>Center of the dash button: straight above the stick's home position, <paramref name="gap"/> above its base (F-STE-03).</summary>
        public static Vector2 DashButtonPosition(Vector2 stickHome, float stickRadius, float buttonSize, float gap)
        {
            return stickHome + new Vector2(0f, stickRadius + gap + buttonSize * 0.5f);
        }

        private static float ClampAxis(float value, float length, float radius)
        {
            if (length <= 2f * radius)
            {
                return length * 0.5f;
            }

            return Mathf.Clamp(value, radius, length - radius);
        }
    }
}
