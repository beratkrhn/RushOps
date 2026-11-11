using NUnit.Framework;
using RushOps.Puzzles;

namespace RushOps.Tests.EditMode
{
    /// <summary>When a zombie puzzle sends its next zombie (issue #289, F-RAE-10, F-RAE-12).</summary>
    public class ReinforcementTimerTests
    {
        [Test]
        public void FirstZombie_ComesAtOnce_ThenOneEveryInterval()
        {
            var timer = new ReinforcementTimer(12f, 2);

            Assert.IsTrue(timer.Tick(0f, 0));
            Assert.IsFalse(timer.Tick(11.9f, 1));
            Assert.IsTrue(timer.Tick(0.1f, 1));
        }

        [Test]
        public void Cap_HoldsBackTheNext_UntilOneDies()
        {
            var timer = new ReinforcementTimer(12f, 2);
            timer.Tick(0f, 0);
            timer.Tick(12f, 1);

            Assert.IsFalse(timer.Tick(30f, 2), "two alive: the cap holds the third back");
            Assert.IsTrue(timer.Tick(0f, 1), "one died: the held-back one comes at once");
            Assert.IsFalse(timer.Tick(0f, 1));
        }

        [Test]
        public void BadValues_FallBackToSafeOnes()
        {
            var timer = new ReinforcementTimer(-1f, 0);

            Assert.IsTrue(timer.Tick(0f, 0));
            Assert.IsFalse(timer.Tick(0f, 1), "at least one alive is the cap");
        }
    }
}
