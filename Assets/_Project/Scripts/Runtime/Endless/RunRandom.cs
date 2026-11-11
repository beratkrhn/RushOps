using RushOps.Perks;

namespace RushOps.Endless
{
    /// <summary>
    /// Random streams of an endless run (F-END-13, decision N21, ADR 0015): one seed for the whole run, and from it one
    /// independent stream per system (spawns, drops, offers, …). A stream only depends on the seed and its name, so the
    /// upgrade offer after round 3 is the same however many shots or coins came before it.
    /// </summary>
    public static class RunRandom
    {
        /// <summary>Stream of the spawn points.</summary>
        public const string SpawnPoints = "spawn-points";

        /// <summary>Stream of the offsets inside a pack.</summary>
        public const string Packs = "packs";

        /// <summary>Stream of the upgrade offers.</summary>
        public const string Offers = "offers";

        /// <summary>Stream of the perk drops of normal enemies.</summary>
        public const string Perks = "perks";

        /// <summary>Stream of the gold scatter.</summary>
        public const string Gold = "gold";

        /// <summary>Stream of the weapon spread.</summary>
        public const string Spread = "spread";

        /// <summary>Stream of the sound pitch.</summary>
        public const string Sounds = "sounds";

        /// <summary>Prefix of the stream of a boss round, followed by the round number.</summary>
        public const string Boss = "boss-";

        /// <summary>The stream <paramref name="name"/> of the run with <paramref name="seed"/>; the same pair always repeats.</summary>
        public static IRandomSource Stream(int seed, string name)
        {
            return new SystemRandomSource(StreamSeed(seed, name));
        }

        /// <summary>The seed of the stream <paramref name="name"/> of the run with <paramref name="seed"/>.</summary>
        public static int StreamSeed(int seed, string name)
        {
            unchecked
            {
                // Mix the run seed into the FNV hash of the name, then scramble (murmur3 finaliser).
                uint h = (uint)DailySeed.FromKey(name) ^ (uint)seed * 0x9E3779B1u;
                h ^= h >> 16;
                h *= 0x85EBCA6Bu;
                h ^= h >> 13;
                h *= 0xC2B2AE35u;
                h ^= h >> 16;
                return (int)h;
            }
        }
    }
}
