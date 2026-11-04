using System.Collections;
using NUnit.Framework;
using RushOps.Audio;
using RushOps.Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Sound effects and mixer in a level (issue #108).</summary>
    public class AudioTests
    {
        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
        }

        [Test]
        public void Effects_PlayThroughTheEffectsGroupOnSeveralVoices()
        {
            var sfx = Object.FindFirstObjectByType<SfxPlayer>();
            Assert.IsNotNull(sfx.Output, "effects are not routed to the mixer");
            Assert.AreEqual("Effekte", sfx.Output.name);
            AudioSource[] voices = sfx.GetComponents<AudioSource>();
            Assert.Greater(voices.Length, 1, "one voice re-pitches overlapping sounds");
            foreach (AudioSource voice in voices)
            {
                Assert.AreSame(sfx.Output, voice.outputAudioMixerGroup);
            }
        }

        [Test]
        public void Volumes_SetTheMixer()
        {
            var volumes = Object.FindFirstObjectByType<AudioVolumes>();
            var sfx = Object.FindFirstObjectByType<SfxPlayer>();
            volumes.SetVolume(AudioChannel.Effects, 0.5f);

            Assert.IsTrue(sfx.Output.audioMixer.GetFloat("EffectsVolume", out float decibels));
            Assert.AreEqual(AudioVolumeMath.ToDecibels(0.5f), decibels, 0.01f);
            Assert.AreEqual(0.5f, volumes.GetVolume(AudioChannel.Effects), 1e-5f);
        }

        [Test]
        public void Buttons_PlayTheClick()
        {
            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.IsNotEmpty(buttons);
            foreach (Button button in buttons)
            {
                bool clicks = false;
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                {
                    clicks |= button.onClick.GetPersistentTarget(i) is UiClickSound;
                }

                Assert.IsTrue(clicks, $"{button.name} has no click sound");
            }
        }

        [Test]
        public void EveryEnemyType_HasSpawnAndApproachSounds()
        {
            foreach (EnemyPool pool in Object.FindObjectsByType<EnemyPool>(FindObjectsSortMode.None))
            {
                Assert.IsNotNull(pool.Definition.SpawnClip, $"{pool.Definition.name} has no spawn sound");
                Assert.IsNotNull(pool.Definition.ApproachClip, $"{pool.Definition.name} has no approach sound");
            }
        }
    }
}
