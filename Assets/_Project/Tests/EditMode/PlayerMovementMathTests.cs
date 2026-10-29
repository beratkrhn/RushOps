using NUnit.Framework;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class PlayerMovementMathTests
    {
        private const float Speed = 5f;
        private const float Acceleration = 40f;
        private const float Deceleration = 50f;
        private const float Step = 0.02f;

        [Test]
        public void TargetVelocity_FullDeflection_ReachesMoveSpeed()
        {
            Vector2 result = PlayerMovementMath.TargetVelocity(Vector2.right, Speed);

            Assert.AreEqual(new Vector2(Speed, 0f), result);
        }

        [Test]
        public void TargetVelocity_HalfDeflection_HalfSpeed()
        {
            Vector2 result = PlayerMovementMath.TargetVelocity(new Vector2(0f, 0.5f), Speed);

            Assert.AreEqual(Speed * 0.5f, result.y, 1e-5f);
        }

        [Test]
        public void TargetVelocity_DiagonalKeys_NotFasterThanMoveSpeed()
        {
            Vector2 result = PlayerMovementMath.TargetVelocity(new Vector2(1f, 1f), Speed);

            Assert.AreEqual(Speed, result.magnitude, 1e-5f);
        }

        [Test]
        public void StepVelocity_FromRest_AcceleratesByOneStep()
        {
            Vector2 result = PlayerMovementMath.StepVelocity(Vector2.zero, new Vector2(Speed, 0f), Acceleration, Deceleration, Step);

            Assert.AreEqual(Acceleration * Step, result.x, 1e-5f);
        }

        [Test]
        public void StepVelocity_NearTarget_DoesNotOvershoot()
        {
            var target = new Vector2(Speed, 0f);

            Vector2 result = PlayerMovementMath.StepVelocity(new Vector2(4.9f, 0f), target, Acceleration, Deceleration, Step);

            Assert.AreEqual(target, result);
        }

        [Test]
        public void StepVelocity_InputReleased_SlowsDownWithDeceleration()
        {
            Vector2 result = PlayerMovementMath.StepVelocity(new Vector2(Speed, 0f), Vector2.zero, Acceleration, Deceleration, Step);

            Assert.AreEqual(Speed - Deceleration * Step, result.x, 1e-5f);
        }

        [Test]
        public void StepVelocity_RepeatedSteps_ReachesTopSpeedWithinShortTime()
        {
            var target = new Vector2(0f, Speed);
            Vector2 velocity = Vector2.zero;
            float time = 0f;
            while (velocity != target && time < 1f)
            {
                velocity = PlayerMovementMath.StepVelocity(velocity, target, Acceleration, Deceleration, Step);
                time += Step;
            }

            // Speed / Acceleration = 0.125 s, rounded up to whole steps.
            Assert.AreEqual(target, velocity);
            Assert.LessOrEqual(time, Speed / Acceleration + Step);
        }
    }
}
