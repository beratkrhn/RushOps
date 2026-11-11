using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.Save;
using RushOps.Scoring;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Level badges in a campaign level and on the island map (issue #288, F-SCO-04, F-SCO-05, F-UI-08).</summary>
    public class LevelBadgeTests
    {
        private const string LevelScene = "Level_1-1";

        private SaveGameService _save;
        private LevelResults _results;
        private PlayerHealth _player;

        [UnitySetUp]
        public IEnumerator LoadLevel()
        {
            _save = SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(LevelScene, LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            _results = Object.FindFirstObjectByType<LevelResults>();
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            Assert.IsNotNull(Object.FindFirstObjectByType<LevelBadges>(), "level has no badge tracker");
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
            SaveGameIsolation.Reset();
        }

        [UnityTest]
        public IEnumerator FinishingWithoutAHit_GivesUnharmed_AndTheMapShowsIt()
        {
            yield return new WaitForSeconds(0.2f);
            _results.ShowDone();
            yield return null;

            BadgeKind saved = ProgressRules.BadgesOf(_save.Data.Progress, LevelScene);
            Assert.IsTrue(BadgeRules.Has(saved, BadgeKind.Unharmed), "no „Unversehrt“ after a level without a hit");
            Assert.IsTrue(BadgeRules.Has(saved, BadgeKind.Blitz), "a quick finish should give „Blitz“");
            Assert.IsFalse(BadgeRules.Has(saved, BadgeKind.Combo), "no kills, no „Kombo“");

            var row = Object.FindFirstObjectByType<BadgeRowView>();
            Assert.IsNotNull(row, "„Level geschafft!“ has no badge row");
            Assert.AreEqual(saved, row.Shown);
            Assert.AreEqual(saved, row.ShownNew, "first finish: every badge is new");

            Time.timeScale = 1f;
            yield return SceneManager.LoadSceneAsync(IslandMap.SceneName, LoadSceneMode.Single);
            yield return null;
            LevelNode node = FindNode(0, 0);
            Assert.AreEqual(saved, node.BadgesShown);
        }

        [UnityTest]
        public IEnumerator AHit_PreventsUnharmed()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsTrue(_player.TakeDamage(1), "hit did not count");
            yield return null;
            _results.ShowDone();
            yield return null;

            BadgeKind saved = ProgressRules.BadgesOf(_save.Data.Progress, LevelScene);
            Assert.IsFalse(BadgeRules.Has(saved, BadgeKind.Unharmed));
            Assert.IsTrue(BadgeRules.Has(saved, BadgeKind.Blitz));
        }

        [UnityTest]
        public IEnumerator PauseOverlay_ListsTheThreeConditions()
        {
            var pause = Object.FindFirstObjectByType<PauseController>();
            pause.Pause();
            yield return null;

            var list = Object.FindFirstObjectByType<BadgeListView>();
            Assert.IsNotNull(list, "pause overlay has no badge list");
            Assert.AreEqual("Unversehrt: kein Herz verlieren", list.TextOf(BadgeKind.Unharmed));
            StringAssert.StartsWith("Blitz: unter ", list.TextOf(BadgeKind.Blitz));
            Assert.AreEqual("Kombo: x5 erreichen", list.TextOf(BadgeKind.Combo));
            Assert.IsTrue(list.ShowsPossible(BadgeKind.Unharmed));
            pause.Resume();

            _player.TakeDamage(1);
            yield return null;
            pause.Pause();
            yield return null;
            Assert.IsFalse(list.ShowsPossible(BadgeKind.Unharmed), "a lost heart should dim „Unversehrt“");
            pause.Resume();
        }

        private static LevelNode FindNode(int island, int level)
        {
            foreach (LevelNode node in Object.FindObjectsByType<LevelNode>(FindObjectsSortMode.None))
            {
                if (node.Island == island && node.Level == level)
                {
                    return node;
                }
            }

            Assert.Fail($"no node {island}-{level}");
            return null;
        }
    }
}
