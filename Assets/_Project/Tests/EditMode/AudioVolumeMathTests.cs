using NUnit.Framework;
using RushOps.Audio;

namespace RushOps.Tests.EditMode
{
    public class AudioVolumeMathTests
    {
        [TestCase(1f, 0f)]
        [TestCase(0.5f, -6.0206f)]
        [TestCase(0.1f, -20f)]
        [TestCase(2f, 0f)]
        public void ToDecibels_IsLogarithmic(float volume, float expected)
        {
            Assert.AreEqual(expected, AudioVolumeMath.ToDecibels(volume), 1e-3f);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(0.00001f)]
        public void ToDecibels_ZeroMutes(float volume)
        {
            Assert.AreEqual(AudioVolumeMath.MutedDecibels, AudioVolumeMath.ToDecibels(volume));
        }

        [Test]
        public void ParameterNames_AreDistinct()
        {
            Assert.AreEqual("MasterVolume", AudioVolumeMath.ParameterName(AudioChannel.Master));
            Assert.AreEqual("MusicVolume", AudioVolumeMath.ParameterName(AudioChannel.Music));
            Assert.AreEqual("EffectsVolume", AudioVolumeMath.ParameterName(AudioChannel.Effects));
        }
    }
}
