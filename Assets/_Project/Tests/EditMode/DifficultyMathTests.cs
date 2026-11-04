using NUnit.Framework;
using RushOps.Levels;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class DifficultyMathTests
    {
        private LevelDifficulty _difficulty;

        [SetUp]
        public void Create()
        {
            _difficulty = ScriptableObject.CreateInstance<LevelDifficulty>();
        }

        [TearDown]
        public void Destroy()
        {
            Object.DestroyImmediate(_difficulty);
        }

        [Test]
        public void WithoutDifficulty_TimesStayAsSet()
        {
            Assert.AreEqual(2f, DifficultyMath.WaveTime(2f, (LevelDifficulty)null));
            Assert.AreEqual(12f, DifficultyMath.SwitchTimeLimit(12f, null));
            Assert.AreEqual(10f, DifficultyMath.HoldDuration(10f, null));
        }

        [Test]
        public void Island2_WavesDenser_HoldLonger()
        {
            // Decision C3: waves 20 % denser, hold zone +3 s.
            _difficulty.Configure(1.2f, 1f, 3f);

            Assert.AreEqual(2.5f, DifficultyMath.WaveTime(3f, _difficulty), 1e-5f);
            Assert.AreEqual(12f, DifficultyMath.SwitchTimeLimit(12f, _difficulty), 1e-5f);
            Assert.AreEqual(13f, DifficultyMath.HoldDuration(10f, _difficulty), 1e-5f);
        }

        [Test]
        public void Island3_SwitchTimeMinus20Percent()
        {
            _difficulty.Configure(1.2f, 0.8f, 3f);

            Assert.AreEqual(9.6f, DifficultyMath.SwitchTimeLimit(12f, _difficulty), 1e-5f);
        }

        [Test]
        public void Configure_ClampsNonsense()
        {
            _difficulty.Configure(0f, -1f, -5f);

            Assert.AreEqual(0.1f, _difficulty.WaveDensity, 1e-5f);
            Assert.AreEqual(0.1f, _difficulty.SwitchTimeFactor, 1e-5f);
            Assert.AreEqual(0f, _difficulty.ExtraHoldSeconds);
            Assert.AreEqual(10f, DifficultyMath.WaveTime(1f, 0f), 1e-5f, "density below 0.1 counts as 0.1");
        }
    }
}
