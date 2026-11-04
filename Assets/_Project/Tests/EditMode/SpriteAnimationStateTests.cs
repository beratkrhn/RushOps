using NUnit.Framework;
using RushOps.Characters;

namespace RushOps.Tests.EditMode
{
    public class SpriteAnimationStateTests
    {
        [Test]
        public void New_PlaysLoop()
        {
            var state = new SpriteAnimationState();

            Assert.AreEqual(SpriteAnimationMode.Loop, state.Mode);
            Assert.AreEqual(0f, state.Time);
        }

        [Test]
        public void Hit_ReturnsToLoopAfterItsDuration()
        {
            var state = new SpriteAnimationState();
            Assert.IsTrue(state.PlayHit());

            state.Tick(0.1f, 0.2f);
            Assert.AreEqual(SpriteAnimationMode.Hit, state.Mode);

            state.Tick(0.15f, 0.2f);
            Assert.AreEqual(SpriteAnimationMode.Loop, state.Mode);
            Assert.AreEqual(0.05f, state.Time, 1e-5f);
        }

        [Test]
        public void Dash_ReturnsToLoopAfterItsDuration()
        {
            var state = new SpriteAnimationState();
            Assert.IsTrue(state.PlayDash(0.2f));

            state.Tick(0.1f, 0.5f);
            Assert.AreEqual(SpriteAnimationMode.Dash, state.Mode);

            state.Tick(0.15f, 0.5f);
            Assert.AreEqual(SpriteAnimationMode.Loop, state.Mode);
            Assert.AreEqual(0.05f, state.Time, 1e-5f);
        }

        [Test]
        public void Dash_IsNotInterruptedByAHit_ButByDeath()
        {
            var state = new SpriteAnimationState();
            state.PlayDash(0.2f);

            Assert.IsFalse(state.PlayHit());
            Assert.AreEqual(SpriteAnimationMode.Dash, state.Mode);

            state.PlayDeath();
            Assert.AreEqual(SpriteAnimationMode.Death, state.Mode);
            Assert.IsFalse(state.PlayDash(0.2f));
            Assert.AreEqual(SpriteAnimationMode.Death, state.Mode);
        }

        [Test]
        public void Hit_WhileHit_Restarts()
        {
            var state = new SpriteAnimationState();
            state.PlayHit();
            state.Tick(0.15f, 0.2f);

            Assert.IsTrue(state.PlayHit());

            Assert.AreEqual(0f, state.Time);
            Assert.AreEqual(SpriteAnimationMode.Hit, state.Mode);
        }

        [Test]
        public void Death_IgnoresHitsAndHolds()
        {
            var state = new SpriteAnimationState();
            state.PlayDeath();

            Assert.IsFalse(state.PlayHit());
            state.Tick(5f, 0.2f);

            Assert.AreEqual(SpriteAnimationMode.Death, state.Mode);
            Assert.AreEqual(5f, state.Time, 1e-5f);
        }

        [Test]
        public void PlayLoop_AfterDeath_Restarts()
        {
            var state = new SpriteAnimationState();
            state.PlayDeath();
            state.Tick(1f, 0.2f);

            state.PlayLoop();

            Assert.AreEqual(SpriteAnimationMode.Loop, state.Mode);
            Assert.AreEqual(0f, state.Time);
        }
    
        [Test]
        public void Alert_SwitchesLoopAndRestartsIt()
        {
            var state = new SpriteAnimationState();
            state.Tick(0.3f, 0.2f);

            state.SetAlert(true);

            Assert.IsTrue(state.IsAlert);
            Assert.AreEqual(SpriteAnimationMode.Loop, state.Mode);
            Assert.AreEqual(0f, state.Time);
        }

        [Test]
        public void Alert_DuringDeath_KeepsDeathRunning()
        {
            var state = new SpriteAnimationState();
            state.PlayDeath();
            state.Tick(0.3f, 0.2f);

            state.SetAlert(true);

            Assert.AreEqual(SpriteAnimationMode.Death, state.Mode);
            Assert.AreEqual(0.3f, state.Time, 1e-5f);
        }

        [Test]
        public void PlayLoop_ClearsAlert()
        {
            var state = new SpriteAnimationState();
            state.SetAlert(true);

            state.PlayLoop();

            Assert.IsFalse(state.IsAlert);
        }
    }
}
