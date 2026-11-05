using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// The Mega-Boomer's moves as pure rules (issue #159, decisions C9a, C9b): it walks at the player and leaves a
    /// puddle every few seconds. The whole boss splits once its health share falls to the split share: it stands still
    /// for the split time, then two halves take over, each with half of its remaining health. A half does not split
    /// again.
    /// </summary>
    public class MegaBoomerLogic
    {
        private readonly float _puddleInterval;
        private readonly float _splitShare;
        private readonly float _splitTime;
        private readonly bool _canSplit;

        /// <summary>
        /// Creates the rules: a puddle every <paramref name="puddleInterval"/> s while walking; if
        /// <paramref name="canSplit"/>, a split of <paramref name="splitTime"/> s once the health share is at or below
        /// <paramref name="splitShare"/> (0..1).
        /// </summary>
        public MegaBoomerLogic(float puddleInterval, float splitShare, float splitTime, bool canSplit)
        {
            _puddleInterval = Mathf.Max(0.1f, puddleInterval);
            _splitShare = Mathf.Clamp01(splitShare);
            _splitTime = Mathf.Max(0f, splitTime);
            _canSplit = canSplit;
        }

        /// <summary>Current phase.</summary>
        public MegaBoomerPhase Phase { get; private set; } = MegaBoomerPhase.Walk;

        /// <summary>Seconds in the current phase.</summary>
        public float PhaseTime { get; private set; }

        /// <summary>Seconds since the last puddle, or since the fight started.</summary>
        public float SincePuddle { get; private set; }

        /// <summary>
        /// Splits <paramref name="remaining"/> hit points into the two halves; they add up to it and differ by at most
        /// one. Each half has at least one hit point.
        /// </summary>
        public static (int first, int second) SplitHealth(int remaining)
        {
            int first = Mathf.Max(1, (remaining + 1) / 2);
            int second = Mathf.Max(1, remaining - first);
            return (first, second);
        }

        /// <summary>
        /// Advances by <paramref name="deltaTime"/> at <paramref name="healthShare"/> (0..1) of the health; returns
        /// what happens now.
        /// </summary>
        public MegaBoomerEvent Tick(float deltaTime, float healthShare)
        {
            PhaseTime += deltaTime;
            switch (Phase)
            {
                case MegaBoomerPhase.Walk:
                    if (_canSplit && healthShare > 0f && healthShare <= _splitShare)
                    {
                        Enter(MegaBoomerPhase.Split);
                        return MegaBoomerEvent.SplitStarted;
                    }

                    SincePuddle += deltaTime;
                    if (SincePuddle >= _puddleInterval)
                    {
                        SincePuddle -= _puddleInterval;
                        return MegaBoomerEvent.Puddle;
                    }

                    return MegaBoomerEvent.None;
                case MegaBoomerPhase.Split:
                    if (PhaseTime < _splitTime)
                    {
                        return MegaBoomerEvent.None;
                    }

                    Enter(MegaBoomerPhase.Done);
                    return MegaBoomerEvent.Split;
                default:
                    return MegaBoomerEvent.None;
            }
        }

        private void Enter(MegaBoomerPhase phase)
        {
            Phase = phase;
            PhaseTime = 0f;
        }
    }
}
