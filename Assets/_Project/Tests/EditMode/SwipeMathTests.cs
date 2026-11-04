using NUnit.Framework;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class SwipeMathTests
    {
        [Test]
        public void Vertical_UpAndDown()
        {
            Assert.AreEqual(1, SwipeMath.Vertical(new Vector2(100, 100), new Vector2(105, 200), 50f));
            Assert.AreEqual(-1, SwipeMath.Vertical(new Vector2(100, 300), new Vector2(90, 150), 50f));
        }

        [Test]
        public void Vertical_IgnoresTapsShortMovesAndSidewaysSwipes()
        {
            Assert.AreEqual(0, SwipeMath.Vertical(new Vector2(100, 100), new Vector2(100, 100), 50f));
            Assert.AreEqual(0, SwipeMath.Vertical(new Vector2(100, 100), new Vector2(100, 140), 50f));
            Assert.AreEqual(0, SwipeMath.Vertical(new Vector2(100, 100), new Vector2(300, 170), 50f));
        }

        [Test]
        public void Approach_MovesPartWayAndSnapsAtTheEnd()
        {
            float next = SwipeMath.Approach(0f, 640f, 12f, 1f / 60f);
            Assert.Greater(next, 0f);
            Assert.Less(next, 640f);

            Assert.AreEqual(640f, SwipeMath.Approach(639.8f, 640f, 12f, 1f / 60f));
            Assert.AreEqual(640f, SwipeMath.Approach(0f, 640f, 0f, 1f / 60f), "no sharpness jumps");
        }

        [Test]
        public void Approach_IsIndependentOfTheFrameRate()
        {
            float a = 0f;
            for (int i = 0; i < 60; i++)
            {
                a = SwipeMath.Approach(a, 640f, 3f, 1f / 60f);
            }

            float b = 0f;
            for (int i = 0; i < 30; i++)
            {
                b = SwipeMath.Approach(b, 640f, 3f, 1f / 30f);
            }

            Assert.AreEqual(a, b, 1f);
        }

        [Test]
        public void Horizontal_RightAndLeft()
        {
            Assert.AreEqual(1, SwipeMath.Horizontal(new Vector2(100, 100), new Vector2(200, 110), 50f));
            Assert.AreEqual(-1, SwipeMath.Horizontal(new Vector2(300, 100), new Vector2(150, 90), 50f));
        }

        [Test]
        public void Horizontal_IgnoresTapsShortMovesAndVerticalSwipes()
        {
            Assert.AreEqual(0, SwipeMath.Horizontal(new Vector2(100, 100), new Vector2(100, 100), 50f));
            Assert.AreEqual(0, SwipeMath.Horizontal(new Vector2(100, 100), new Vector2(140, 100), 50f));
            Assert.AreEqual(0, SwipeMath.Horizontal(new Vector2(100, 100), new Vector2(170, 200), 50f));
        }

        [Test]
        public void SideBlend_IsZeroInTheMiddleAndOneAtTheNeighbour()
        {
            Assert.AreEqual(0f, SwipeMath.SideBlend(300f, 300f, 150f));
            Assert.AreEqual(0.5f, SwipeMath.SideBlend(375f, 300f, 150f), 1e-5f);
            Assert.AreEqual(1f, SwipeMath.SideBlend(0f, 300f, 150f));
            Assert.AreEqual(1f, SwipeMath.SideBlend(0f, 0f, 0f));
        }
    }
}
