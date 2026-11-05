using UnityEngine;

namespace RushOps.Player
{
    /// <summary>How the gun in the player's hand is placed for one facing direction (see <see cref="PlayerGunMath"/>).</summary>
    public readonly struct GunPose
    {
        public GunPose(Vector2 grip, float angle, bool flipY, bool behindBody)
        {
            Grip = grip;
            Angle = angle;
            FlipY = flipY;
            BehindBody = behindBody;
        }

        /// <summary>Grip position relative to the feet (transform pivot), in units.</summary>
        public Vector2 Grip { get; }

        /// <summary>Rotation in degrees, counter-clockwise from +X.</summary>
        public float Angle { get; }

        /// <summary>True if the sprite is mirrored vertically so the gun stays upright when pointing left.</summary>
        public bool FlipY { get; }

        /// <summary>True if the gun is drawn behind the body (pointing away from the camera).</summary>
        public bool BehindBody { get; }
    }
}
