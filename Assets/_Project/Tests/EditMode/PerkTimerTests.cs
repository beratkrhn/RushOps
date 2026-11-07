using NUnit.Framework;
using RushOps.Perks;

namespace RushOps.Tests.EditMode
{
    public class PerkTimerTests
    {
        [Test]
        public void Tick_WholeStepsEndOnTheLastStep()
        {
            var timer = new PerkTimer();
            timer.Restart(1f);
            bool expired = false;
            for (int i = 0; i < 10; i++)
            {
                expired = timer.Tick(0.1f);
            }

            Assert.IsTrue(expired);
            Assert.IsFalse(timer.IsRunning);
        }

        [Test]
        public void Restart_ZeroOrNegative_StartsNothing()
        {
            var timer = new PerkTimer();
            timer.Restart(0f);
            Assert.IsFalse(timer.IsRunning);
        }
    }
}
