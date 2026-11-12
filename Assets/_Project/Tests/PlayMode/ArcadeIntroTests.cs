using System.Collections;
using NUnit.Framework;
using RushOps.Arcade;
using RushOps.Endless;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    /// <summary>The walk from the menu to the cabinet (issue #324, F-ARC-07).</summary>
    public class ArcadeIntroTests
    {
        private ArcadeIntro _intro;

        [UnitySetUp]
        public IEnumerator LoadMenu()
        {
            SaveGameIsolation.Reset();
            SaveGameIsolation.Service.Data.Arcade.Selected = true;
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            yield return null;
            _intro = Object.FindAnyObjectByType<ArcadeIntro>();
            Assert.IsNotNull(_intro, "the main menu has no arcade transition");
        }

        [TearDown]
        public void TearDown()
        {
            EndlessStart.Arcade = false;
            ArcadeTransition.FadeInMenu = false;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Start_PlaysZoomWalkAndDive_ThenStartsTheArcadeRun()
        {
            ArcadeConfig config = _intro.Config;
            Object.FindAnyObjectByType<RushOps.UI.MainMenu>().StartEndless();
            Assert.IsTrue(_intro.IsPlaying);
            Assert.AreEqual(3f, _intro.Duration, 0.6f, "about 3 s");
            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name);

            var figure = GameObject.Find("Figure").GetComponent<RectTransform>();
            var menu = (RectTransform)Object.FindAnyObjectByType<RushOps.UI.SafeAreaFitter>().transform;
            Vector2 start = figure.anchoredPosition;
            yield return new WaitForSecondsRealtime(config.IntroZoomOut + config.IntroWalk * 0.5f);
            Assert.AreEqual(ArcadeIntroPhase.Walk, _intro.Phase);
            Assert.AreEqual(config.IntroMenuScale, menu.localScale.x, 0.01f, "the menu shrank away");
            Assert.Greater(figure.anchoredPosition.y, start.y, "the figure walks up to the cabinet");

            float waited = 0f;
            while (SceneManager.GetActiveScene().name != "Endless_Arena" && waited < 6f)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            Assert.AreEqual("Endless_Arena", SceneManager.GetActiveScene().name);
            yield return null;
            Assert.IsTrue(Object.FindAnyObjectByType<ArcadeCabinet>().IsActive);
        }

        [UnityTest]
        public IEnumerator Tap_SkipsTheTransition()
        {
            Object.FindAnyObjectByType<RushOps.UI.MainMenu>().StartDaily();
            yield return null;
            GameObject.Find("Skip").GetComponent<Button>().onClick.Invoke();
            yield return null;
            yield return null;

            Assert.AreEqual("Endless_Arena", SceneManager.GetActiveScene().name);
            Assert.IsTrue(EndlessStart.Daily);
            Assert.IsTrue(Object.FindAnyObjectByType<ArcadeCabinet>().IsActive);
        }

        [UnityTest]
        public IEnumerator NormalDisplay_StartsWithoutTheTransition()
        {
            SaveGameIsolation.Service.Data.Arcade.Selected = false;
            Object.FindAnyObjectByType<RushOps.UI.MainMenu>().StartEndless();
            Assert.IsFalse(_intro.IsPlaying);
            yield return null;
            yield return null;
            Assert.AreEqual("Endless_Arena", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator BackFromAnArcadeRun_TheMenuFadesInBriefly()
        {
            yield return ArcadeRunTests.LoadWithoutReset(true);
            SceneManager.LoadScene("MainMenu");
            yield return null;
            var black = GameObject.Find("Black").GetComponent<Image>();
            Assert.IsTrue(black.enabled, "starts black");
            Assert.Greater(black.color.a, 0.5f);

            yield return new WaitForSecondsRealtime(Object.FindAnyObjectByType<ArcadeIntro>().Config.MenuFadeIn + 0.2f);
            Assert.IsFalse(black.enabled, "faded in");
            Assert.IsFalse(ArcadeTransition.FadeInMenu, "only once");
        }
    }
}
