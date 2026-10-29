using NUnit.Framework;
using RushOps.Cameras;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class CameraFollowMathTests
    {
        // 40x60 level, view of 180x320 px at 16 PPU -> half extents 5.625 x 10.
        private static readonly Rect Level = new Rect(0f, 0f, 40f, 60f);
        private static readonly Vector2 HalfView = new Vector2(5.625f, 10f);

        [Test]
        public void ClampToBounds_InsideLevel_ReturnsCenterUnchanged()
        {
            var center = new Vector2(20f, 30f);

            Assert.AreEqual(center, CameraFollowMath.ClampToBounds(center, Level, HalfView));
        }

        [Test]
        public void ClampToBounds_NearBottomLeftCorner_KeepsViewInsideLevel()
        {
            Vector2 result = CameraFollowMath.ClampToBounds(new Vector2(1f, 2f), Level, HalfView);

            Assert.AreEqual(5.625f, result.x, 1e-5f);
            Assert.AreEqual(10f, result.y, 1e-5f);
        }

        [Test]
        public void ClampToBounds_BeyondTopRightCorner_KeepsViewInsideLevel()
        {
            Vector2 result = CameraFollowMath.ClampToBounds(new Vector2(100f, 100f), Level, HalfView);

            Assert.AreEqual(40f - 5.625f, result.x, 1e-5f);
            Assert.AreEqual(50f, result.y, 1e-5f);
        }

        [Test]
        public void ClampToBounds_LevelSmallerThanView_CentersOnThatAxis()
        {
            var narrowLevel = new Rect(10f, 0f, 8f, 60f);

            Vector2 result = CameraFollowMath.ClampToBounds(new Vector2(11f, 30f), narrowLevel, HalfView);

            Assert.AreEqual(14f, result.x, 1e-5f);
            Assert.AreEqual(30f, result.y, 1e-5f);
        }

        [Test]
        public void SnapToPixelGrid_RoundsToNearestArtPixel()
        {
            Vector2 result = CameraFollowMath.SnapToPixelGrid(new Vector2(1.03f, -2.04f), 16);

            Assert.AreEqual(1.0f, result.x, 1e-5f);
            Assert.AreEqual(-2.0625f, result.y, 1e-5f);
        }

        [Test]
        public void SnapToPixelGrid_AlreadyOnGrid_ReturnsSamePosition()
        {
            var position = new Vector2(3.5625f, 7.125f);

            Assert.AreEqual(position, CameraFollowMath.SnapToPixelGrid(position, 16));
        }

        [Test]
        public void SnapToPixelGrid_NonPositivePixelsPerUnit_ReturnsInput()
        {
            var position = new Vector2(1.234f, 5.678f);

            Assert.AreEqual(position, CameraFollowMath.SnapToPixelGrid(position, 0));
        }
    }
}
