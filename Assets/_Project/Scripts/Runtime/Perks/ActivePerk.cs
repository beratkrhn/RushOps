namespace RushOps.Perks
{
    /// <summary>A running timed perk in <see cref="ActivePerks"/>.</summary>
    public readonly struct ActivePerk
    {
        public ActivePerk(string id, PerkEffect effect, float magnitude, float duration, float remaining)
        {
            Id = id;
            Effect = effect;
            Magnitude = magnitude;
            Duration = duration;
            Remaining = remaining;
        }

        /// <summary>Id of the perk, see <see cref="PerkDefinition.Id"/>.</summary>
        public string Id { get; }

        /// <summary>What the perk does.</summary>
        public PerkEffect Effect { get; }

        /// <summary>Strength of the effect, e.g. 1.5 for a speed multiplier.</summary>
        public float Magnitude { get; }

        /// <summary>Full duration in seconds.</summary>
        public float Duration { get; }

        /// <summary>Seconds left.</summary>
        public float Remaining { get; }

        /// <summary>Remaining share of the duration, 1 right after pickup down to 0; drives the HUD timer.</summary>
        public float Fraction => Duration > 0f ? Remaining / Duration : 0f;

        /// <summary>A copy with <paramref name="remaining"/> seconds left.</summary>
        public ActivePerk WithRemaining(float remaining)
        {
            return new ActivePerk(Id, Effect, Magnitude, Duration, remaining);
        }
    }
}
