using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>
    /// Picking a weapon off the wall as a pure rule (issue #175, decision W8): standing in front of it fills a ring;
    /// once it is full the weapon is picked up. Stepping away, or holding that weapon already, empties the ring again.
    /// </summary>
    public sealed class WallPickupTimer
    {
        private readonly float _duration;

        /// <summary>Creates the rule: <paramref name="duration"/> seconds in front of the weapon pick it up.</summary>
        public WallPickupTimer(float duration)
        {
            _duration = Mathf.Max(0f, duration);
        }

        /// <summary>Fill of the ring, 0..1.</summary>
        public float Progress { get; private set; }

        /// <summary>
        /// Advances by <paramref name="deltaTime"/>; <paramref name="inFront"/> tells whether the player stands in front
        /// of the weapon, <paramref name="canPick"/> whether picking it up would change anything. Returns true once,
        /// in the step the ring fills up; the ring then starts empty again.
        /// </summary>
        public bool Tick(float deltaTime, bool inFront, bool canPick)
        {
            if (!inFront || !canPick)
            {
                Progress = 0f;
                return false;
            }

            Progress = _duration <= 0f ? 1f : Mathf.Min(1f, Progress + deltaTime / _duration);
            if (Progress < 1f)
            {
                return false;
            }

            Progress = 0f;
            return true;
        }
    }
}
