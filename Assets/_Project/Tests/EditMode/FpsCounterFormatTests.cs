using NUnit.Framework;
using RushOps.UI;

namespace RushOps.Tests.EditMode
{
    public class FpsCounterFormatTests
    {
        [Test]
        public void Format_ShowsFpsAndBothFrameTimes_WithDecimalComma()
        {
            Assert.AreEqual("FPS 60\n16,7 / 21,3 ms", FpsCounter.Format(60, 16.66f, 21.3f));
        }

        [Test]
        public void Format_ClampsOutOfRangeValues()
        {
            Assert.AreEqual("FPS 240\n0,0 / 999,9 ms", FpsCounter.Format(1000, -1f, 5000f));
        }

        [Test]
        public void Format_WithMemory_AddsALine()
        {
            Assert.AreEqual("FPS 59\n16,9 / 18,0 ms\nRAM 182 MB", FpsCounter.Format(59, 16.9f, 18f, 182));
        }

        [Test]
        public void Format_ReusesTheSameString()
        {
            Assert.AreSame(FpsCounter.Format(58, 17.2f, 30f), FpsCounter.Format(58, 17.2f, 30f));
        }
    }
}
