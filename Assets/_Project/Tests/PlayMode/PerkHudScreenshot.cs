using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshot for the perk HUD pull request (issue #247): timers of the drone and the scarecrow plus the name
    /// flash, written to <c>docs/art/perk_hud.png</c>. Explicit, so normal test runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the perk HUD screenshot")]
    [Category("Screenshots")]
    public class PerkHudScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_TimersOfDroneAndScarecrow()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<WaveDirector>().enabled = false;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;

            var perks = Object.FindAnyObjectByType<PlayerPerks>();
            perks.Apply(AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/Companion.asset"));
            yield return new WaitForSeconds(3f);
            perks.Apply(AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/Scarecrow.asset"));
            yield return new WaitForSeconds(0.2f);

            yield return StoreScreenshots.Capture("perk_hud.png", "docs/art");
            SaveGameIsolation.Reset();
        }

        /// <summary>Super weapon timer next to another perk timer plus the name flash (issue #256), <c>docs/art/super_weapon_hud.png</c>.</summary>
        [UnityTest]
        public IEnumerator Shot_TimerOfTheRocketLauncher()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<WaveDirector>().enabled = false;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;

            var perks = Object.FindAnyObjectByType<PlayerPerks>();
            perks.Apply(AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/DoubleDamage.asset"));
            yield return new WaitForSeconds(3f);
            perks.Apply(AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/RocketLauncher.asset"));
            yield return new WaitForSeconds(0.2f);

            yield return StoreScreenshots.Capture("super_weapon_hud.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
