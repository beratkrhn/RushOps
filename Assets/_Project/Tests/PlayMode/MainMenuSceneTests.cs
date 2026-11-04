using System.Collections;
using NUnit.Framework;
using RushOps.Save;
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
        public IEnumerator StoryMode_StartsLevel1_1OnANewGame()
        {
            Assert.AreEqual("Level_1-1", _menu.StoryScene);

            FindButton("StoryButton").onClick.Invoke();
            yield return null;
            yield return null;

            Assert.AreEqual("Level_1-1", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator TestMenuButton_OpensTheTestMenu()
        {
            FindButton("TestMenuButton").onClick.Invoke();
            yield return null;
            yield return null;

            Assert.AreEqual("TestMenu", SceneManager.GetActiveScene().name);
        }

        [Test]
        public void StoryMode_AfterLevel1_1_PlaysItAgainUntilMoreLevelsExist()
        {
            ProgressRules.RecordCompletion(SaveGameIsolation.Service.Data.Progress, "Level_1-1", 3, 5000);

            Assert.AreEqual("Level_1-1", _menu.StoryScene);
        }
    }
}
