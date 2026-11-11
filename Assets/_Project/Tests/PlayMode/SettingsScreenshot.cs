using System.Collections;
using NUnit.Framework;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshot of the settings screen with the vibration toggle for its pull request (issue #300). Written to
    /// <c>docs/art/settings/</c>. Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the settings screenshot")]
    [Category("Screenshots")]
    public class SettingsScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_SettingsWithVibration()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
            yield return null;
            Object.FindFirstObjectByType<SettingsMenu>().Open();
            yield return new WaitForSecondsRealtime(0.5f);
            yield return StoreScreenshots.Capture("settings_vibration.png", "docs/art/settings");
            SaveGameIsolation.Reset();
        }
    }
}
