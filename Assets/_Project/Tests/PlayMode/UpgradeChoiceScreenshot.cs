using System.Collections;
using NUnit.Framework;
using RushOps.Endless;
using RushOps.Enemies;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots for the upgrade choice pull request (issue #279): the three cards after round 3 and the pause overlay
    /// with the upgrades of the run, written to <c>docs/art/upgrade_choice.png</c> and <c>docs/art/upgrade_pause.png</c>.
    /// Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the upgrade choice screenshots")]
    [Category("Screenshots")]
    public class UpgradeChoiceScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_ChoiceAndPauseList()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            EndlessStart.Round = 1;
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;
            var controller = Object.FindAnyObjectByType<EndlessRunController>();
            EndlessConfig config = controller.Rules.Config;
            float pause = config.PauseBetweenRounds;
            int hearts = config.HeartsPerRound;
            var health = Object.FindAnyObjectByType<PlayerHealth>();
            health.GetComponent<PlayerMoveInput>().enabled = false;
            health.GetComponent<PlayerAimInput>().enabled = false;
            // The same three cards every time the shot is taken.
            controller.SetRandomSource(new RushOps.Perks.SystemRandomSource(7));
            config.ConfigureRounds(0.2f, hearts);
            try
            {
                controller.BeginRun(3);
                float end = Time.realtimeSinceStartup + 20f;
                while (controller.Run.Phase != EndlessPhase.Choice && Time.realtimeSinceStartup < end)
                {
                    foreach (Enemy enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                    {
                        if (enemy.isActiveAndEnabled && !enemy.IsDead)
                        {
                            enemy.Kill();
                        }
                    }

                    yield return null;
                }

                Assert.AreEqual(EndlessPhase.Choice, controller.Run.Phase);
                yield return StoreScreenshots.Capture("upgrade_choice.png", "docs/art");

                var view = Object.FindAnyObjectByType<UpgradeChoiceView>();
                view.Cards[0].Button.onClick.Invoke();
                // A few more, as if from later rounds, so the list has several lines.
                var debug = controller.GetComponent<DebugUpgradeKey>();
                debug.Grant("bounce");
                debug.Grant("damage");
                debug.Grant("damage");
                debug.Grant("quick_feet");
                var pauseController = Object.FindAnyObjectByType<PauseController>();
                pauseController.Pause();
                yield return StoreScreenshots.Capture("upgrade_pause.png", "docs/art");
                pauseController.Resume();
            }
            finally
            {
                config.ConfigureRounds(pause, hearts);
                Time.timeScale = 1f;
                SaveGameIsolation.Reset();
            }
        }
    }
}
