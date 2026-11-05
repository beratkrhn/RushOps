using NUnit.Framework;
using RushOps.Bosses;

namespace RushOps.Tests.EditMode
{
    public class PuddleTimerTests
    {
        [Test]
        public void Puddle_GlowsFirst_ThenHurts_ThenIsGone()
        {
            // Decision C9b: 1 s glow, harmful until 6 s after it appeared.
            var timer = new PuddleTimer(1f, 6f);

            Assert.AreEqual(PuddleState.Warm, timer.State);
            Assert.AreEqual(PuddleState.Warm, timer.Tick(0.99f));
            Assert.AreEqual(PuddleState.Active, timer.Tick(0.02f));
            Assert.AreEqual(PuddleState.Active, timer.Tick(4.9f));
            Assert.AreEqual(PuddleState.Gone, timer.Tick(0.1f));
        }

        [Test]
        public void Restart_StartsAFreshPuddle()
        {
            var timer = new PuddleTimer(1f, 6f);
            timer.Tick(7f);

            timer.Restart();

            Assert.AreEqual(PuddleState.Warm, timer.State);
            Assert.AreEqual(0f, timer.Age);
        }

        [Test]
        public void Lifetime_IsNeverShorterThanTheWarmUp()
        {
            var timer = new PuddleTimer(2f, 1f);

            Assert.AreEqual(PuddleState.Warm, timer.Tick(1.5f));
            Assert.AreEqual(PuddleState.Gone, timer.Tick(0.5f));
        }
    }
}
