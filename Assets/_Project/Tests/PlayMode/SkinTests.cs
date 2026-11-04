using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Player;
using RushOps.Save;
using RushOps.Skins;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    public class SkinTests
    {
        private const string SceneName = "Arena_Zonen";

        private static IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator NewGame_PlaysInTheStandardSkin()
        {
            SaveGameIsolation.Reset();
            yield return Load();

            var skin = Object.FindFirstObjectByType<PlayerSkin>();
            Assert.IsNotNull(skin, "player has no PlayerSkin");
            Assert.IsNotNull(skin.Skin);
            Assert.AreEqual(SkinUnlockKind.Default, skin.Skin.UnlockKind);
        }

        [UnityTest]
        public IEnumerator ChosenSkin_IsShownWithoutChangingGameValues()
        {
            SaveGameService save = SaveGameIsolation.Reset();
            save.Data.Skins.Unlocked.Add("Veteran");
            save.Data.Skins.Current = "Veteran";
            yield return Load();

            var skin = Object.FindFirstObjectByType<PlayerSkin>();
            Assert.AreEqual("Veteran", skin.Skin.Id);
            Assert.IsNotNull(skin.GetComponent<SpriteRenderer>().sprite);
            PlayerConfig config = skin.GetComponent<PlayerMovement>().Config;
            Assert.AreEqual(config.MaxHearts, skin.GetComponent<PlayerHealth>().Max, "a skin must not change the hearts");
        }

        [UnityTest]
        public IEnumerator ReachingAMilestone_ShowsAndSavesTheNewSkin()
        {
            SaveGameService save = SaveGameIsolation.Reset();
            yield return Load();
            save.Data.Stats.TotalKills = 100;
            var view = Object.FindFirstObjectByType<UnlockedSkinView>();
            Assert.IsNotNull(view, "„Level geschafft!“ has no UnlockedSkinView");
            Assert.IsNull(view.Shown);

            var player = Object.FindFirstObjectByType<PlayerHealth>().GetComponent<Rigidbody2D>();
            Vector2 exit = Object.FindFirstObjectByType<LevelExit>().transform.position;
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.position = exit;
            player.transform.position = exit;
            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsTrue(Object.FindFirstObjectByType<LevelResults>().ShowsDone);
            Assert.IsNotNull(view.Shown, "no new skin shown");
            Assert.AreEqual("Veteran", view.Shown.Id);
            CollectionAssert.Contains(new SaveGameStore(SaveGameIsolation.Files, "playmode-tests").Load().Skins.Unlocked, "Veteran");
        }
    }
}
