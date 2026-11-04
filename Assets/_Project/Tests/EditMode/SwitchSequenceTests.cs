using NUnit.Framework;
using RushOps.Puzzles;

namespace RushOps.Tests.EditMode
{
    public class SwitchSequenceTests
    {
        private SwitchSequence _sequence;

        [SetUp]
        public void CreateSequence()
        {
            _sequence = new SwitchSequence(3, 10f);
        }

        [Test]
        public void Start_NothingPressed_TimerStopped()
        {
            Assert.AreEqual(0, _sequence.PressedCount);
            Assert.IsFalse(_sequence.IsRunning);
            Assert.IsFalse(_sequence.Tick(100f), "timer must not run before the first press");
        }

        [Test]
        public void FirstPress_StartsTimer()
        {
            Assert.IsTrue(_sequence.Press(1));

            Assert.IsTrue(_sequence.IsRunning);
            Assert.AreEqual(10f, _sequence.Remaining, 1e-5f);
            Assert.IsTrue(_sequence.IsPressed(1));
        }

        [Test]
        public void LaterPresses_DoNotRestartTimer()
        {
            _sequence.Press(0);
            _sequence.Tick(4f);

            _sequence.Press(2);

            Assert.AreEqual(6f, _sequence.Remaining, 1e-4f);
        }

        [Test]
        public void SameSwitchTwice_CountsOnce()
        {
            _sequence.Press(0);

            Assert.IsFalse(_sequence.Press(0));
            Assert.AreEqual(1, _sequence.PressedCount);
        }

        [Test]
        public void AllPressedInTime_Completes()
        {
            int completed = 0;
            _sequence.Completed += () => completed++;
            _sequence.Press(0);
            _sequence.Tick(3f);
            _sequence.Press(1);
            _sequence.Tick(6.9f);

            _sequence.Press(2);

            Assert.IsTrue(_sequence.IsComplete);
            Assert.IsFalse(_sequence.IsRunning);
            Assert.AreEqual(1, completed);
            Assert.IsFalse(_sequence.Tick(100f), "a completed sequence never runs out");
        }

        [Test]
        public void TimerRunsOut_ResetsAllSwitches()
        {
            int expired = 0;
            _sequence.Expired += () => expired++;
            _sequence.Press(0);
            _sequence.Press(1);

            Assert.IsTrue(_sequence.Tick(10f));

            Assert.AreEqual(1, expired);
            Assert.AreEqual(0, _sequence.PressedCount);
            Assert.IsFalse(_sequence.IsPressed(0));
            Assert.IsFalse(_sequence.IsPressed(1));
            Assert.IsFalse(_sequence.IsRunning);
        }

        [Test]
        public void AfterReset_NextPressStartsAFreshTimer()
        {
            _sequence.Press(0);
            _sequence.Tick(10f);

            _sequence.Press(2);

            Assert.IsTrue(_sequence.IsRunning);
            Assert.AreEqual(10f, _sequence.Remaining, 1e-5f);
            Assert.AreEqual(1, _sequence.PressedCount);
        }

        [Test]
        public void TimeStepsSummingToLimit_RunOut()
        {
            _sequence.Press(0);
            bool expired = false;
            for (int i = 0; i < 600; i++)
            {
                expired |= _sequence.Tick(1f / 60f);
            }

            Assert.IsTrue(expired);
        }

        [TestCase(-1)]
        [TestCase(3)]
        public void Press_InvalidIndex_Ignored(int index)
        {
            Assert.IsFalse(_sequence.Press(index));
            Assert.IsFalse(_sequence.IsRunning);
        }
    }
}
