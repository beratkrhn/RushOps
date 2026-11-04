using RushOps.Save;
using UnityEngine;
using UnityEngine.Audio;

namespace RushOps.Audio
{
    /// <summary>
    /// Sets the volumes of the mixer channels (issue #108, F-AUD-04): when the scene starts the player's saved
    /// settings (issue #126), or the start volumes of <see cref="AudioConfig"/> until the player changed them; later
    /// the settings screen through <see cref="SetVolume"/>.
    /// </summary>
    public class AudioVolumes : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioConfig _config;
        [Tooltip("Save game with the player's settings; empty uses the start volumes.")]
        [SerializeField] private SaveGameService _save;

        private readonly float[] _volumes = { 1f, 1f, 1f };

        /// <summary>Current volume of <paramref name="channel"/>, 0..1.</summary>
        public float GetVolume(AudioChannel channel)
        {
            return _volumes[(int)channel];
        }

        /// <summary>Sets <paramref name="channel"/> to <paramref name="volume"/> (0 mutes, 1 is full).</summary>
        public void SetVolume(AudioChannel channel, float volume)
        {
            _volumes[(int)channel] = Mathf.Clamp01(volume);
            if (_mixer != null)
            {
                _mixer.SetFloat(AudioVolumeMath.ParameterName(channel), AudioVolumeMath.ToDecibels(volume));
            }
        }

        // The mixer ignores SetFloat before Start.
        private void Start()
        {
            SettingsDefaults defaults = SettingsRules.DefaultsFrom(_config, null);
            SettingsData settings = _save != null ? _save.Data.Settings : null;
            for (int i = 0; i < _volumes.Length; i++)
            {
                var channel = (AudioChannel)i;
                SetVolume(channel, SettingsRules.GetVolume(settings, channel, defaults));
            }
        }
    }
}
