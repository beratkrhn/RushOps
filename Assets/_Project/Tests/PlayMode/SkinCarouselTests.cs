using System.Collections;
using NUnit.Framework;
using RushOps.Save;
using RushOps.Skins;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    public class SkinCarouselTests
    {
        private SaveGameService _save;
        private SkinCarousel _carousel;

        private IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;
            _carousel = Object.FindFirstObjectByType<SkinCarousel>();
            Assert.IsNotNull(_carousel, "main menu has no SkinCarousel");
        }

        private static Text Label(string name)
        {
            GameObject go = GameObject.Find(name);
            Assert.IsNotNull(go, $"no {name}");
            return go.GetComponent<Text>();
        }

        private static Button Button(string name)
        {
            GameObject go = GameObject.Find(name);
            Assert.IsNotNull(go, $"no {name}");
            return go.GetComponent<Button>();
        }

        [UnityTest]
        public IEnumerator SkinsButton_OpensTheCarouselWithEverySkin()
        {
            _save = SaveGameIsolation.Reset();
            yield return Load();
            Button skins = Button("SkinsButton");
            Assert.IsTrue(skins.interactable);
            Assert.IsFalse(_carousel.IsOpen);

            skins.onClick.Invoke();
            yield return null;

            Assert.IsTrue(_carousel.IsOpen);
            Assert.AreEqual(0, _carousel.Index, "a new game starts at the standard skin");
            Assert.AreEqual(7, GameObject.Find("Strip").transform.childCount - 1, "one figure per skin besides the template");
            Assert.AreEqual("Standard", Label("Name").text);
            Assert.AreEqual(SkinHint.Chosen, Button("SelectButton").GetComponentInChildren<Text>().text);
            Assert.IsFalse(Button("SelectButton").interactable);
            Assert.IsFalse(Button("PreviousButton").interactable);
            Assert.AreEqual("1 / 7", Label("Counter").text);
        }

        [UnityTest]
        public IEnumerator LockedSkin_ShowsSilhouetteAndHowToGetIt()
        {
            _save = SaveGameIsolation.Reset();
            _save.Data.Stats.TotalKills = 660;
            yield return Load();
            _carousel.Open();

            _carousel.Show(5);
            yield return null;

            Assert.AreEqual("Zombiejaeger", _carousel.Shown.Id);
            Assert.AreEqual("Noch 340 Kills", Label("Hint").text);
            Assert.AreEqual(SkinHint.Locked, Button("SelectButton").GetComponentInChildren<Text>().text);
            Assert.IsFalse(Button("SelectButton").interactable);
            Color figure = GameObject.Find("Skin_Zombiejaeger").GetComponent<Image>().color;
            Assert.Less(figure.r + figure.g + figure.b, 0.5f, "locked skin is not a silhouette");

            _carousel.Show(1);
            yield return null;
            Assert.AreEqual("Schaffe Level 1-3", Label("Hint").text);

            _carousel.Select();
            Assert.IsTrue(string.IsNullOrEmpty(_save.Data.Skins.Current), "a locked skin was chosen");
        }

        [UnityTest]
        public IEnumerator ChoosingAnUnlockedSkin_SavesItAndDressesTheMenuFigure()
        {
            _save = SaveGameIsolation.Reset();
            _save.Data.Skins.Unlocked.Add("Sandsturm");
            yield return Load();
            _carousel.Open();
            _carousel.Next();
            yield return null;
            Assert.AreEqual("Sandsturm", _carousel.Shown.Id);
            Assert.IsTrue(Button("SelectButton").interactable);

            Button("SelectButton").onClick.Invoke();
            yield return null;

            Assert.AreEqual("Sandsturm", _save.Data.Skins.Current);
            Assert.AreEqual("Sandsturm", new SaveGameStore(SaveGameIsolation.Files, "playmode-tests").Load().Skins.Current);
            Assert.AreEqual(SkinHint.Chosen, Button("SelectButton").GetComponentInChildren<Text>().text);
            var menuFigure = GameObject.Find("Player").GetComponent<MenuFigure>();
            Assert.AreSame(_carousel.Shown.Idle, menuFigure.Animation);

            Button("BackButton").onClick.Invoke();
            Assert.IsFalse(_carousel.IsOpen);
        }

        [UnityTest]
        public IEnumerator Menu_StartsWithTheFigureInTheCurrentSkin()
        {
            _save = SaveGameIsolation.Reset();
            _save.Data.Skins.Unlocked.Add("Veteran");
            _save.Data.Skins.Current = "Veteran";
            yield return Load();

            var menuFigure = GameObject.Find("Player").GetComponent<MenuFigure>();
            Assert.AreEqual("Veteran_Idle", menuFigure.Animation.name);

            _carousel.Open();
            Assert.AreEqual("Veteran", _carousel.Shown.Id, "carousel does not start at the current skin");
        }

        [UnityTest]
        public IEnumerator Arrows_StopAtTheEnds()
        {
            _save = SaveGameIsolation.Reset();
            yield return Load();
            _carousel.Open();

            _carousel.Previous();
            Assert.AreEqual(0, _carousel.Index);
            _carousel.Show(99);
            Assert.AreEqual(6, _carousel.Index);
            _carousel.Next();
            Assert.AreEqual(6, _carousel.Index);
            yield return null;
            Assert.IsFalse(Button("NextButton").interactable);
        }
    }
}
