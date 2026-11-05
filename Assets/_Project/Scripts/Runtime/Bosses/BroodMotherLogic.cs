using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// The Brutmutter's moves as pure rules (issue #155, decision C8): she walks at the player and gives birth to a
    /// runner every few seconds, more often below a share of her health. A birth only starts while the level has room
    /// for one more enemy (F-GEG-03); if the room is gone when the birth ends, no runner comes out and she tries again
    /// as soon as there is room. The time from one runner to the next is the spawn interval, birth included.
    /// </summary>
    public class BroodMotherLogic
    {
        private readonly float _spawnInterval;
        private readonly float _enragedSpawnInterval;
        private readonly float _enragedHealthShare;
        private readonly float _birthTime;

        /// <summary>
        /// Creates the rules: a runner every <paramref name="spawnInterval"/> seconds, every
        /// <paramref name="enragedSpawnInterval"/> seconds below <paramref name="enragedHealthShare"/> (0..1) of her
        /// health; each birth takes <paramref name="birthTime"/> seconds, at most the shorter interval.
        /// </summary>
        public BroodMotherLogic(float spawnInterval, float enragedSpawnInterval, float enragedHealthShare, float birthTime)
        {
            _spawnInterval = Mathf.Max(0.01f, spawnInterval);
            _enragedSpawnInterval = Mathf.Max(0.01f, enragedSpawnInterval);
            _enragedHealthShare = Mathf.Clamp01(enragedHealthShare);
            _birthTime = Mathf.Clamp(birthTime, 0f, Mathf.Min(_spawnInterval, _enragedSpawnInterval));
        }

        /// <summary>Current phase.</summary>
        public BroodMotherPhase Phase { get; private set; } = BroodMotherPhase.Walk;

        /// <summary>Seconds in the current phase.</summary>
        public float PhaseTime { get; private set; }

        /// <summary>Seconds since the last runner came out, or since the fight started.</summary>
        public float SinceSpawn { get; private set; }

        /// <summary>Seconds from one runner to the next at <paramref name="healthShare"/> (0..1) of her health.</summary>
        public float Interval(float healthShare)
        {
            return healthShare < _enragedHealthShare ? _enragedSpawnInterval : _spawnInterval;
        }

        /// <summary>
        /// Advances by <paramref name="deltaTime"/> at <paramref name="healthShare"/> (0..1) of her health;
        /// <paramref name="hasRoom"/> tells whether one more enemy may appear. Returns true if a runner comes out now.
        /// </summary>
        public bool Tick(float deltaTime, float healthShare, bool hasRoom)
        {
            SinceSpawn += deltaTime;
            PhaseTime += deltaTime;
            if (Phase == BroodMotherPhase.Walk)
            {
                if (hasRoom && SinceSpawn >= Interval(healthShare) - _birthTime)
                {
                    Enter(BroodMotherPhase.Birth);
                }

                return false;
            }

            if (PhaseTime < _birthTime)
            {
                return false;
            }

            Enter(BroodMotherPhase.Walk);
            if (!hasRoom)
            {
                return false;
            }

            SinceSpawn = 0f;
            return true;
        }

        private void Enter(BroodMotherPhase phase)
        {
            Phase = phase;
            PhaseTime = 0f;
        }
    }
}
