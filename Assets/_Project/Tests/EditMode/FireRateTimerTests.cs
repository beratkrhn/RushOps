using NUnit.Framework;
using RushOps.Combat;

namespace RushOps.Tests.EditMode
{
    public class FireRateTimerTests
    {
        private const float Rate = 5f;
        private const float Frame = 1f / 60f;

        [Test]
        public void Tick_FirstPress_FiresImmediately()
        {
            var timer = new FireRateTimer();

            Assert.AreEqual(1, timer.Tick(Frame, true, Rate));
        }

        [Test]
        public void Tick_Released_NeverFires()
        {
            var timer = new FireRateTimer();

            Assert.AreEqual(0, Simulate(timer, 2f, Frame, false));
        }

        [Test]
        public void Tick_HeldOneSecond_FiresAtConfiguredRate()
        {
            var timer = new FireRateTimer();

            // First shot at t=0, then every 0.2 s: 0, 0.2, 0.4, 0.6, 0.8 (and 1.0 lands on the boundary).
            int shots = Simulate(timer, 1f - 1e-3f, Frame, true);

            Assert.AreEqual(5, shots);
        }

        [Test]
        public void Tick_LowFrameRate_FiresSameNumberOfShots()
        {
            var fast = new FireRateTimer();
            var slow = new FireRateTimer();

            int fastShots = Simulate(fast, 3f, 1f / 120f, true);
            int slowShots = Simulate(slow, 3f, 1f / 10f, true);

            Assert.AreEqual(fastShots, slowShots, 1);
        }

        [Test]
        public void Tick_LongFrame_FiresSeveralShotsAtOnce()
        {
            var timer = new FireRateTimer();
            timer.Tick(Frame, true, Rate);

            // 0.5 s at 5 shots/s covers 2.5 intervals.
            Assert.AreEqual(2, timer.Tick(0.5f, true, Rate));
        }

        [Test]
        public void Tick_TappingFast_NotFasterThanHolding()
        {
            var timer = new FireRateTimer();
            int shots = 0;
            for (int i = 0; i < 60; i++)
            {
                // Alternate pressed and released every frame for one second.
                shots += timer.Tick(Frame, i % 2 == 0, Rate);
            }

            Assert.LessOrEqual(shots, 5);
        }

        [Test]
        public void Tick_AfterLongPause_DoesNotBankShots()
        {
            var timer = new FireRateTimer();
            timer.Tick(Frame, true, Rate);
            Simulate(timer, 5f, Frame, false);

            Assert.AreEqual(1, timer.Tick(Frame, true, Rate));
        }

        [Test]
        public void Tick_ZeroRate_NeverFires()
        {
            var timer = new FireRateTimer();

            Assert.AreEqual(0, timer.Tick(Frame, true, 0f));
        }

        [Test]
        public void Reset_DuringCooldown_FiresImmediatelyAgain()
        {
            var timer = new FireRateTimer();
            timer.Tick(Frame, true, Rate);

            timer.Reset();

            Assert.AreEqual(1, timer.Tick(Frame, true, Rate));
        }

        private static int Simulate(FireRateTimer timer, float duration, float step, bool held)
        {
            int shots = 0;
            for (float t = 0f; t < duration; t += step)
            {
                shots += timer.Tick(step, held, Rate);
            }

            return shots;
        }
    }
}
