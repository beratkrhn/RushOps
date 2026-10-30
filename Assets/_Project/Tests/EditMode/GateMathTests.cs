using NUnit.Framework;
using RushOps.Levels;

namespace RushOps.Tests.EditMode
{
    public class GateMathTests
    {
        [Test]
        public void Sink_DuringShake_IsZero()
        {
            Assert.AreEqual(0f, GateMath.Sink(0.1f, 1f, 0.2f));
        }

        [Test]
        public void Sink_RisesToOneAtEnd()
        {
            float middle = GateMath.Sink(0.6f, 1f, 0.2f);

            Assert.Greater(middle, 0f);
            Assert.Less(middle, 1f);
            Assert.AreEqual(1f, GateMath.Sink(1f, 1f, 0.2f));
        }

        [Test]
        public void Sink_ShakeLongerThanOpening_JumpsAtEnd()
        {
            Assert.AreEqual(0f, GateMath.Sink(0.5f, 0.1f, 0.2f));
            Assert.AreEqual(1f, GateMath.Sink(1f, 0.1f, 0.2f));
        }

        [Test]
        public void Shake_DuringShake_HasAmplitude()
        {
            float shake = GateMath.Shake(0.05f, 1f, 0.2f, 0.04f);

            Assert.AreEqual(0.04f, System.Math.Abs(shake), 1e-6f);
        }

        [Test]
        public void Shake_AfterShakeOrBeforeStart_IsZero()
        {
            Assert.AreEqual(0f, GateMath.Shake(0f, 1f, 0.2f, 0.04f));
            Assert.AreEqual(0f, GateMath.Shake(0.5f, 1f, 0.2f, 0.04f));
        }
    }
}
