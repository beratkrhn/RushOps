using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;

namespace RushOps.Tests.EditMode
{
    public class ExitLockTests
    {
        [Test]
        public void ZoneWithoutWaves_IsOpen()
        {
            Assert.IsTrue(ExitLock.IsOpen(false, WavePhase.Idle, 3));
        }

        [TestCase(WavePhase.Idle)]
        [TestCase(WavePhase.Spawning)]
        [TestCase(WavePhase.Clearing)]
        [TestCase(WavePhase.Pause)]
        public void WavesRunning_IsLocked(WavePhase phase)
        {
            Assert.IsFalse(ExitLock.IsOpen(true, phase, 0));
        }

        [Test]
        public void WavesFinished_EnemiesLeft_IsLocked()
        {
            Assert.IsFalse(ExitLock.IsOpen(true, WavePhase.Finished, 1));
        }

        [Test]
        public void WavesFinished_NoEnemies_IsOpen()
        {
            Assert.IsTrue(ExitLock.IsOpen(true, WavePhase.Finished, 0));
        }
    }
}
