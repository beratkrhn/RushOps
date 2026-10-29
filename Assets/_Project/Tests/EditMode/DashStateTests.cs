using NUnit.Framework;
using RushOps.Player;

namespace RushOps.Tests.EditMode
{
    public class DashStateTests
    {
        private const float Duration = 0.2f;
        private const float IFrames = 0.3f;
        private const float Cooldown = 1f;
        private const float Step = 0.02f;
        // Whole steps of 0.02 s: i-frames 0.3 s, dash 0.2 s, cooldown 1.0 s.
        private const int IFrameSteps = 15;
        private const int DurationSteps = 10;
        private const int CooldownSteps = 50;

        [Test]
        public void TryStart_Ready_StartsDashWithIFrames()
        {
            var state = new DashState();

            Assert.IsTrue(state.TryStart(Duration, IFrames, Cooldown));
            Assert.IsTrue(state.IsDashing);
            Assert.IsTrue(state.IsInvulnerable);
            Assert.IsFalse(state.CanDash);
        }

        [Test]
        public void Tick_DashEndsAfterDuration_WholeSteps()
        {
            var state = new DashState();
            state.TryStart(Duration, IFrames, Cooldown);

            // 0.2 s at 0.02 s per step: dashing for exactly 10 steps despite float rounding.
            Assert.AreEqual(DurationSteps, StepsWhile(state, () => state.IsDashing));
        }

        [Test]
        public void Tick_IFrameWindow_LastsConfiguredTime()
        {
            var state = new DashState();
            state.TryStart(Duration, IFrames, Cooldown);

            Tick(state, IFrameSteps - 1);
            Assert.IsTrue(state.IsInvulnerable, "still invulnerable one step before the window ends");
            Assert.IsFalse(state.IsDashing, "the dash itself is over");

            Tick(state, 1);
            Assert.IsFalse(state.IsInvulnerable, "vulnerable again after the window");
        }

        [Test]
        public void TryStart_IFramesShorterThanDash_InvulnerableForWholeDash()
        {
            var state = new DashState();
            state.TryStart(Duration, 0.05f, Cooldown);

            Tick(state, DurationSteps - 1);

            Assert.IsTrue(state.IsDashing);
            Assert.IsTrue(state.IsInvulnerable);
        }

        [Test]
        public void TryStart_DuringCooldown_Fails()
        {
            var state = new DashState();
            state.TryStart(Duration, IFrames, Cooldown);

            Tick(state, CooldownSteps - 2);

            Assert.IsFalse(state.TryStart(Duration, IFrames, Cooldown));
            Assert.Greater(state.CooldownRemaining, 0f);
        }

        [Test]
        public void TryStart_AfterCooldown_Succeeds()
        {
            var state = new DashState();
            state.TryStart(Duration, IFrames, Cooldown);

            Tick(state, CooldownSteps);

            Assert.AreEqual(0f, state.CooldownRemaining);
            Assert.IsTrue(state.TryStart(Duration, IFrames, Cooldown));
        }

        [Test]
        public void CooldownProgress_RisesFromZeroToOne()
        {
            var state = new DashState();
            Assert.AreEqual(1f, state.CooldownProgress, "ready before the first dash");

            state.TryStart(Duration, IFrames, Cooldown);
            Assert.AreEqual(0f, state.CooldownProgress, 1e-5f);

            Tick(state, CooldownSteps / 2);
            Assert.AreEqual(0.5f, state.CooldownProgress, 1e-3f);

            Tick(state, CooldownSteps);
            Assert.AreEqual(1f, state.CooldownProgress);
        }

        [Test]
        public void Lock_PreventsDash_UntilAllSourcesUnlock()
        {
            var state = new DashState();
            object carry = new object();
            object cutscene = new object();

            state.Lock(carry);
            state.Lock(cutscene);
            Assert.IsTrue(state.IsLocked);
            Assert.IsFalse(state.TryStart(Duration, IFrames, Cooldown));

            state.Unlock(carry);
            Assert.IsFalse(state.TryStart(Duration, IFrames, Cooldown), "one source still locks");

            state.Unlock(cutscene);
            Assert.IsFalse(state.IsLocked);
            Assert.IsTrue(state.TryStart(Duration, IFrames, Cooldown));
        }

        [Test]
        public void Lock_DuringDash_DashContinues()
        {
            var state = new DashState();
            state.TryStart(Duration, IFrames, Cooldown);

            state.Lock(this);

            Assert.IsTrue(state.IsDashing);
            Assert.IsTrue(state.IsInvulnerable);
        }

        [Test]
        public void Reset_EndsDashIFramesAndCooldown()
        {
            var state = new DashState();
            state.TryStart(Duration, IFrames, Cooldown);

            state.Reset();

            Assert.IsFalse(state.IsDashing);
            Assert.IsFalse(state.IsInvulnerable);
            Assert.IsTrue(state.CanDash);
        }

        private static void Tick(DashState state, int steps)
        {
            for (int i = 0; i < steps; i++)
            {
                state.Tick(Step);
            }
        }

        private static int StepsWhile(DashState state, System.Func<bool> condition)
        {
            int steps = 0;
            while (condition() && steps < 1000)
            {
                state.Tick(Step);
                steps++;
            }

            return steps;
        }
    }
}
