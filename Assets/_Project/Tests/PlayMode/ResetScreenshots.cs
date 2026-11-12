using System.Collections;
using NUnit.Framework;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots for the campaign reset pull request (issue #328): the settings with „Story zurücksetzen“ and the
    /// confirmation, written to <c>docs/art/campaign_reset_*.png</c>. Explicit; needs a graphics device.
    /// </summary>
    [Explicit("Produces the campaign reset screenshots")]
    [Category("Screenshots")]
    public class ResetScreenshots
    {
        [UnityTest]
        public IEnumerator Shot_SettingsAndConfirmation()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;
            GameObject.Find("SettingsButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            yield return StoreScreenshots.Capture("campaign_reset_settings.png", "docs/art");

            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;
            GameObject.Find("SettingsButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            GameObject.Find("ResetCampaignButton").GetComponent<Button>().onClick.Invoke();
            yield return null;
            yield return StoreScreenshots.Capture("campaign_reset_confirm.png", "docs/art");
        }
    }
}
