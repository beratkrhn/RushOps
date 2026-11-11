using NUnit.Framework;
using RushOps.Feel;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Rules and setting of the vibration (issue #300, F-AUD-05, N32).</summary>
    public class HapticRulesTests
    {
        [Test]
        public void FirstPulse_GoesOut()
        {
            var rules = new HapticRules(0.1f);

            Assert.IsTrue(rules.Request(HapticKind.Explosion, 5f));
        }

        [Test]
        public void ChainOfExplosions_KeepsTheGap()
        {
            var rules = new HapticRules(0.1f);

            Assert.IsTrue(rules.Request(HapticKind.Explosion, 1f));
            Assert.IsFalse(rules.Request(HapticKind.Explosion, 1.05f), "inside the gap");
            Assert.IsFalse(rules.Request(HapticKind.Explosion, 1.09f), "still inside the gap of the first pulse");
            Assert.IsTrue(rules.Request(HapticKind.Explosion, 1.1f), "exactly the gap");
        }

        [Test]
        public void Damage_GetsThroughRightAfterAnExplosion_ButNotTheOtherWayRound()
        {
            var rules = new HapticRules(0.1f);

            Assert.IsTrue(rules.Request(HapticKind.Explosion, 1f));
            Assert.IsTrue(rules.Request(HapticKind.Damage, 1.01f), "damage wins over an explosion");
            Assert.IsFalse(rules.Request(HapticKind.Explosion, 1.02f));
            Assert.IsFalse(rules.Request(HapticKind.Damage, 1.03f), "two damage pulses keep the gap");
        }

        [Test]
        public void ZeroGap_LetsEveryPulseThrough()
        {
            var rules = new HapticRules(0f);

            Assert.IsTrue(rules.Request(HapticKind.Explosion, 1f));
            Assert.IsTrue(rules.Request(HapticKind.Explosion, 1f));
        }

        [Test]
        public void Vibration_IsOnWithoutSettings_AndCanBeSwitchedOff()
        {
            Assert.IsTrue(SettingsRules.GetVibration(null));
            var settings = new SettingsData();
            Assert.IsTrue(SettingsRules.GetVibration(settings));

            Assert.IsTrue(SettingsRules.SetVibration(settings, false));
            Assert.IsFalse(SettingsRules.GetVibration(settings));
            Assert.IsFalse(SettingsRules.SetVibration(settings, false), "no change");
        }

        [Test]
        public void Vibration_IsIndependentOfCustomizedVolumes()
        {
            var settings = new SettingsData();
            SettingsRules.SetVibration(settings, false);

            Assert.IsFalse(settings.Customized, "switching vibration does not freeze the start volumes");
            Assert.IsFalse(SettingsRules.GetVibration(settings));
        }

        [Test]
        public void OldSaveWithoutTheField_ReadsVibrationAsOn()
        {
            var settings = JsonUtility.FromJson<SettingsData>(
                "{\"Customized\":true,\"MasterVolume\":0.5,\"MusicVolume\":0.4,\"EffectsVolume\":0.3,\"AimAssist\":false}");

            Assert.IsTrue(SettingsRules.GetVibration(settings));
            Assert.AreEqual(0.4f, settings.MusicVolume, 1e-5f);
        }
    }
}
