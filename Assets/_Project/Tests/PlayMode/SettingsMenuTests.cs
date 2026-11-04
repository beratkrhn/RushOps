using System.Collections;
using NUnit.Framework;
using RushOps.Audio;
using RushOps.Player;
using RushOps.Save;
using RushOps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RushOps.Tests.PlayMode
{
    public class SettingsMenuTests
    {
        private SettingsMenu _settings;
        private SaveGameService _save;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            _save = SaveGameIsolation.Reset();
            yield return SceneManager.LoadSceneAsync(MainMenu.SceneName, LoadSceneMode.Single);
            yield return null;
            _settings = Object.FindFirstObjectByType<SettingsMenu>();
            Assert.IsNotNull(_settings, "MainMenu has no SettingsMenu");
        }

        // Other tests expect the start volumes.
        [TearDown]
        public void ForgetSettings()
        {
            SaveGameIsolation.Reset();
        }

        private static T Find<T>(string name) where T : Component
        {
            GameObject go = GameObject.Find(name);
            Assert.IsNotNull(go, $"no {name}");
            return go.GetComponent<T>();
        }

        private static SaveData OnDisk()
        {
            return new SaveGameStore(SaveGameIsolation.Files, "playmode-tests").Load();
        }

        [Test]
        public void SettingsButton_OpensTheScreenWithoutStickSettings()
        {
            Button button = Find<Button>("SettingsButton");
            Assert.IsTrue(button.interactable);

            button.onClick.Invoke();

            Assert.IsTrue(_settings.IsOpen);
            Assert.IsNotNull(GameObject.Find("MasterSlider"));
            Assert.IsNotNull(GameObject.Find("MusicSlider"));
            Assert.IsNotNull(GameObject.Find("EffectsSlider"));
            Assert.AreEqual("Zielhilfe: An", Find<Button>("AimAssistButton").GetComponentInChildren<Text>().text);
            foreach (Text text in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
            {
                StringAssert.DoesNotContain("Stick", text.text);
            }
        }

        [Test]
        public void MovingASlider_ChangesTheVolumeAtOnce_AndClosingSaves()
        {
            _settings.Open();
            var volumes = Object.FindFirstObjectByType<AudioVolumes>();

            Find<Slider>("EffectsSlider").value = 0.25f;

            Assert.AreEqual(0.25f, volumes.GetVolume(AudioChannel.Effects), 1e-4f);
            Assert.AreEqual(0.25f, _save.Data.Settings.EffectsVolume, 1e-4f);
            Assert.IsFalse(OnDisk().Settings.Customized, "saved before the screen closed");

            _settings.Close();

            Assert.IsFalse(_settings.IsOpen);
            Assert.AreEqual(0.25f, OnDisk().Settings.EffectsVolume, 1e-4f);
        }

        [Test]
        public void ReopeningTheScreen_ShowsTheSavedValues()
        {
            _settings.Open();
            Find<Slider>("MusicSlider").value = 0.4f;
            _settings.Close();

            _settings.Open();

            Assert.AreEqual(0.4f, Find<Slider>("MusicSlider").value, 1e-4f);
        }

        [Test]
        public void AimAssistButton_SwitchesAndSaves()
        {
            _settings.Open();

            Find<Button>("AimAssistButton").onClick.Invoke();

            Assert.AreEqual("Zielhilfe: Aus", Find<Button>("AimAssistButton").GetComponentInChildren<Text>().text);
            Assert.IsFalse(OnDisk().Settings.AimAssist);
        }

        [Test]
        public void Credits_ShowTheLinesAndGoBack()
        {
            _settings.Open();
            Find<Button>("CreditsButton").onClick.Invoke();

            Assert.IsTrue(_settings.ShowsCredits);
            StringAssert.Contains("RushOps", Find<Text>("Lines").text);

            _settings.CloseCredits();
            Assert.IsFalse(_settings.ShowsCredits);
            Assert.IsTrue(_settings.IsOpen);
        }

        [UnityTest]
        public IEnumerator SavedSettings_ApplyInTheLevel()
        {
            _settings.Open();
            Find<Slider>("MasterSlider").value = 0.5f;
            Find<Button>("AimAssistButton").onClick.Invoke();
            _settings.Close();

            yield return SceneManager.LoadSceneAsync("Arena_Zonen", LoadSceneMode.Single);
            yield return null;

            Assert.IsFalse(Object.FindFirstObjectByType<AimAssist>().Enabled, "aim assist still on in the level");
            Assert.AreEqual(0.5f, Object.FindFirstObjectByType<AudioVolumes>().GetVolume(AudioChannel.Master), 1e-4f);
        }
    }
}
