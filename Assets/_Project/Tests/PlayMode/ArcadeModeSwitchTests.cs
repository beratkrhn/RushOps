using System.Collections;
using NUnit.Framework;
using RushOps.Arcade;
using RushOps.Endless;
using RushOps.Player;
using RushOps.Save;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Switch „Darstellung: Normal | Arcade“ and the own lists of arcade runs (issue #325, F-ARC-08).</summary>
    public class ArcadeModeSwitchTests
    {
        private const float Timeout = 30f;

        [TearDown]
        public void TearDown()
        {
            EndlessStart.Arcade = false;
            EndlessStart.Daily = false;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator Switch_SavesTheChoice_AndStartsTheRunInTheCabinet()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            yield return null;
            var menu = Object.FindAnyObjectByType<EndlessMenu>();
            menu.Open();
            Assert.AreEqual("Normal", menu.DisplayText, "Normal is the default");
            Assert.IsFalse(menu.ArcadeSelected);

            Button display = GameObject.Find("DisplayButton").GetComponent<Button>();
            display.onClick.Invoke();
            Assert.AreEqual("Arcade", menu.DisplayText);
            Assert.IsTrue(SaveGameIsolation.Service.Data.Arcade.Selected);
            var loaded = new SaveGameStore(SaveGameIsolation.Files, "playmode-tests").Load();
            Assert.IsTrue(loaded.Arcade.Selected, "saved at once");

            GameObject.Find("StartButton").GetComponent<Button>().onClick.Invoke();
            // The walk to the cabinet plays first (issue #324); a tap skips it.
            var intro = Object.FindAnyObjectByType<ArcadeIntro>();
            Assert.IsTrue(intro.IsPlaying);
            intro.Skip();
            yield return null;
            yield return null;
            Assert.AreEqual("Endless_Arena", SceneManager.GetActiveScene().name);
            Assert.IsTrue(Object.FindAnyObjectByType<ArcadeCabinet>().IsActive, "the run shows in the cabinet");
        }

        [UnityTest]
        public IEnumerator Switch_BackToNormal_StartsANormalRun()
        {
            SaveGameIsolation.Reset();
            SaveGameIsolation.Service.Data.Arcade.Selected = true;
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            yield return null;
            var menu = Object.FindAnyObjectByType<EndlessMenu>();
            menu.Open();
            Assert.AreEqual("Arcade", menu.DisplayText, "the saved choice shows");
            menu.ToggleDisplay();
            Assert.AreEqual("Normal", menu.DisplayText);

            GameObject.Find("DailyButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            yield return null;
            Assert.IsFalse(Object.FindAnyObjectByType<ArcadeCabinet>().IsActive);
        }

        [UnityTest]
        public IEnumerator ArcadeRun_GoesIntoTheArcadeTopTen_GoldAndRunsCountAsUsual()
        {
            SaveGameService save = SaveGameIsolation.Reset();
            EndlessRecords.Record(save.Data.Endless, 99999, 30, "2026-01-01");
            yield return ArcadeRunTests.LoadWithoutReset(true);
            var controller = Object.FindAnyObjectByType<EndlessRunController>();
            var hud = Object.FindAnyObjectByType<EndlessHud>();
            var health = Object.FindAnyObjectByType<PlayerHealth>();
            Assert.IsTrue(controller.IsArcade);
            float waited = 0f;
            while (controller.Run.Phase != EndlessPhase.Running && waited < Timeout)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            Object.FindAnyObjectByType<RushOps.Scoring.LevelScore>().Keeper.AddPoints(1500);
            health.TakeDamage(health.Max);
            waited = 0f;
            while (!hud.ShowsResult && waited < Timeout)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            var loaded = new SaveGameStore(SaveGameIsolation.Files, "playmode-tests").Load();
            Assert.AreEqual(1, loaded.Arcade.Endless.Top.Count, "into the arcade top 10");
            Assert.AreEqual(1500, loaded.Arcade.Endless.Top[0].Score);
            Assert.AreEqual(1, loaded.Endless.Top.Count, "the normal top 10 stays");
            Assert.AreEqual(2, loaded.Endless.Runs, "counts as a run");
            Assert.AreEqual(1, loaded.Stats.Deaths);
            Assert.AreEqual("Platz 1 der Top 10", hud.RankText);
            StringAssert.Contains("1500", hud.TopColumnText(1));
            StringAssert.DoesNotContain("99999", hud.TopColumnText(1));
        }
    }
}
