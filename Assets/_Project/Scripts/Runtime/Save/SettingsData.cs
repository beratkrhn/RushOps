using System;

namespace RushOps.Save
{
    /// <summary>
    /// The player's settings in the save game (F-UI-04, F-DAT-04). Until the player changes something
    /// (<see cref="Customized"/> false) the start values from the configs apply, so tuning them still reaches
    /// players who never opened the settings.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        /// <summary>True once the player changed a setting; only then the values below apply.</summary>
        public bool Customized;

        /// <summary>Volume of everything, 0..1.</summary>
        public float MasterVolume = 1f;

        /// <summary>Volume of the music, 0..1.</summary>
        public float MusicVolume = 1f;

        /// <summary>Volume of the effects, 0..1.</summary>
        public float EffectsVolume = 1f;

        /// <summary>Aim assist on or off.</summary>
        public bool AimAssist = true;
    }
}
