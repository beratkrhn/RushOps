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
    /// <summary>
    /// Screenshots of the level badges for their pull request (issue #288): „Level geschafft!“ with a new badge, the pause
    /// overlay, the island map and the three badge skins in the carousel. Written to <c>docs/art/badges/</c>. Explicit,
    /// so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the badge screenshots")]
    [Category("Screenshots")]
    public class BadgeScreenshots
    {
        private const string Dir = "docs/art/badges";

        [UnityTest]
        public IEnumerator Shots_Badges()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameService save = SaveGameIsolation.Reset();
            // An earlier try gave „Kombo“; this one adds „Unversehrt“ and „Blitz“.
            ProgressRules.RecordCompletion(save.Data.Progress, "Level_1-1", 2, 1300);
            ProgressRules.RecordBadges(save.Data.Progress, "Level_1-1", BadgeKind.Combo);
            yield return SceneManager.LoadSceneAsync("Level_1-1", LoadSceneMode.Single);
            yield return new WaitForSeconds(0.5f);
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;

            var pause = Object.FindFirstObjectByType<PauseController>();
            pause.Pause();
            yield return null;
            yield return StoreScreenshots.Capture("pause.png", Dir);
            pause.Resume();
            yield return null;

            Object.FindFirstObjectByType<LevelScore>().Keeper.RegisterKill(10);
            Object.FindFirstObjectByType<LevelResults>().ShowDone();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return StoreScreenshots.Capture("done.png", Dir);
            Time.timeScale = 1f;

            // A finished first island with a mix of badges.
            ProgressRules.RecordCompletion(save.Data.Progress, "Level_1-2", 3, 2100);
            ProgressRules.RecordBadges(save.Data.Progress, "Level_1-2", BadgeKind.Blitz);
            ProgressRules.RecordCompletion(save.Data.Progress, "Level_1-3", 1, 900);
            yield return SceneManager.LoadSceneAsync(IslandMap.SceneName, LoadSceneMode.Single);
            yield return new WaitForSeconds(0.5f);
            // The map opens on the newest island; the badges of island 1 are one step down.
            Object.FindFirstObjectByType<IslandMap>().Previous();
            yield return new WaitForSeconds(1f);
            yield return StoreScreenshots.Capture("map.png", Dir);

            // Späher stays locked to show its condition, the other two show their colours.
            save.Data.Skins.Unlocked.Add("Elite");
            save.Data.Skins.Unlocked.Add("Legende");
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return new WaitForSeconds(0.5f);
            var carousel = Object.FindFirstObjectByType<SkinCarousel>();
            carousel.Open();
            yield return new WaitForSeconds(1f);
            string[] skins = { "spaeher", "elite", "legende" };
            for (int i = 0; i < skins.Length; i++)
            {
                carousel.Show(11 + i);
                yield return new WaitForSeconds(1.2f);
                yield return StoreScreenshots.Capture($"skin_{skins[i]}.png", Dir);
            }

            SaveGameIsolation.Reset();
        }
    }
}
