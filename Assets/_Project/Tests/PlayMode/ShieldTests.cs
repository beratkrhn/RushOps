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
    public class ShieldTests
    {
        private const string SceneName = "Arena_Test";
        // Free floor along the east wall of Arena_Test, far from the player's start.
        private static readonly Vector2 Spot = new Vector2(34.5f, 18.5f);

        private PlayerPerks _perks;
        private PlayerShield _shield;
        private PlayerHealth _health;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;

            _perks = Object.FindFirstObjectByType<PlayerPerks>();
            Assert.IsNotNull(_perks, "Arena_Test has no PlayerPerks");
            _shield = _perks.GetComponent<PlayerShield>();
            Assert.IsNotNull(_shield, "player has no PlayerShield");
            _health = _perks.GetComponent<PlayerHealth>();
            Object.FindFirstObjectByType<WaveDirector>().enabled = false;
            Object.FindFirstObjectByType<LevelRestarter>().enabled = false;
            _perks.GetComponent<PlayerMoveInput>().enabled = false;
            _perks.GetComponent<PlayerAimInput>().enabled = false;
            _perks.GetComponent<PlayerWeapon>().enabled = false;
        }

        [TearDown]
        public void RestoreTime()
        {
            Time.timeScale = 1f;
        }

        [Test]
        public void Definition_LastsEightSeconds()
        {
            PerkDefinition shield = Shield();
            Assert.AreEqual(PerkEffect.Shield, shield.Effect);
            Assert.AreEqual(PerkKind.Timed, shield.Kind);
            Assert.AreEqual(8f, shield.Duration, 1e-5f);
        }

        [UnityTest]
        public IEnumerator Shield_BlocksDirectDamage_AndShowsBarrier()
        {
            Assert.IsFalse(_shield.IsBarrierVisible, "barrier visible without shield");
            int before = _health.Current;

            _perks.Apply(Shield());
            yield return null;

            Assert.IsTrue(_shield.IsActive);
            Assert.IsTrue(_health.IsInvulnerable);
            Assert.IsTrue(_shield.IsBarrierVisible, "no barrier while the shield runs");
            Assert.IsFalse(_health.TakeDamage(1), "damage counted behind the shield");
            Assert.AreEqual(before, _health.Current);
        }

        [UnityTest]
        public IEnumerator Shield_BlocksExplosions()
        {
            Teleport(Spot + new Vector2(0f, 3f));
            _perks.Apply(Shield());
            Enemy boomer = Place(EnemyPools.Of(EnemyPools.Boomer), Spot + new Vector2(0f, 2f));
            yield return new WaitForFixedUpdate();
            int before = _health.Current;
            bool exploded = false;
            var explosions = Object.FindFirstObjectByType<ExplosionSystem>();
            explosions.Exploded += (center, definition) => exploded = true;

            boomer.Kill();
            float timeout = Time.realtimeSinceStartup + 3f;
            while (Time.realtimeSinceStartup < timeout && !exploded)
            {
                yield return null;
            }

            Assert.IsTrue(exploded, "boomer did not explode");
            Assert.AreEqual(before, _health.Current, "the blast hurt the player behind the shield");
        }

        [UnityTest]
        public IEnumerator TouchingEnemy_DealsNoDamage_AndIsPushedBack()
        {
            Teleport(Spot);
            _perks.Apply(Shield());
            yield return new WaitForFixedUpdate();
            int before = _health.Current;
            Enemy walker = EnemyPools.Of(EnemyPools.Walker).Spawn(Spot + new Vector2(0f, 1.5f));
            var movement = walker.GetComponent<EnemyMovement>();

            bool pushed = false;
            float timeout = Time.realtimeSinceStartup + 4f;
            while (Time.realtimeSinceStartup < timeout && !pushed)
            {
                pushed = movement.IsKnockedBack;
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(pushed, "walker touching the shield was not pushed back");
            Assert.Greater(movement.Velocity.y, 0f, "walker was not pushed away from the player");
            for (int i = 0; i < 30; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(before, _health.Current, "walker hurt the player behind the shield");
        }

        [UnityTest]
        public IEnumerator Barrier_FlickersInLastSeconds_ThenShieldEnds()
        {
            // A short shield starts inside the warning time, so the barrier flickers at once.
            _perks.Active.Activate("shield", PerkEffect.Shield, 1f, 1f);
            bool seen = false;
            bool hidden = false;
            float timeout = Time.time + 0.8f;
            while (Time.time < timeout)
            {
                yield return null;
                seen |= _shield.IsBarrierVisible;
                hidden |= !_shield.IsBarrierVisible;
            }

            Assert.IsTrue(seen && hidden, "barrier did not flicker in the last seconds");
            yield return new WaitForSeconds(0.4f);

            Assert.IsFalse(_shield.IsActive);
            Assert.IsFalse(_shield.IsBarrierVisible, "barrier stays after the shield ended");
            Assert.IsTrue(_health.TakeDamage(1), "player still protected after the shield ended");
        }

        private static PerkDefinition Shield()
        {
            foreach (PerkDefinition perk in Object.FindFirstObjectByType<PerkPickupPool>().Config.Perks)
            {
                if (perk.Id == "shield")
                {
                    return perk;
                }
            }

            Assert.Fail("LootConfig has no shield perk");
            return null;
        }

        private static Enemy Place(EnemyPool pool, Vector2 position)
        {
            Enemy enemy = pool.Spawn(position);
            enemy.GetComponent<EnemyMovement>().Stop();
            return enemy;
        }

        private void Teleport(Vector2 position)
        {
            var body = _perks.GetComponent<Rigidbody2D>();
            body.position = position;
            body.transform.position = position;
        }
    }
}
