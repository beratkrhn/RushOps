using NUnit.Framework;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class SafeAreaMathTests
    {
        [Test]
        public void Anchors_FullScreen_SpanZeroToOne()
        {
            SafeAreaMath.Anchors(new Rect(0f, 0f, 1080f, 1920f), new Vector2(1080f, 1920f), out Vector2 min, out Vector2 max);

            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }

        [Test]
        public void Anchors_NotchAndHomeIndicator_InsetTopAndBottom()
        {
            // iPhone-like portrait screen: 132 px notch at the top, 102 px home indicator at the bottom.
            SafeAreaMath.Anchors(new Rect(0f, 102f, 1170f, 2298f), new Vector2(1170f, 2532f), out Vector2 min, out Vector2 max);

            Assert.AreEqual(0f, min.x, 1e-6f);
            Assert.AreEqual(102f / 2532f, min.y, 1e-6f);
            Assert.AreEqual(1f, max.x, 1e-6f);
            Assert.AreEqual(2400f / 2532f, max.y, 1e-6f);
        }

        [Test]
        public void Anchors_AreaBeyondScreen_IsClamped()
        {
            SafeAreaMath.Anchors(new Rect(-10f, -10f, 2000f, 3000f), new Vector2(1080f, 1920f), out Vector2 min, out Vector2 max);

            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }

        [Test]
        public void Anchors_NoScreenSize_SpanWholeScreen()
        {
            SafeAreaMath.Anchors(new Rect(0f, 0f, 100f, 100f), Vector2.zero, out Vector2 min, out Vector2 max);

            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }
    }
}
