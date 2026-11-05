using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// Life of one Mega-Boomer puddle as a pure rule (decision C9b): it glows for the warm-up time, then hurts on
    /// contact until its lifetime (counted from when it appeared) is over. It never explodes.
    /// </summary>
    public class PuddleTimer
    {
        private readonly float _warmUp;
        private readonly float _lifetime;

        /// <summary>Creates the rule: harmless for <paramref name="warmUp"/> s, gone after <paramref name="lifetime"/> s.</summary>
        public PuddleTimer(float warmUp, float lifetime)
        {
            _warmUp = Mathf.Max(0f, warmUp);
            _lifetime = Mathf.Max(_warmUp, lifetime);
        }

        /// <summary>Seconds since the puddle appeared.</summary>
        public float Age { get; private set; }

        /// <summary>Current state.</summary>
        public PuddleState State => Age < _warmUp ? PuddleState.Warm : Age < _lifetime ? PuddleState.Active : PuddleState.Gone;

        /// <summary>Starts again from a fresh puddle.</summary>
        public void Restart()
        {
            Age = 0f;
        }

        /// <summary>Advances by <paramref name="deltaTime"/> seconds; returns the state afterwards.</summary>
        public PuddleState Tick(float deltaTime)
        {
            Age += Mathf.Max(0f, deltaTime);
            return State;
        }
    }
}
