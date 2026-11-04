using System;
using RushOps.Audio;
using RushOps.Combat;

namespace RushOps.Save
{
    /// <summary>
    /// Reads and changes the settings in the save game (issue #126, F-UI-04, F-AUD-04). Uncustomized settings give the
    /// start values; the first change copies all start values in, so the other settings stay as they were.
    /// </summary>
    public static class SettingsRules
    {
        /// <summary>Start values from the configs; missing configs give full volume and aim assist on.</summary>
        public static SettingsDefaults DefaultsFrom(AudioConfig audio, WeaponConfig weapon)
        {
            return new SettingsDefaults(
                audio != null ? audio.StartVolume(AudioChannel.Master) : 1f,
                audio != null ? audio.StartVolume(AudioChannel.Music) : 1f,
                audio != null ? audio.StartVolume(AudioChannel.Effects) : 1f,
                weapon == null || weapon.AimAssistEnabled);
        }

        /// <summary>Volume of <paramref name="channel"/> in effect, 0..1.</summary>
        public static float GetVolume(SettingsData settings, AudioChannel channel, SettingsDefaults defaults)
        {
            if (settings == null || !settings.Customized)
            {
                return Pick(channel, defaults.Master, defaults.Music, defaults.Effects);
            }

            return Clamp01(Pick(channel, settings.MasterVolume, settings.MusicVolume, settings.EffectsVolume));
        }

        /// <summary>Whether aim assist is on.</summary>
        public static bool GetAimAssist(SettingsData settings, bool defaultOn)
        {
            return settings != null && settings.Customized ? settings.AimAssist : defaultOn;
        }

        /// <summary>Sets the volume of <paramref name="channel"/>.</summary>
        /// <returns>True if the setting changed.</returns>
        public static bool SetVolume(SettingsData settings, AudioChannel channel, float volume, SettingsDefaults defaults)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            volume = Clamp01(volume);
            bool wasCustomized = settings.Customized;
            if (wasCustomized && Math.Abs(GetVolume(settings, channel, defaults) - volume) < 1e-4f)
            {
                return false;
            }

            Customize(settings, defaults);
            switch (channel)
            {
                case AudioChannel.Music:
                    settings.MusicVolume = volume;
                    break;
                case AudioChannel.Effects:
                    settings.EffectsVolume = volume;
                    break;
                default:
                    settings.MasterVolume = volume;
                    break;
            }

            return true;
        }

        /// <summary>Turns aim assist on or off.</summary>
        /// <returns>True if the setting changed.</returns>
        public static bool SetAimAssist(SettingsData settings, bool on, SettingsDefaults defaults)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (GetAimAssist(settings, defaults.AimAssist) == on && settings.Customized)
            {
                return false;
            }

            Customize(settings, defaults);
            settings.AimAssist = on;
            return true;
        }

        private static void Customize(SettingsData settings, SettingsDefaults defaults)
        {
            if (settings.Customized)
            {
                return;
            }

            settings.Customized = true;
            settings.MasterVolume = Clamp01(defaults.Master);
            settings.MusicVolume = Clamp01(defaults.Music);
            settings.EffectsVolume = Clamp01(defaults.Effects);
            settings.AimAssist = defaults.AimAssist;
        }

        private static float Pick(AudioChannel channel, float master, float music, float effects)
        {
            switch (channel)
            {
                case AudioChannel.Music:
                    return music;
                case AudioChannel.Effects:
                    return effects;
                default:
                    return master;
            }
        }

        private static float Clamp01(float value)
        {
            return value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
