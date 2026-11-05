using NUnit.Framework;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class PlayerGunMathTests
    {
        private const float Height = 0.6f;
        private const float Grip = 0.2f;
        private const float Barrel = 0.19f;

        [Test]
        public void Muzzle_LiesOneBarrelInFrontOfTheGrip()
        {
            Vector2 grip = PlayerGunMath.Grip(Vector2.right, Height, Grip);
            Vector2 muzzle = PlayerGunMath.Muzzle(Vector2.right, Height, Grip, Barrel);

            Assert.AreEqual(new Vector2(Grip, Height), grip);
            Assert.AreEqual(Grip + Barrel, muzzle.x, 1e-5f);
            Assert.AreEqual(Height, muzzle.y, 1e-5f);
        }

        [Test]
        public void Muzzle_IgnoresTheLengthOfTheDirection()
        {
            Assert.AreEqual(
                PlayerGunMath.Muzzle(Vector2.left, Height, Grip, Barrel),
                PlayerGunMath.Muzzle(Vector2.left * 3f, Height, Grip, Barrel));
        }

        [Test]
        public void Pose_PointingRight_IsUprightInFront()
        {
            GunPose pose = PlayerGunMath.Pose(Vector2.right, Height, Grip);

            Assert.AreEqual(0f, pose.Angle, 1e-4f);
            Assert.IsFalse(pose.FlipY);
            Assert.IsFalse(pose.BehindBody);
        }

        [Test]
        public void Pose_PointingLeft_IsFlippedToStayUpright()
        {
            GunPose pose = PlayerGunMath.Pose(Vector2.left, Height, Grip);

            Assert.AreEqual(180f, pose.Angle, 1e-4f);
            Assert.IsTrue(pose.FlipY);
        }

        [TestCase(0f, 1f, true)]
        [TestCase(0.7f, 0.7f, true)]
        [TestCase(1f, 0.2f, false)]
        [TestCase(0f, -1f, false)]
        public void Pose_PointingAwayFromTheCamera_IsBehindTheBody(float x, float y, bool behind)
        {
            Assert.AreEqual(behind, PlayerGunMath.Pose(new Vector2(x, y), Height, Grip).BehindBody);
        }

        [Test]
        public void Pose_WithoutDirection_PointsRight()
        {
            GunPose pose = PlayerGunMath.Pose(Vector2.zero, Height, Grip);

            Assert.AreEqual(0f, pose.Angle);
            Assert.AreEqual(new Vector2(Grip, Height), pose.Grip);
        }
    }
}
