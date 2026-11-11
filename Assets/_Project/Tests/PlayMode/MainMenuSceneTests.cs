using System.Collections;
using NUnit.Framework;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    public class MainMenuSceneTests
    {
        private MainMenu _menu;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;
            _menu = Object.FindFirstObjectByType<MainMenu>();
            Assert.IsNotNull(_menu, "MainMenu scene has no MainMenu");
        }

        private static Button FindButton(string name)
        {
            GameObject go = GameObject.Find(name);
            Assert.IsNotNull(go, $"no {name}");
            return go.GetComponent<Button>();
        }

        [Test]
        public void EndlessButton_SitsUnderStoryMode_AndIsAvailableFromTheStart()
        {
            Button story = FindButton("StoryButton");
            Button endless = FindButton("EndlessButton");
            Button skins = FindButton("SkinsButton");

            Assert.IsTrue(endless.interactable, "endless mode needs no campaign progress (E7)");
            Assert.AreEqual("Endlos", endless.GetComponentInChildren<Text>().text);
            Assert.Greater(story.transform.position.y, endless.transform.position.y);
            Assert.Greater(endless.transform.position.y, skins.transform.position.y);
        }

        [UnityTest]
        public IEnumerator Endless_OpensTheArena_WithTheRunAtRoundOne()
        {
            RushOps.Endless.EndlessStart.Round = 7;
            RushOps.Endless.EndlessStart.Daily = true;
            // „Endlos“ opens its area (issue #280); „Starten“ there starts a normal run.
            FindButton("EndlessButton").onClick.Invoke();
            Assert.IsTrue(Object.FindAnyObjectByType<EndlessMenu>().IsOpen);
            FindButton("StartButton").onClick.Invoke();
            yield return null;
            yield return null;

            Assert.AreEqual("Endless_Arena", SceneManager.GetActiveScene().name);
            var controller = Object.FindAnyObjectByType<RushOps.Endless.EndlessRunController>();
            Assert.IsNotNull(controller, "no endless run in the arena");
            Assert.AreEqual(1, controller.Run.Round, "the menu always starts at round 1");
            Assert.IsFalse(controller.IsDaily, "„Starten“ is a normal run");
        }

        [Test]
        public void Menu_HasStoryModeSkinsAndSettings_InThisOrder()
        {
            Button story = FindButton("StoryButton");
            Button skins = FindButton("SkinsButton");
            Button settings = FindButton("SettingsButton");

            Assert.IsTrue(story.interactable);
            Assert.AreEqual("Story Mode", story.GetComponentInChildren<Text>().text);
            Assert.AreEqual("Spielerauswahl", skins.GetComponentInChildren<Text>().text);
            Assert.AreEqual("Einstellungen", settings.GetComponentInChildren<Text>().text);
            float storyY = story.transform.position.y;
            Assert.Greater(storyY, skins.transform.position.y);
            Assert.Greater(skins.transform.position.y, settings.transform.position.y);
        }

        [Test]
        public void Menu_ShowsLogoPlayerVersionAndTestButtonInTheEditor()
        {
            Assert.IsNotNull(GameObject.Find("Logo").GetComponent<Image>().sprite);
            var figure = Object.FindFirstObjectByType<MenuFigure>();
            Assert.IsNotNull(figure, "no player figure");
            Assert.IsNotNull(figure.GetComponent<Image>().sprite);
            Assert.AreEqual("v" + Application.version, GameObject.Find("Version").GetComponent<Text>().text);
            Assert.IsNotNull(GameObject.Find("TestMenuButton"), "test menu button hidden in a development build");
            Assert.IsNotNull(Object.FindFirstObjectByType<SafeAreaFitter>(), "menu ignores the safe area");
        }

        [UnityTest]
        public IEnumerator StoryMode_OpensTheIslandMap()
        {
            FindButton("StoryButton").onClick.Invoke();
            yield return null;
            yield return null;

            Assert.AreEqual("IslandMap", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator TestMenuButton_OpensTheTestMenu()
        {
            FindButton("TestMenuButton").onClick.Invoke();
            yield return null;
            yield return null;

            Assert.AreEqual("TestMenu", SceneManager.GetActiveScene().name);
        }
    }
}
