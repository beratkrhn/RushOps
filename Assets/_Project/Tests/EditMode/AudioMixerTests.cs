using NUnit.Framework;
using RushOps.Audio;
using UnityEditor;
using UnityEngine.Audio;

namespace RushOps.Tests.EditMode
{
    /// <summary>The audio mixer (issue #108, F-AUD-04).</summary>
    public class AudioMixerTests
    {
        private const string MixerPath = "Assets/_Project/Audio/RushOps.mixer";

        [Test]
        public void Mixer_HasMusicAndEffectsGroupsUnderMaster()
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            Assert.IsNotNull(mixer, "no mixer at " + MixerPath);
            Assert.IsNotEmpty(mixer.FindMatchingGroups("Master/Musik"));
            Assert.IsNotEmpty(mixer.FindMatchingGroups("Master/Effekte"));
        }

        [TestCase(AudioChannel.Master)]
        [TestCase(AudioChannel.Music)]
        [TestCase(AudioChannel.Effects)]
        public void Mixer_ExposesVolume(AudioChannel channel)
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            Assert.IsTrue(mixer.GetFloat(AudioVolumeMath.ParameterName(channel), out _), $"{channel} volume not exposed");
        }
    }
}
