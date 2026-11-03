using NUnit.Framework;
using RushOps.Perks;

namespace RushOps.Tests.EditMode
{
    public class PickupLifetimeTests
    {
        [Test]
        public void ExpiresAfterLifetime_Once()
        {
            var lifetime = new PickupLifetime();
            lifetime.Reset(10f, 3f);

            Assert.IsFalse(lifetime.Tick(9.9f));
            Assert.IsFalse(lifetime.IsExpired);
            Assert.IsTrue(lifetime.Tick(0.1f), "expires on the step that reaches 10 s");
            Assert.IsTrue(lifetime.IsExpired);
            Assert.IsFalse(lifetime.Tick(1f), "reports the expiry only once");
            Assert.AreEqual(0f, lifetime.Remaining);
        }

        [Test]
        public void BlinksOnlyInLastThreeSeconds()
        {
            var lifetime = new PickupLifetime();
            lifetime.Reset(10f, 3f);

            lifetime.Tick(6.9f);
            Assert.IsFalse(lifetime.IsBlinking);
            Assert.IsTrue(lifetime.IsVisible(0.15f));

            lifetime.Tick(0.11f);
            Assert.IsTrue(lifetime.IsBlinking, "blinks from 7 s on");
        }

        [Test]
        public void Blinking_AlternatesStartingHidden()
        {
            var lifetime = new PickupLifetime();
            lifetime.Reset(10f, 3f);

            lifetime.Tick(7.05f);
            Assert.IsFalse(lifetime.IsVisible(0.15f), "first blink phase is hidden");
            lifetime.Tick(0.15f);
            Assert.IsTrue(lifetime.IsVisible(0.15f));
            lifetime.Tick(0.15f);
            Assert.IsFalse(lifetime.IsVisible(0.15f));
        }

        [Test]
        public void Reset_StartsANewLife()
        {
            var lifetime = new PickupLifetime();
            lifetime.Reset(10f, 3f);
            lifetime.Tick(12f);

            lifetime.Reset(10f, 3f);

            Assert.IsFalse(lifetime.IsExpired);
            Assert.AreEqual(0f, lifetime.Elapsed);
            Assert.AreEqual(10f, lifetime.Remaining);
            Assert.IsTrue(lifetime.IsVisible(0.15f));
        }

        [Test]
        public void ExpiredPickup_IsHidden()
        {
            var lifetime = new PickupLifetime();
            lifetime.Reset(1f, 0f);
            lifetime.Tick(1f);

            Assert.IsFalse(lifetime.IsVisible(0.15f));
        }

        [Test]
        public void Bob_StaysBetweenRestAndHeight()
        {
            Assert.AreEqual(0f, PickupMath.Bob(0f, 0.2f, 1f), 1e-5f);
            Assert.AreEqual(0.2f, PickupMath.Bob(0.5f, 0.2f, 1f), 1e-5f);
            Assert.AreEqual(0f, PickupMath.Bob(1f, 0.2f, 1f), 1e-5f);
            Assert.AreEqual(0f, PickupMath.Bob(0.3f, 0.2f, 0f), "no period, no floating");
        }

        [Test]
        public void Pulse_StartsBrightAndDipsToMin()
        {
            Assert.AreEqual(1f, PickupMath.Pulse(0f, 0.4f, 1f, 0.8f), 1e-5f);
            Assert.AreEqual(0.4f, PickupMath.Pulse(0.4f, 0.4f, 1f, 0.8f), 1e-5f);
        }
    }
}
