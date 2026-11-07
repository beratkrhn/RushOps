using System.Collections;
using NUnit.Framework;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots of the gold skin for its pull request (issue #237): the carousel with the locked skin and its price,
    /// the purchase question, the carousel after buying, and the main menu with the balance. Written to
    /// <c>docs/art/goldskin/</c>. Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the gold skin screenshots")]
    [Category("Screenshots")]
    public class GoldSkinScreenshots
    {
        private const string Dir = "docs/art/goldskin";

        [UnityTest]
        public IEnumerator Shots_GoldSkin()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            RushOps.Save.SaveGameService save = SaveGameIsolation.Reset();
            save.Data.Gold.Balance = 2600;
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return new WaitForSeconds(0.5f);
            yield return StoreScreenshots.Capture("menu_balance.png", Dir);

            var carousel = Object.FindFirstObjectByType<SkinCarousel>();
            carousel.Open();
            carousel.Show(10);
            yield return new WaitForSeconds(0.8f);
            yield return StoreScreenshots.Capture("carousel_locked.png", Dir);

            carousel.Buy();
            yield return null;
            yield return StoreScreenshots.Capture("carousel_confirm.png", Dir);

            carousel.ConfirmBuy();
            yield return new WaitForSeconds(0.5f);
            yield return StoreScreenshots.Capture("carousel_bought.png", Dir);
            SaveGameIsolation.Reset();
        }
    }
}
