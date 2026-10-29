using NUnit.Framework;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class VirtualStickMathTests
    {
        private const float Radius = 48f;
        private static readonly Vector2 Zone = new Vector2(180f, 640f);

        [Test]
        public void HomePosition_Left_SitsInBottomLeftCorner()
        {
            Vector2 home = VirtualStickMath.HomePosition(Zone, StickSide.Left, Radius, 24f);

            Assert.AreEqual(new Vector2(72f, 72f), home);
        }

        [Test]
        public void HomePosition_Right_SitsInBottomRightCorner()
        {
            Vector2 home = VirtualStickMath.HomePosition(Zone, StickSide.Right, Radius, 24f);

            Assert.AreEqual(new Vector2(108f, 72f), home);
        }

        [Test]
        public void HomePosition_MarginTooLargeForZone_StaysInsideZone()
        {
            Vector2 home = VirtualStickMath.HomePosition(new Vector2(120f, 640f), StickSide.Left, Radius, 100f);

            Assert.AreEqual(72f, home.x);
            Assert.AreEqual(148f, home.y);
        }

        [Test]
        public void ClampToZone_PointAtEdge_KeepsWholeStickInside()
        {
            Vector2 clamped = VirtualStickMath.ClampToZone(new Vector2(-10f, 700f), Zone, Radius);

            Assert.AreEqual(new Vector2(Radius, Zone.y - Radius), clamped);
        }

        [Test]
        public void ClampToZone_ZoneSmallerThanStick_Centers()
        {
            Vector2 clamped = VirtualStickMath.ClampToZone(new Vector2(5f, 5f), new Vector2(60f, 80f), Radius);

            Assert.AreEqual(new Vector2(30f, 40f), clamped);
        }

        [Test]
        public void AcceptsPress_Floating_AnywhereInZone()
        {
            Assert.IsTrue(VirtualStickMath.AcceptsPress(StickMode.Floating, new Vector2(72f, 72f), new Vector2(150f, 600f), Radius));
        }

        [Test]
        public void AcceptsPress_Fixed_OnlyWithinGrabRadius()
        {
            var home = new Vector2(72f, 72f);

            Assert.IsTrue(VirtualStickMath.AcceptsPress(StickMode.Fixed, home, home + new Vector2(Radius, 0f), Radius));
            Assert.IsFalse(VirtualStickMath.AcceptsPress(StickMode.Fixed, home, home + new Vector2(Radius + 1f, 0f), Radius));
        }

        [Test]
        public void Origin_Fixed_IsHome()
        {
            var home = new Vector2(72f, 72f);

            Assert.AreEqual(home, VirtualStickMath.Origin(StickMode.Fixed, home, new Vector2(90f, 80f), Zone, Radius));
        }

        [Test]
        public void Origin_Floating_IsClampedTouchPoint()
        {
            var home = new Vector2(72f, 72f);

            Assert.AreEqual(new Vector2(100f, 300f), VirtualStickMath.Origin(StickMode.Floating, home, new Vector2(100f, 300f), Zone, Radius));
            Assert.AreEqual(new Vector2(Radius, Radius), VirtualStickMath.Origin(StickMode.Floating, home, new Vector2(2f, 3f), Zone, Radius));
        }

        [Test]
        public void Value_WithinRadius_IsProportional()
        {
            Vector2 value = VirtualStickMath.Value(new Vector2(100f, 100f), new Vector2(124f, 100f), Radius);

            Assert.AreEqual(0.5f, value.x, 1e-5f);
            Assert.AreEqual(0f, value.y, 1e-5f);
        }

        [Test]
        public void Value_BeyondRadius_IsClampedToLengthOne()
        {
            Vector2 value = VirtualStickMath.Value(Vector2.zero, new Vector2(300f, 400f), Radius);

            Assert.AreEqual(1f, value.magnitude, 1e-5f);
            Assert.AreEqual(0.6f, value.x, 1e-5f);
            Assert.AreEqual(0.8f, value.y, 1e-5f);
        }

        [Test]
        public void Value_ZeroRadius_IsZero()
        {
            Assert.AreEqual(Vector2.zero, VirtualStickMath.Value(Vector2.zero, Vector2.one, 0f));
        }

        [Test]
        public void KnobPosition_FullDeflection_SitsOnRadius()
        {
            Assert.AreEqual(new Vector2(10f, 10f + Radius), VirtualStickMath.KnobPosition(new Vector2(10f, 10f), Vector2.up, Radius));
        }

        [Test]
        public void DashButtonPosition_SitsAboveStickBase()
        {
            Vector2 position = VirtualStickMath.DashButtonPosition(new Vector2(108f, 72f), Radius, 72f, 16f);

            // Base top at 120, plus gap 16, plus half the button 36.
            Assert.AreEqual(new Vector2(108f, 172f), position);
        }
    }
}
