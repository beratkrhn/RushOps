using System.Collections;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Screenshots for the zombie dog pull request (issue #263): a pack running at the player and a dog crouching before
    /// its lunge, written to <c>docs/art/dog_pack.png</c> and <c>docs/art/dog_crouch.png</c>. Explicit, so normal test
    /// runs skip it; needs a graphics device.
    /// </summary>
    [Explicit("Produces the dog screenshots")]
    [Category("Screenshots")]
    public class DogScreenshot
    {
        [UnityTest]
        public IEnumerator Shot_PackAndCrouchingDog()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                Assert.Ignore("no graphics device (-nographics): nothing can render");
            }

            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync("Dog_Test", LoadSceneMode.Single);
            yield return null;
            Object.FindAnyObjectByType<WaveDirector>().enabled = false;
            Object.FindAnyObjectByType<LevelRestarter>().enabled = false;
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            player.GetComponent<PlayerMoveInput>().enabled = false;
            player.GetComponent<PlayerAimInput>().enabled = false;
            player.GetComponent<PlayerWeapon>().enabled = false;
            Vector2 center = player.transform.position;
            EnemyPool dogs = EnemyPools.Of(EnemyPools.Dog);

            // A pack comes in from the upper left, still out of lunge range.
            Vector2 spot = center + new Vector2(-4f, 9f);
            float spread = dogs.Definition.PackSpread;
            for (int i = 0; i < dogs.Definition.PackSize; i++)
            {
                float angle = i * 2f * Mathf.PI / dogs.Definition.PackSize;
                dogs.Spawn(spot + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spread);
            }

            yield return new WaitForSeconds(0.9f);
            yield return StoreScreenshots.Capture("dog_pack.png", "docs/art");

            foreach (Enemy enemy in dogs.Active)
            {
                enemy.Kill();
            }

            yield return new WaitForSeconds(1.5f);
            Dog dog = dogs.Spawn(center + new Vector2(2.2f, 1.6f)).GetComponent<Dog>();
            float timeout = Time.time + 2f;
            while (dog.State != DogState.Crouch && Time.time < timeout)
            {
                yield return null;
            }

            // Past the flash, so the crouching pose shows.
            yield return new WaitForSeconds(0.15f);
            Time.timeScale = 0f;
            yield return StoreScreenshots.Capture("dog_crouch.png", "docs/art");
            Time.timeScale = 1f;
            SaveGameIsolation.Reset();
        }
    }
}
