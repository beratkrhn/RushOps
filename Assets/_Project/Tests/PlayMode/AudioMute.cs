using NUnit.Framework;
using UnityEngine;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Silences all audio output while the PlayMode tests run, so a test run in the open Editor makes no noise.
    /// Only the global listener volume changes; mixer groups and volume settings stay as the scenes set them.
    /// </summary>
    [SetUpFixture]
    public class AudioMute
    {
        private float _previousVolume = 1f;

        [OneTimeSetUp]
        public void Mute()
        {
            _previousVolume = AudioListener.volume;
            AudioListener.volume = 0f;
        }

        [OneTimeTearDown]
        public void Restore()
        {
            AudioListener.volume = _previousVolume;
        }
    }
}
