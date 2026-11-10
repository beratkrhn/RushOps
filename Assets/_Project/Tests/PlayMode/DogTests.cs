using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Navigation;
using RushOps.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// The zombie dog in the test scene Dog_Test (issue #263, F-GEG-05, F-GEG-06): packs spawn together, a dog in range
    /// crouches 0.4 s with a flash and a growl, then leaps to where the player stood; a dodge makes it miss.
    /// </summary>
    public class DogTests
    {
        private const string SceneName = "Dog_Test";
        // One physics step of slack on every timing.
        private const float StepTolerance = 0.03f;

        private EnemyPool _dogs;
        private PlayerHealth _player;
        private Rigidbody2D _playerBody;
        private WaveDirector _director;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _dogs = EnemyPools.Of(EnemyPools.Dog);
            _player = Object.FindFirstObjectByType<PlayerHealth>();
            _playerBody = _player.GetComponent<Rigidbody2D>();
            _director = Object.FindFirstObjectByType<WaveDirector>();
            // Touching the player must not reload the scene, and the player must neither move nor shoot by itself.
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _player.GetComponent<PlayerMoveInput>().enabled = false;
            _player.GetComponent<PlayerAimInput>().enabled = false;
            _player.GetComponent<PlayerWeapon>().enabled = false;
        }

        [UnityTearDown]
        public IEnumerator ResetSave()
        {
            SaveGameIsolation.Reset();
            yield return null;
        }

        [Test]
        public void Prefab_IsADogWithItsSounds()
        {
            EnemyDefinition dog = _dogs.Definition;

            Assert.IsNotNull(dog.Dog, "the dog needs its pack and lunge values");
            Assert.AreEqual(3, dog.PackSize);
            Assert.AreEqual(1, dog.ContactDamage, "a dog takes 1 heart (H3)");
            Assert.IsNotNull(dog.SpawnClip);
            Assert.IsNotNull(dog.Dog.GrowlClip, "the crouch is audible (F-GEG-06)");
            Assert.IsNotNull(dog.Dog.LungeClip, "the lunge has its own sound");
            Assert.AreNotEqual(dog.Dog.GrowlClip, dog.Dog.LungeClip);
        }

        [UnityTest]
        public IEnumerator Pack_SpawnsTogetherAtOnePoint()
        {
            // Portrait as on the phone: a landscape batchmode view would see every spawn point, and enemies only spawn
            // out of sight.
            Camera.main.aspect = 9f / 16f;
            var spawned = new List<Enemy>();
            _dogs.EnemySpawned += spawned.Add;
            float timeout = Time.time + 10f;
            while (spawned.Count < 3 && Time.time < timeout)
            {
                yield return null;
            }

            _director.enabled = false;
            _dogs.EnemySpawned -= spawned.Add;

            Assert.GreaterOrEqual(spawned.Count, 3, "the waves of Dog_Test sent no pack");
            float spread = _dogs.Definition.PackSpread;
            for (int i = 1; i < 3; i++)
            {
                float apart = Vector2.Distance(spawned[0].transform.position, spawned[i].transform.position);
                Assert.LessOrEqual(apart, 2f * spread + 0.2f, "the dogs of a pack share one spawn point (H5)");
            }

            foreach (Enemy dog in spawned)
            {
                Assert.IsNotNull(dog.GetComponent<Dog>(), "every pack member is a dog");
            }
        }

        [UnityTest]
        public IEnumerator InRange_CrouchesWithFlashAndGrowl_ThenLunges_StandingPlayerLosesOneHeart()
        {
            _director.enabled = false;
            int crouched = 0;
            int lunged = 0;
            _dogs.EnemyCrouched += _ => crouched++;
            _dogs.EnemyLunged += _ => lunged++;
            Vector2 playerAt = _playerBody.position;
            Dog dog = SpawnDog(playerAt + new Vector2(2.5f, 0f));

            yield return WaitForState(dog, DogState.Crouch, 3f);
            float crouchStart = Time.fixedTime;
            Assert.AreEqual(1, crouched, "the crouch growls once");
            Assert.IsTrue(dog.Enemy.IsFlashing, "the crouch flashes");
            Assert.That(Vector2.Distance(dog.LungeTarget, playerAt), Is.LessThan(0.05f), "the lunge aims at the player");
            int hearts = _player.Current;
            Vector2 crouchedAt = dog.transform.position;

            yield return WaitForState(dog, DogState.Lunge, 1f);
            float crouchTime = Time.fixedTime - crouchStart;
            Assert.That(crouchTime, Is.EqualTo(_dogs.Definition.Dog.CrouchDuration).Within(StepTolerance),
                "the dog crouches 0.4 s (F-GEG-06)");
            Assert.That(Vector2.Distance(crouchedAt, dog.transform.position), Is.LessThan(0.1f),
                "the dog stands still while it crouches");
            Assert.AreEqual(1, lunged, "the lunge has its sound");

            yield return WaitForState(dog, DogState.Recovery, 1f);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(hearts - 1, _player.Current, "a standing player loses 1 heart");
        }

        [UnityTest]
        public IEnumerator Lunge_GoesToTheOldPosition_DodgingPlayerStaysUnhurt()
        {
            _director.enabled = false;
            Vector2 playerAt = _playerBody.position;
            Dog dog = SpawnDog(playerAt + new Vector2(2.5f, 0f));

            yield return WaitForState(dog, DogState.Crouch, 3f);
            int hearts = _player.Current;
            // Sideways out of the lunge line while the dog crouches.
            Vector2 dodged = playerAt + new Vector2(0f, 2f);
            _playerBody.position = dodged;
            _player.transform.position = dodged;

            yield return WaitForState(dog, DogState.Lunge, 1f);
            yield return WaitForState(dog, DogState.Recovery, 1f);
            yield return new WaitForFixedUpdate();

            Assert.That(Vector2.Distance(dog.LungeTarget, playerAt), Is.LessThan(0.05f),
                "the lunge keeps the position from the start of the crouch");
            Assert.That(Vector2.Distance(dog.GetComponent<Rigidbody2D>().position, playerAt), Is.LessThan(0.5f),
                "the dog lands where the player stood");
            Assert.AreEqual(hearts, _player.Current, "a dodging player stays unhurt");
        }

        [UnityTest]
        public IEnumerator Dog_DiesLikeAnyEnemy()
        {
            _director.enabled = false;
            Dog dog = SpawnDog(_playerBody.position + new Vector2(6f, 0f));
            int died = 0;
            _dogs.EnemyDied += _ => died++;
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(dog.Enemy.Kill());

            Assert.IsTrue(dog.Enemy.IsDead);
            Assert.AreEqual(1, died, "kill counter, points, gold and perk drops listen to EnemyDied");
            yield return new WaitForSeconds(dog.Enemy.DeathTime + 0.2f);
            Assert.IsFalse(dog.gameObject.activeSelf, "the dead dog returns to the pool");
        }

        [Test]
        public void EverySystemThatKnowsTheEnemies_KnowsTheDogs()
        {
            AssertDogPoolWired();
        }

        [UnityTest]
        public IEnumerator EndlessArena_EverySystemThatKnowsTheEnemies_KnowsTheDogs()
        {
            yield return SceneManager.LoadSceneAsync("Endless_Arena", LoadSceneMode.Single);
            yield return null;

            AssertDogPoolWired();
        }

        [UnityTest]
        public IEnumerator Lured_DoesNotLunge()
        {
            _director.enabled = false;
            var navigator = Object.FindFirstObjectByType<FlowFieldNavigator>();
            navigator.SetLure(_playerBody.position + new Vector2(0f, 6f), 1f);
            Dog dog = SpawnDog(_playerBody.position + new Vector2(2.5f, 0f));

            for (int i = 0; i < 25; i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.AreEqual(DogState.Chase, dog.State, "a dog lured by the scarecrow does not lunge at the player");
            }

            navigator.ClearLure();
        }

        // Nuke, drone, explosions (boomer, rocket, scarecrow), aim assist, kills, score, gold and perk drops all take
        // their enemies from pool arrays; each one that lists the walkers must list the dogs as well.
        private static void AssertDogPoolWired()
        {
            EnemyPool dogs = EnemyPools.Of(EnemyPools.Dog);
            EnemyPool walkers = EnemyPools.Of(EnemyPools.Walker);
            int systems = 0;
            foreach (MonoBehaviour behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                foreach (FieldInfo field in behaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (field.FieldType != typeof(EnemyPool[]) || field.GetValue(behaviour) is not EnemyPool[] pools)
                    {
                        continue;
                    }

                    if (System.Array.IndexOf(pools, walkers) >= 0)
                    {
                        systems++;
                        Assert.Contains(dogs, pools, $"{behaviour.GetType().Name}.{field.Name} misses the dogs");
                    }
                }
            }

            Assert.GreaterOrEqual(systems, 8, "expected nuke, drone, explosions, effects, feel, drops, kills and score");
        }

        private Dog SpawnDog(Vector2 position)
        {
            Enemy enemy = _dogs.Spawn(position);
            Assert.IsNotNull(enemy);
            var dog = enemy.GetComponent<Dog>();
            Assert.IsNotNull(dog);
            return dog;
        }

        private static IEnumerator WaitForState(Dog dog, DogState state, float seconds)
        {
            float timeout = Time.fixedTime + seconds;
            while (dog.State != state && Time.fixedTime < timeout)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(state, dog.State, $"the dog did not reach {state} within {seconds} s");
        }
    }
}
