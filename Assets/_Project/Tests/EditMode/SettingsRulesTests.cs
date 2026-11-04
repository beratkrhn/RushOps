using NUnit.Framework;
using RushOps.Audio;
using RushOps.Save;

namespace RushOps.Tests.EditMode
{
    public class SettingsRulesTests
    {
        private static readonly SettingsDefaults Defaults = new SettingsDefaults(1f, 0.8f, 0.9f, true);

        [Test]
        public void Untouched_GivesTheStartValues()
        {
            var settings = new SettingsData();

            Assert.AreEqual(1f, SettingsRules.GetVolume(settings, AudioChannel.Master, Defaults));
            Assert.AreEqual(0.8f, SettingsRules.GetVolume(settings, AudioChannel.Music, Defaults));
            Assert.AreEqual(0.9f, SettingsRules.GetVolume(settings, AudioChannel.Effects, Defaults));
            Assert.IsTrue(SettingsRules.GetAimAssist(settings, Defaults.AimAssist));
            Assert.AreEqual(0.8f, SettingsRules.GetVolume(null, AudioChannel.Music, Defaults));
        }

        [Test]
        public void FirstChange_KeepsTheOtherStartValues()
        {
            var settings = new SettingsData();

            Assert.IsTrue(SettingsRules.SetVolume(settings, AudioChannel.Effects, 0.3f, Defaults));

            Assert.IsTrue(settings.Customized);
            Assert.AreEqual(0.3f, SettingsRules.GetVolume(settings, AudioChannel.Effects, Defaults), 1e-5f);
            Assert.AreEqual(0.8f, SettingsRules.GetVolume(settings, AudioChannel.Music, Defaults), 1e-5f);
            Assert.AreEqual(1f, SettingsRules.GetVolume(settings, AudioChannel.Master, Defaults), 1e-5f);
            Assert.IsTrue(SettingsRules.GetAimAssist(settings, Defaults.AimAssist));
        }

        [Test]
        public void Customized_IgnoresLaterChangesOfTheStartValues()
        {
            var settings = new SettingsData();
            SettingsRules.SetVolume(settings, AudioChannel.Music, 0.5f, Defaults);
            var tuned = new SettingsDefaults(0.2f, 0.2f, 0.2f, false);

            Assert.AreEqual(0.5f, SettingsRules.GetVolume(settings, AudioChannel.Music, tuned), 1e-5f);
            Assert.AreEqual(1f, SettingsRules.GetVolume(settings, AudioChannel.Master, tuned), 1e-5f);
            Assert.IsTrue(SettingsRules.GetAimAssist(settings, tuned.AimAssist));
        }

        [Test]
        public void SetVolume_ClampsAndReportsNoChangeForTheSameValue()
        {
            var settings = new SettingsData();
            SettingsRules.SetVolume(settings, AudioChannel.Master, 2f, Defaults);

            Assert.AreEqual(1f, settings.MasterVolume);
            Assert.IsFalse(SettingsRules.SetVolume(settings, AudioChannel.Master, 1f, Defaults));
            Assert.IsTrue(SettingsRules.SetVolume(settings, AudioChannel.Master, -1f, Defaults));
            Assert.AreEqual(0f, settings.MasterVolume);
        }

        [Test]
        public void SetAimAssist_SwitchesAndKeepsTheVolumes()
        {
            var settings = new SettingsData();

            Assert.IsTrue(SettingsRules.SetAimAssist(settings, false, Defaults));
            Assert.IsFalse(SettingsRules.GetAimAssist(settings, true));
            Assert.AreEqual(0.8f, settings.MusicVolume, 1e-5f);
            Assert.IsFalse(SettingsRules.SetAimAssist(settings, false, Defaults));
        }

        [Test]
        public void DefaultsFrom_WithoutConfigs_IsFullVolumeWithAimAssist()
        {
            SettingsDefaults defaults = SettingsRules.DefaultsFrom(null, null);

            Assert.AreEqual(1f, defaults.Master);
            Assert.AreEqual(1f, defaults.Music);
            Assert.AreEqual(1f, defaults.Effects);
            Assert.IsTrue(defaults.AimAssist);
        }

        [Test]
        public void Settings_SurviveSaveAndLoad_AndOldSavesGetDefaults()
        {
            var files = new MemorySaveFileSystem();
            var store = new SaveGameStore(files, "saves");
            var data = new SaveData();
            SettingsRules.SetVolume(data.Settings, AudioChannel.Music, 0.25f, Defaults);
            SettingsRules.SetAimAssist(data.Settings, false, Defaults);
            store.Save(data);

            SaveData loaded = new SaveGameStore(files, "saves").Load();
            Assert.IsTrue(loaded.Settings.Customized);
            Assert.AreEqual(0.25f, loaded.Settings.MusicVolume, 1e-5f);
            Assert.IsFalse(loaded.Settings.AimAssist);

            files.Files[store.MainPath] = "{\"Version\": 1, \"Stats\": {\"TotalKills\": 3}}";
            SaveData old = new SaveGameStore(files, "saves").Load();
            Assert.IsNotNull(old.Settings);
            Assert.IsFalse(old.Settings.Customized);
        }
    }
}
