using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots for the scarecrow pull request (issue #246): zombies gathered around the scarecrow and its blast,
    /// written to <c>docs/art/scarecrow_lure.png</c> and <c>docs/art/scarecrow_blast.png</c>. Explicit, so normal test
    /// runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the scarecrow screenshots")]
    [Category("Screenshots")]
    public class ScarecrowScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_ZombiesAroundTheScarecrow_AndTheBlast()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Arena_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<WaveDirector>().enabled = false;
            Object.FindAnyObjectByType<LevelRestarter>().enabled = false;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;
            player.GetComponent<PlayerWeapon>().enabled = false;
            Vector2 center = player.transform.position;
            Vector2 spot = center + new Vector2(0f, 4f);

            var perk = UnityEditor.AssetDatabase.LoadAssetAtPath<PerkDefinition>("Assets/_Project/Data/Perks/Scarecrow.asset");
            Object.FindAnyObjectByType<PerkPickupPool>().Spawn(spot, perk).Collect();
            var scarecrow = Object.FindAnyObjectByType<Scarecrow>();
            bool blasted = false;
            scarecrow.Detonated += _ => blasted = true;

            // Around the player, so without the lure they would come for the player.
            Vector2[] offsets =
            {
                new Vector2(-3.5f, 1.5f), new Vector2(3.5f, 1.5f), new Vector2(-2.5f, -2f), new Vector2(2.5f, -2f),
                new Vector2(-4f, 6f), new Vector2(4f, 6.5f), new Vector2(0f, 8.5f),
            };
            string[] types = { EnemyPools.Walker, EnemyPools.Runner, EnemyPools.Walker, EnemyPools.Boomer };
            for (int i = 0; i < offsets.Length; i++)
            {
                EnemyPools.Of(types[i % types.Length]).Spawn(center + offsets[i]);
            }

            yield return new WaitForSeconds(4f);
            yield return StoreScreenshots.Capture("scarecrow_lure.png", "docs/art");

            float timeout = Time.realtimeSinceStartup + 8f;
            while (Time.realtimeSinceStartup < timeout && !blasted)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.12f);
            yield return StoreScreenshots.Capture("scarecrow_blast.png", "docs/art");
            SaveGameIsolation.Reset();
        }
    }
}
