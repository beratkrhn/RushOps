using System;
using System.Collections;
using NUnit.Framework;
using RushOps.Endless;
using RushOps.Player;
using RushOps.Save;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots for the daily run pull request (issue #280): the area „Endlos“ of the main menu with a week of daily
    /// bests and the break of a daily run, written to <c>docs/art/daily_menu.png</c> and <c>docs/art/daily_run.png</c>.
    /// Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the daily run screenshots")]
    [Category("Screenshots")]
    public class DailyRunScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_MenuAndRun()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameService save = SaveGameIsolation.Reset();
            DateTime today = DateTime.Now.Date;
            int[] scores = { 4210, 0, 3180, 5020, 0, 2650, 3890 };
            int[] rounds = { 11, 0, 9, 13, 0, 8, 10 };
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] > 0)
                {
                    DailyRecords.Record(save.Data.Daily, scores[i], rounds[i], DailySeed.DateKey(today.AddDays(-i)));
                }
            }

            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<EndlessMenu>().Open();
            yield return new WaitForSeconds(0.3f);
            yield return StoreScreenshots.Capture("daily_menu.png", "docs/art");

            EndlessStart.Round = 1;
            EndlessStart.Daily = true;
            try
            {
                yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
                yield return null;
                var player = Object.FindAnyObjectByType<PlayerMovement>();
                player.GetComponent<PlayerMoveInput>().enabled = false;
                player.GetComponent<PlayerAimInput>().enabled = false;
                yield return new WaitForSeconds(0.8f);
                yield return StoreScreenshots.Capture("daily_run.png", "docs/art");
            }
            finally
            {
                EndlessStart.Daily = false;
                SaveGameIsolation.Reset();
            }
        }
    }
}
