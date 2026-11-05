using System.Collections;
using NUnit.Framework;
using RushOps.Bosses;
using RushOps.Player;
using RushOps.Save;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    public class WeaponSelectionTests
    {
        private SaveGameService _save;

        [UnityTearDown]
        public IEnumerator FreshSave()
        {
            Time.timeScale = 1f;
            SaveGameIsolation.Reset();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Carousel_ShowsLockedWeaponsWithTheirBoss()
        {
            _save = SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;
            var carousel = Object.FindFirstObjectByType<WeaponCarousel>();
            Assert.IsNotNull(carousel, "issue #176: the main menu has a weapon selection");
            Button button = GameObject.Find("WeaponsButton").GetComponent<Button>();
            Assert.AreEqual("Waffenwahl", button.GetComponentInChildren<Text>().text);

            button.onClick.Invoke();
            yield return null;
            Assert.IsTrue(carousel.IsOpen);
            Assert.AreEqual("Pistole", carousel.Shown.DisplayName);
            Assert.AreEqual(string.Empty, carousel.HintShown);

            carousel.Next();
            Assert.AreEqual("Schrotflinte", carousel.Shown.DisplayName);
            Assert.AreEqual("Besiege den Koloss", carousel.HintShown, "W5: locked weapons show their condition");
            carousel.Select();
            Assert.AreEqual(string.Empty, _save.Data.Weapons.Current, "a locked weapon cannot be chosen");
        }

        [UnityTest]
        public IEnumerator ChosenWeapon_IsSaved_AndInTheFirstSlotAtTheLevelStart()
        {
            _save = SaveGameIsolation.Reset();
            ProgressRules.RecordCompletion(_save.Data.Progress, "Level_1-3", 1, 1000);
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;
            var carousel = Object.FindFirstObjectByType<WeaponCarousel>();
            carousel.Open();
            CollectionAssert.Contains(_save.Data.Weapons.Unlocked, "Shotgun", "a boss beaten earlier unlocks its weapon");

            carousel.Show(1);
            carousel.Select();
            Assert.AreEqual("Shotgun", _save.Data.Weapons.Current);
            Assert.IsTrue(System.Linq.Enumerable.Any(SaveGameIsolation.Files.Files.Values, f => f.Contains("\"Shotgun\"")),
                "saved to disk");

            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            PlayerWeapon weapon = Object.FindFirstObjectByType<PlayerWeapon>();
            Assert.AreEqual("Schrotflinte", weapon.ActiveWeapon.DisplayName, "W5: the level starts with the chosen weapon");
            Assert.AreSame(weapon.ActiveWeapon, weapon.Slots.Selected, "in the first slot");
        }

        [UnityTest]
        public IEnumerator BeatingTheKoloss_UnlocksTheShotgun_AndTheResultSaysSo()
        {
            _save = SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Level_1-3", LoadSceneMode.Single);
            yield return null;
            var fight = Object.FindAnyObjectByType<BossFight>();
            var body = GameObject.Find("Player").GetComponent<Rigidbody2D>();
            // Like Level1_3SceneTests: beside the Koloss, inside the arena.
            var arena = new Vector2(4f, fight.Bodies[0].transform.position.y);
            body.position = arena;
            body.transform.position = arena;
            float waited = 0f;
            while (!fight.IsRunning && waited < 2f)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(fight.IsRunning, "the fight did not start");
            Boss boss = fight.Bodies[0];
            boss.TakeHit(boss.MaxHealth, Vector2.up);
            yield return new WaitForSecondsRealtime(boss.Definition.VictoryDelay + 0.3f);

            var view = Object.FindFirstObjectByType<UnlockedWeaponView>();
            Assert.IsNotNull(view.Shown, "no „Neue Waffe“ on the result screen");
            Assert.AreEqual("Schrotflinte", view.Shown.DisplayName);
            CollectionAssert.Contains(_save.Data.Weapons.Unlocked, "Shotgun");
        }
    }
}
