using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// The Koloss's moves as pure rules (issue #151): chase for a while, charge up for a moment with the direction
    /// locked at the start (L7), rush straight ahead until a wall stops it, stand dazed (C7), chase again. A rush that
    /// meets no wall ends after its longest time and goes back to chasing.
    /// </summary>
    public class ColossusLogic
    {
        private readonly float _chaseTime;
        private readonly float _windupTime;
        private readonly float _maxChargeTime;
        private readonly float _stunTime;

        /// <summary>Creates the rules with the given times in seconds.</summary>
        public ColossusLogic(float chaseTime, float windupTime, float maxChargeTime, float stunTime)
        {
            _chaseTime = Mathf.Max(0f, chaseTime);
            _windupTime = Mathf.Max(0f, windupTime);
            _maxChargeTime = Mathf.Max(0.01f, maxChargeTime);
            _stunTime = Mathf.Max(0f, stunTime);
        }

        /// <summary>Current phase.</summary>
        public ColossusPhase Phase { get; private set; } = ColossusPhase.Chase;

        /// <summary>Seconds in the current phase.</summary>
        public float PhaseTime { get; private set; }

        /// <summary>Direction of the rush, locked when the charge up starts; unit length.</summary>
        public Vector2 ChargeDirection { get; private set; } = Vector2.down;

        /// <summary>Share of the charge up done, 0..1; 0 outside the charge up.</summary>
        public float WindupProgress => Phase == ColossusPhase.Windup && _windupTime > 0f
            ? Mathf.Clamp01(PhaseTime / _windupTime)
            : 0f;

        /// <summary>
        /// Advances by <paramref name="deltaTime"/>; <paramref name="toTarget"/> points from the Koloss to the player.
        /// Returns true if the phase changed.
        /// </summary>
        public bool Tick(float deltaTime, Vector2 toTarget)
        {
            PhaseTime += deltaTime;
            switch (Phase)
            {
                case ColossusPhase.Chase when PhaseTime >= _chaseTime:
                    if (toTarget.sqrMagnitude > 1e-6f)
                    {
                        ChargeDirection = toTarget.normalized;
                    }

                    return Enter(ColossusPhase.Windup);
                case ColossusPhase.Windup when PhaseTime >= _windupTime:
                    return Enter(ColossusPhase.Charge);
                case ColossusPhase.Charge when PhaseTime >= _maxChargeTime:
                    return Enter(ColossusPhase.Chase);
                case ColossusPhase.Stunned when PhaseTime >= _stunTime:
                    return Enter(ColossusPhase.Chase);
                default:
                    return false;
            }
        }

        /// <summary>Reports a wall in the way; during a rush the Koloss is dazed. Returns true if it was rushing.</summary>
        public bool HitWall()
        {
            return Phase == ColossusPhase.Charge && Enter(ColossusPhase.Stunned);
        }

        /// <summary>
        /// Velocity in this phase: <paramref name="chaseSpeed"/> along <paramref name="chaseDirection"/> while chasing,
        /// <paramref name="chargeSpeed"/> along the locked direction while rushing, standing still otherwise.
        /// </summary>
        public Vector2 Velocity(Vector2 chaseDirection, float chaseSpeed, float chargeSpeed)
        {
            switch (Phase)
            {
                case ColossusPhase.Chase:
                    return chaseDirection * chaseSpeed;
                case ColossusPhase.Charge:
                    return ChargeDirection * chargeSpeed;
                default:
                    return Vector2.zero;
            }
        }

        private bool Enter(ColossusPhase phase)
        {
            Phase = phase;
            PhaseTime = 0f;
            return true;
        }
    }
}
