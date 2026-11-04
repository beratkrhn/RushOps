using System.Collections;
using NUnit.Framework;
using RushOps.App;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    public class BootSceneTests
    {
        private const string SceneName = "Boot";

        private IntroSequence _intro;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            _intro = Object.FindFirstObjectByType<IntroSequence>();
            Assert.IsNotNull(_intro, "Boot has no IntroSequence");
        }

        [Test]
        public void Start_ShowsTheLogo_AndLoadsTheSaveGame()
        {
            Image logo = GameObject.Find("Logo").GetComponent<Image>();

            Assert.IsNotNull(logo.sprite, "no logo frame");
            Assert.IsFalse(_intro.IsDone);
            Assert.IsTrue(SaveGameIsolation.Service.IsLoaded, "save game not loaded at start");
        }

        [UnityTest]
        public IEnumerator WithoutTap_EndsInTheNextSceneWithinThreeSeconds()
        {
            float start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name == SceneName && Time.realtimeSinceStartup - start < 5f)
            {
                yield return null;
            }

            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name);
            Assert.LessOrEqual(Time.realtimeSinceStartup - start, 3.5f, "logo took longer than 3 s");
        }

        [UnityTest]
        public IEnumerator Tap_SkipsToTheFade()
        {
            yield return null;
            _intro.Skip();
            float start = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name == SceneName && Time.realtimeSinceStartup - start < 3f)
            {
                yield return null;
            }

            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name);
            Assert.Less(Time.realtimeSinceStartup - start, 1f, "skipping did not shorten the logo");
        }
    }
}
