using NUnit.Framework;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>The dog's announced lunge with the start values of the backlog v1.5.0 (issue #262, F-GEG-06).</summary>
    public class DogLungeTests
    {
        private const float Step = 0.02f;

        private DogConfig _config;
        private DogLunge _lunge;

        [SetUp]
        public void Create()
        {
            _config = ScriptableObject.CreateInstance<DogConfig>();
            _config.ConfigurePack(3, 0.5f);
            _config.ConfigureLunge(3f, 0.4f, 3f, 0.3f, 1.5f);
            _lunge = new DogLunge(_config);
        }

        [TearDown]
        public void Destroy()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void New_Chases_WithoutVelocity()
        {
            Assert.AreEqual(DogState.Chase, _lunge.State);
            Assert.IsFalse(_lunge.IsStill);
            Assert.AreEqual(Vector2.zero, _lunge.Velocity);
        }

        [Test]
        public void PlayerOutOfRange_KeepsChasing()
        {
            Assert.AreEqual(DogState.Chase, _lunge.Tick(Step, Vector2.zero, new Vector2(3.01f, 0f)));
        }

        [Test]
        public void PlayerExactlyAtTriggerDistance_Crouches()
        {
            Assert.AreEqual(DogState.Crouch, _lunge.Tick(Step, Vector2.zero, new Vector2(0f, 3f)));
            Assert.IsTrue(_lunge.IsStill);
            Assert.AreEqual(Vector2.zero, _lunge.Velocity, "the dog stands still while it crouches");
        }

        [Test]
        public void Crouch_RaisesCrouchedOnce_LastsFourTenths()
        {
            int crouched = 0;
            int lunged = 0;
            _lunge.Crouched += () => crouched++;
            _lunge.Lunged += () => lunged++;
            var player = new Vector2(2f, 0f);

            _lunge.Tick(0f, Vector2.zero, player);
            _lunge.Tick(0.39f, Vector2.zero, player);
            Assert.AreEqual(DogState.Crouch, _lunge.State);
            Assert.AreEqual(1, crouched);
            Assert.AreEqual(0, lunged);

            _lunge.Tick(0.02f, Vector2.zero, player);
            Assert.AreEqual(DogState.Lunge, _lunge.State);
            Assert.AreEqual(1, crouched);
            Assert.AreEqual(1, lunged);
        }

        [Test]
        public void Lunge_FliesStraightAtLungeSpeed()
        {
            Crouch(Vector2.zero, new Vector2(0f, 2f));
            _lunge.Tick(0.4f, Vector2.zero, new Vector2(0f, 2f));

            Assert.AreEqual(DogState.Lunge, _lunge.State);
            Assert.AreEqual(0f, _lunge.Velocity.x, 1e-5f);
            Assert.AreEqual(10f, _lunge.Velocity.y, 1e-4f, "3 units in 0.3 s");
        }

        [Test]
        public void PlayerMovesDuringCrouch_LungeGoesToTheOldPosition()
        {
            var start = new Vector2(2f, 1f);
            Crouch(Vector2.zero, start);
            Vector2 player = start;
            for (int i = 0; i < 19; i++)
            {
                player += new Vector2(0f, 6f) * Step; // sideways dodge, 6 units per second
                _lunge.Tick(Step, Vector2.zero, player);
                Assert.AreEqual(DogState.Crouch, _lunge.State);
            }

            player += new Vector2(0f, 6f) * Step;
            _lunge.Tick(0.4f - _lunge.StateTime, Vector2.zero, player);
            Assert.AreEqual(DogState.Lunge, _lunge.State);
            Assert.AreEqual(start, _lunge.Target);
            Assert.AreEqual(start.normalized.x, _lunge.Direction.x, 1e-5f);
            Assert.AreEqual(start.normalized.y, _lunge.Direction.y, 1e-5f);

            Vector2 dog = Fly(Vector2.zero, player);
            Assert.AreEqual(start.x, dog.x, 0.05f, "lands on the old position, not where the player went");
            Assert.AreEqual(start.y, dog.y, 0.05f);
            Assert.Greater((player - dog).magnitude, 1f, "the dodging player is missed");
        }

        [Test]
        public void Lunge_StopsAtTheOldPosition_NotAfterTheFullDistance()
        {
            Crouch(Vector2.zero, new Vector2(1.5f, 0f));
            _lunge.Tick(0.4f, Vector2.zero, new Vector2(1.5f, 0f));

            Vector2 dog = Fly(Vector2.zero, new Vector2(1.5f, 0f));

            Assert.AreEqual(1.5f, dog.x, 0.05f);
            Assert.AreEqual(DogState.Recovery, _lunge.State);
        }

        [Test]
        public void Lunge_ThenRecoveryStandingStill_ThenChaseAgain()
        {
            var player = new Vector2(3f, 0f);
            Crouch(Vector2.zero, player);
            _lunge.Tick(0.4f, Vector2.zero, player);
            _lunge.Tick(0.31f, new Vector2(3f, 0f), player);

            Assert.AreEqual(DogState.Recovery, _lunge.State);
            Assert.IsTrue(_lunge.IsStill);
            Assert.AreEqual(Vector2.zero, _lunge.Velocity);

            _lunge.Tick(1.48f, new Vector2(3f, 0f), player);
            Assert.AreEqual(DogState.Recovery, _lunge.State);

            _lunge.Tick(0.02f, new Vector2(3f, 0f), player);
            Assert.AreEqual(DogState.Chase, _lunge.State, "no new crouch in the same tick");

            _lunge.Tick(Step, new Vector2(3f, 0f), player);
            Assert.AreEqual(DogState.Crouch, _lunge.State, "the player is still in range");
        }

        [Test]
        public void LongTick_CarriesTimeIntoTheNextState()
        {
            Crouch(Vector2.zero, new Vector2(3f, 0f));

            _lunge.Tick(0.5f, Vector2.zero, new Vector2(3f, 0f));

            Assert.AreEqual(DogState.Lunge, _lunge.State);
            Assert.AreEqual(0.1f, _lunge.StateTime, 1e-4f);
        }

        [Test]
        public void WallInTheMiddleOfTheLunge_EndsItEarly()
        {
            var player = new Vector2(3f, 0f);
            Crouch(Vector2.zero, player);
            _lunge.Tick(0.4f, Vector2.zero, player);
            _lunge.Tick(0.1f, new Vector2(1f, 0f), player);
            Assert.AreEqual(DogState.Lunge, _lunge.State);

            _lunge.NotifyBlocked();

            Assert.AreEqual(DogState.Recovery, _lunge.State);
            Assert.AreEqual(Vector2.zero, _lunge.Velocity);
            Assert.AreEqual(0f, _lunge.StateTime, "the full recovery follows");
        }

        [Test]
        public void Wall_OutsideTheLunge_ChangesNothing()
        {
            _lunge.NotifyBlocked();
            Assert.AreEqual(DogState.Chase, _lunge.State);

            Crouch(Vector2.zero, new Vector2(1f, 0f));
            _lunge.NotifyBlocked();
            Assert.AreEqual(DogState.Crouch, _lunge.State);
        }

        [Test]
        public void HitDuringTheLunge_OneHeart_EndsTheLunge()
        {
            var player = new Vector2(3f, 0f);
            Crouch(Vector2.zero, player);
            _lunge.Tick(0.4f, Vector2.zero, player);

            Assert.AreEqual(1, _lunge.OnPlayerContact(1, false));
            Assert.AreEqual(DogState.Recovery, _lunge.State);
        }

        [Test]
        public void HitDuringInvulnerability_NoDamage_LungeFliesOn()
        {
            var player = new Vector2(3f, 0f);
            Crouch(Vector2.zero, player);
            _lunge.Tick(0.4f, Vector2.zero, player);

            Assert.AreEqual(0, _lunge.OnPlayerContact(1, true), "after a hit or during the dash");
            Assert.AreEqual(DogState.Lunge, _lunge.State);
        }

        [Test]
        public void ContactWhileChasing_NormalContactDamage()
        {
            Assert.AreEqual(1, _lunge.OnPlayerContact(1, false));
            Assert.AreEqual(0, _lunge.OnPlayerContact(1, true));
            Assert.AreEqual(DogState.Chase, _lunge.State);
        }

        [Test]
        public void PlayerOnTheDogsSpot_NoLunge_RecoversRightAway()
        {
            Crouch(Vector2.zero, Vector2.zero);

            _lunge.Tick(0.4f, Vector2.zero, Vector2.zero);

            Assert.AreEqual(DogState.Recovery, _lunge.State);
        }

        [Test]
        public void Reset_BackToChase()
        {
            Crouch(Vector2.zero, new Vector2(1f, 0f));

            _lunge.Reset();

            Assert.AreEqual(DogState.Chase, _lunge.State);
            Assert.AreEqual(0f, _lunge.StateTime);
        }

        private void Crouch(Vector2 dog, Vector2 player)
        {
            Assert.AreEqual(DogState.Crouch, _lunge.Tick(0f, dog, player));
        }

        // Moves the dog with the lunge velocity in small steps until the lunge ends; returns where it landed.
        private Vector2 Fly(Vector2 dog, Vector2 player)
        {
            const float fine = 0.001f;
            for (int i = 0; i < 2000 && _lunge.State == DogState.Lunge; i++)
            {
                dog += _lunge.Velocity * fine;
                _lunge.Tick(fine, dog, player);
            }

            return dog;
        }
    }
}
