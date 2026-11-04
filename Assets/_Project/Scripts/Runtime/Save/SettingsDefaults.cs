namespace RushOps.Save
{
    /// <summary>Start values of the settings, taken from the audio and weapon configs.</summary>
    public readonly struct SettingsDefaults
    {
        public SettingsDefaults(float master, float music, float effects, bool aimAssist)
        {
            Master = master;
            Music = music;
            Effects = effects;
            AimAssist = aimAssist;
        }

        /// <summary>Start volume of everything, 0..1.</summary>
        public float Master { get; }

        /// <summary>Start volume of the music, 0..1.</summary>
        public float Music { get; }

        /// <summary>Start volume of the effects, 0..1.</summary>
        public float Effects { get; }

        /// <summary>Aim assist on at the start.</summary>
        public bool AimAssist { get; }
    }
}
