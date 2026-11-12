using RushOps.Audio;
using RushOps.Combat;
using RushOps.Save;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Settings screen of the main menu (issue #126, F-UI-04, F-AUD-04): sliders for the three volumes, aim assist on
    /// and off, vibration on and off (issue #300), „Kampagne zurücksetzen“ behind a confirmation (issue #328, F-UI-09),
    /// and the credits. Volume changes are audible at once; the settings are saved when the screen closes,
    /// when aim assist is switched and when the app goes to the background (F-DAT-03). Thin adapter; the rules live
    /// in <see cref="SettingsRules"/>. Buttons and sliders call the public methods.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private SaveGameService _save;
        [SerializeField] private AudioVolumes _volumes;
        [SerializeField] private AudioConfig _audioConfig;
        [SerializeField] private WeaponConfig _weaponConfig;
        [SerializeField] private CreditsConfig _credits;

        [Header("Screens")]
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _creditsPanel;
        [SerializeField] private GameObject _resetPanel;

        [Header("Controls")]
        [SerializeField] private Slider _master;
        [SerializeField] private Slider _music;
        [SerializeField] private Slider _effects;
        [SerializeField] private Text _aimAssistLabel;
        [SerializeField] private Text _vibrationLabel;
        [SerializeField] private Text _creditsText;

        private bool _dirty;
        private bool _refreshing;

        /// <summary>True while the settings screen shows.</summary>
        public bool IsOpen => _settingsPanel != null && _settingsPanel.activeSelf;

        /// <summary>True while the reset confirmation shows.</summary>
        public bool AsksToReset => _resetPanel != null && _resetPanel.activeSelf;

        /// <summary>True while the credits show.</summary>
        public bool ShowsCredits => _creditsPanel != null && _creditsPanel.activeSelf;

        /// <summary>Text of the aim assist button, „Zielhilfe: An“ or „Zielhilfe: Aus“.</summary>
        public static string AimAssistText(bool on)
        {
            return on ? "Zielhilfe: An" : "Zielhilfe: Aus";
        }

        /// <summary>Text of the vibration button, „Vibration: An“ or „Vibration: Aus“ (issue #300).</summary>
        public static string VibrationText(bool on)
        {
            return on ? "Vibration: An" : "Vibration: Aus";
        }

        /// <summary>Shows the settings screen with the current values.</summary>
        public void Open()
        {
            Refresh();
            SetActive(_creditsPanel, false);
            SetActive(_resetPanel, false);
            SetActive(_settingsPanel, true);
        }

        /// <summary>Saves and hides the settings screen („Zurück“).</summary>
        public void Close()
        {
            SaveIfChanged();
            SetActive(_creditsPanel, false);
            SetActive(_resetPanel, false);
            SetActive(_settingsPanel, false);
        }

        /// <summary>„Kampagne zurücksetzen“: asks first (issue #328).</summary>
        public void OpenResetConfirm()
        {
            SetActive(_resetPanel, true);
        }

        /// <summary>„Abbrechen“: keeps the campaign.</summary>
        public void CancelReset()
        {
            SetActive(_resetPanel, false);
        }

        /// <summary>„Zurücksetzen“: clears the campaign and saves at once (F-DAT-07, F-DAT-03).</summary>
        public void ConfirmReset()
        {
            SetActive(_resetPanel, false);
            if (_save == null)
            {
                return;
            }

            CampaignReset.Apply(_save.Data);
            _save.Save();
        }

        /// <summary>Shows the credits over the settings.</summary>
        public void OpenCredits()
        {
            SetActive(_creditsPanel, true);
        }

        /// <summary>Back from the credits to the settings.</summary>
        public void CloseCredits()
        {
            SetActive(_creditsPanel, false);
        }

        /// <summary>Slider „Gesamt“.</summary>
        public void SetMasterVolume(float volume)
        {
            ChangeVolume(AudioChannel.Master, volume);
        }

        /// <summary>Slider „Musik“.</summary>
        public void SetMusicVolume(float volume)
        {
            ChangeVolume(AudioChannel.Music, volume);
        }

        /// <summary>Slider „Effekte“.</summary>
        public void SetEffectsVolume(float volume)
        {
            ChangeVolume(AudioChannel.Effects, volume);
        }

        /// <summary>Switches aim assist and saves.</summary>
        public void ToggleAimAssist()
        {
            if (_save == null)
            {
                return;
            }

            SettingsData settings = _save.Data.Settings;
            bool on = !SettingsRules.GetAimAssist(settings, Defaults.AimAssist);
            SettingsRules.SetAimAssist(settings, on, Defaults);
            SetText(_aimAssistLabel, AimAssistText(on));
            _dirty = false;
            _save.Save();
        }

        /// <summary>Switches the vibration and saves (issue #300).</summary>
        public void ToggleVibration()
        {
            if (_save == null)
            {
                return;
            }

            SettingsData settings = _save.Data.Settings;
            bool on = !SettingsRules.GetVibration(settings);
            SettingsRules.SetVibration(settings, on);
            SetText(_vibrationLabel, VibrationText(on));
            _dirty = false;
            _save.Save();
        }

        private SettingsDefaults Defaults => SettingsRules.DefaultsFrom(_audioConfig, _weaponConfig);

        private void Awake()
        {
            SetActive(_settingsPanel, false);
            SetActive(_creditsPanel, false);
            SetActive(_resetPanel, false);
            if (_creditsText != null && _credits != null)
            {
                _creditsText.text = string.Join("\n", _credits.Lines);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveIfChanged();
            }
        }

        private void OnDestroy()
        {
            SaveIfChanged();
        }

        private void ChangeVolume(AudioChannel channel, float volume)
        {
            if (_refreshing || _save == null)
            {
                return;
            }

            if (SettingsRules.SetVolume(_save.Data.Settings, channel, volume, Defaults))
            {
                _dirty = true;
            }

            if (_volumes != null)
            {
                _volumes.SetVolume(channel, SettingsRules.GetVolume(_save.Data.Settings, channel, Defaults));
            }
        }

        private void Refresh()
        {
            SettingsData settings = _save != null ? _save.Data.Settings : null;
            _refreshing = true;
            SetSlider(_master, SettingsRules.GetVolume(settings, AudioChannel.Master, Defaults));
            SetSlider(_music, SettingsRules.GetVolume(settings, AudioChannel.Music, Defaults));
            SetSlider(_effects, SettingsRules.GetVolume(settings, AudioChannel.Effects, Defaults));
            _refreshing = false;
            SetText(_aimAssistLabel, AimAssistText(SettingsRules.GetAimAssist(settings, Defaults.AimAssist)));
            SetText(_vibrationLabel, VibrationText(SettingsRules.GetVibration(settings)));
        }

        private void SaveIfChanged()
        {
            if (_dirty && _save != null)
            {
                _dirty = false;
                _save.Save();
            }
        }

        private static void SetSlider(Slider slider, float value)
        {
            if (slider != null)
            {
                slider.SetValueWithoutNotify(value);
            }
        }

        private static void SetText(Text label, string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }
    }
}
