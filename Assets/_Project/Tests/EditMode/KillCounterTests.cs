using NUnit.Framework;
using RushOps.Levels;

namespace RushOps.Tests.EditMode
{
    public class KillCounterTests
    {
        [Test]
        public void New_StartsAtZero()
        {
            Assert.AreEqual(0, new KillCounter().Count);
        }

        [Test]
        public void Register_CountsEveryKillAndRaisesChanged()
        {
            var counter = new KillCounter();
            int reported = -1;
            counter.Changed += count => reported = count;

            counter.Register();
            counter.Register();
            counter.Register();

            Assert.AreEqual(3, counter.Count);
            Assert.AreEqual(3, reported);
        }
    }
}
