using System.Collections;
using NUnit.Framework;
using RushOps.Story;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The intro scene plays the intro and opens the island map at the end (issue #332).</summary>
    public class IntroSceneTests
    {
        [UnityTest]
        public IEnumerator Intro_PlaysTheFirstPanel()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(IntroScene.SceneName, LoadSceneMode.Single);
            yield return null;

            var player = Object.FindAnyObjectByType<CutscenePlayer>();
            Assert.IsNotNull(player);
            Assert.IsTrue(player.IsPlaying);
            Assert.AreEqual(7, player.Playback.PanelCount);
            Assert.AreEqual(0, player.Playback.PanelIndex);
        }

        [UnityTest]
        public IEnumerator Skip_OpensTheIslandMap()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(IntroScene.SceneName, LoadSceneMode.Single);
            yield return null;

            Object.FindAnyObjectByType<CutscenePlayer>().Skip();
            float timeout = Time.realtimeSinceStartup + 5f;
            while (SceneManager.GetActiveScene().name != IslandMap.SceneName && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.AreEqual(IslandMap.SceneName, SceneManager.GetActiveScene().name);
        }
    }
}
