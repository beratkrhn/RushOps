using System.Collections;
using NUnit.Framework;
using RushOps.Audio;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The music across scene changes (issue #299, F-AUD-01).</summary>
    public class MusicPlayerTests
    {
        [UnitySetUp]
        public IEnumerator ResetMusic()
        {
            SaveGameIsolation.Reset();
            yield return DestroyMusic();
        }

        [UnityTearDown]
        public IEnumerator CleanUp()
        {
            Time.timeScale = 1f;
            yield return DestroyMusic();
        }

        [UnityTest]
        public IEnumerator MenuToLevel_SwitchesTrack_LevelToLevel_KeepsPlaying()
        {
            yield return Load("MainMenu");
            MusicPlayer music = MusicPlayer.Current;
            Assert.IsNotNull(music, "the main menu started no music");
            Assert.AreEqual(MusicTrack.Menu, music.Track);
            Assert.AreEqual("Music_Menu", music.PlayingClip.name);

            yield return Load("Level_1-1");
            Assert.AreSame(music, MusicPlayer.Current, "the music player survives the scene change");
            Assert.AreEqual(MusicTrack.Game, music.Track);
            Assert.AreEqual("Music_Game", music.PlayingClip.name);
            yield return new WaitForSecondsRealtime(0.5f);
            float before = music.PlayingTime;
            Assert.Greater(before, 0f, "the game track does not play");

            yield return Load("Level_1-2");
            Assert.AreEqual(MusicTrack.Game, music.Track);
            Assert.GreaterOrEqual(music.PlayingTime, before, "the game track started over on the next level");
        }

        [UnityTest]
        public IEnumerator Pause_DucksTheMusic_ResumeBringsItBack()
        {
            yield return Load("Level_1-1");
            MusicPlayer music = MusicPlayer.Current;
            PauseController pause = Object.FindFirstObjectByType<PauseController>();
            Assert.IsFalse(music.IsDucked);

            pause.Pause();
            yield return null;
            yield return null;
            Assert.IsTrue(music.IsDucked, "the pause overlay should duck the music");

            pause.Resume();
            yield return null;
            yield return null;
            Assert.IsFalse(music.IsDucked);
        }

        [UnityTest]
        public IEnumerator Music_PlaysThroughTheMusicGroup()
        {
            yield return Load("MainMenu");
            foreach (AudioSource voice in MusicPlayer.Current.GetComponents<AudioSource>())
            {
                Assert.IsNotNull(voice.outputAudioMixerGroup, "music bypasses the mixer");
                Assert.AreEqual("Musik", voice.outputAudioMixerGroup.name, "the music slider would not work");
            }
        }

        private static IEnumerator Load(string scene)
        {
            yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield return null;
            yield return null;
        }

        private static IEnumerator DestroyMusic()
        {
            if (MusicPlayer.Current != null)
            {
                Object.Destroy(MusicPlayer.Current.gameObject);
                yield return null;
            }
        }
    }
}
