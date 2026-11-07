using System.Collections;
using NUnit.Framework;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshot of the main menu with the „Endlos“ button for its pull request (issue #208), written to
    /// <c>docs/art/menu/Hauptmenue.png</c>. Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the main menu screenshot")]
    [Category("Screenshots")]
    public class MainMenuScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_MainMenu()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return new WaitForSeconds(0.5f);
            yield return StoreScreenshots.Capture("Hauptmenue.png", "docs/art/menu");
            SaveGameIsolation.Reset();
        }
    }
}
