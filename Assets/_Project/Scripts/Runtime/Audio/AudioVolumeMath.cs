using UnityEngine;

namespace RushOps.Audio
{
    /// <summary>Converts the volume sliders (0..1) to mixer attenuation (issue #108).</summary>
    public static class AudioVolumeMath
    {
        /// <summary>Attenuation of a muted channel, the mixer's minimum.</summary>
        public const float MutedDecibels = -80f;

        /// <summary>Name of the exposed mixer parameter of <paramref name="channel"/>.</summary>
        public static string ParameterName(AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Music:
                    return "MusicVolume";
                case AudioChannel.Effects:
                    return "EffectsVolume";
                default:
                    return "MasterVolume";
            }
        }

        /// <summary>
        /// Decibels for a linear volume: 1 is 0 dB, 0.5 about -6 dB, 0 (or less) mutes. Values above 1 are clamped.
        /// </summary>
        public static float ToDecibels(float volume)
        {
            if (volume <= 0.0001f)
            {
                return MutedDecibels;
            }

            return Mathf.Max(MutedDecibels, 20f * Mathf.Log10(Mathf.Min(volume, 1f)));
        }
    }
}
