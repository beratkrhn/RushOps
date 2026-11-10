using System;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>
    /// The dog's announced lunge as a state machine (F-GEG-06, decision H2): chase → crouch (stands still) → lunge in a
    /// straight line to where the player stood when the crouch began → recovery (stands still) → chase. The lunge does
    /// not track the player; a sideways dodge makes it miss. A wall or a landed hit ends it early. Plain C#, the
    /// adapter moves the body with <see cref="Velocity"/> and reports walls and contacts.
    /// </summary>
    public class DogLunge
    {
        private readonly DogConfig _config;
        private float _elapsed;
        private float _lungeTime;

        /// <summary>Creates the state machine over <paramref name="config"/>, chasing.</summary>
        public DogLunge(DogConfig config)
        {
            _config = config != null ? config : throw new ArgumentNullException(nameof(config));
        }

        /// <summary>Current state.</summary>
        public DogState State { get; private set; } = DogState.Chase;

        /// <summary>Seconds spent in the current state.</summary>
        public float StateTime => _elapsed;

        /// <summary>Where the player stood when the crouch began; the end point of the lunge.</summary>
        public Vector2 Target { get; private set; }

        /// <summary>Unit direction of the current or last lunge; zero before the first.</summary>
        public Vector2 Direction { get; private set; }

        /// <summary>True while the dog must not move by itself: crouching or recovering.</summary>
        public bool IsStill => State == DogState.Crouch || State == DogState.Recovery;

        /// <summary>Velocity of the body while lunging; zero in all other states.</summary>
        public Vector2 Velocity => State == DogState.Lunge ? Direction * _config.LungeSpeed : Vector2.zero;

        /// <summary>Raised when the dog starts to crouch, for pose, flash and growl.</summary>
        public event Action Crouched;

        /// <summary>Raised when the lunge starts, for its sound.</summary>
        public event Action Lunged;

        /// <summary>Back to chasing, e.g. when the dog returns to the pool.</summary>
        public void Reset()
        {
            Enter(DogState.Chase);
            Target = Vector2.zero;
            Direction = Vector2.zero;
            _lungeTime = 0f;
        }

        /// <summary>
        /// Advances by <paramref name="deltaTime"/> seconds with the dog at <paramref name="dog"/> and the player at
        /// <paramref name="player"/>, and returns the new state. Time left over from a finished state carries on into
        /// the next one.
        /// </summary>
        public DogState Tick(float deltaTime, Vector2 dog, Vector2 player)
        {
            float left = Mathf.Max(0f, deltaTime);
            // Bounded: every pass either uses up the time or moves on to the next state.
            for (int pass = 0; pass < 4; pass++)
            {
                switch (State)
                {
                    case DogState.Chase:
                        if (!InRange(dog, player))
                        {
                            return State;
                        }

                        Target = player;
                        Enter(DogState.Crouch);
                        Crouched?.Invoke();
                        // The crouch starts now; the time of this tick counts towards it.
                        break;

                    case DogState.Crouch:
                        if (!Use(ref left, _config.CrouchDuration))
                        {
                            return State;
                        }

                        StartLunge(dog);
                        break;

                    case DogState.Lunge:
                        if (!Use(ref left, _lungeTime))
                        {
                            return State;
                        }

                        Enter(DogState.Recovery);
                        break;

                    case DogState.Recovery:
                        if (!Use(ref left, _config.RecoveryDuration))
                        {
                            return State;
                        }

                        Enter(DogState.Chase);
                        // A new crouch waits for the next tick, so a dog never chains lunges within one frame.
                        return State;
                }
            }

            return State;
        }

        /// <summary>A wall stopped the body: ends a running lunge early and starts the recovery.</summary>
        public void NotifyBlocked()
        {
            if (State == DogState.Lunge)
            {
                Enter(DogState.Recovery);
            }
        }

        /// <summary>
        /// The dog touches the player: returns the hearts the player loses, <paramref name="contactDamage"/> or 0 while
        /// <paramref name="playerInvulnerable"/> (after a hit, during the dash). A landed hit ends a running lunge;
        /// against an invulnerable player the lunge flies on.
        /// </summary>
        public int OnPlayerContact(int contactDamage, bool playerInvulnerable)
        {
            if (playerInvulnerable || contactDamage <= 0)
            {
                return 0;
            }

            if (State == DogState.Lunge)
            {
                Enter(DogState.Recovery);
            }

            return contactDamage;
        }

        private bool InRange(Vector2 dog, Vector2 player)
        {
            float range = _config.LungeTriggerDistance;
            return (player - dog).sqrMagnitude <= range * range;
        }

        private void StartLunge(Vector2 dog)
        {
            Vector2 toTarget = Target - dog;
            float distance = toTarget.magnitude;
            float travel = Mathf.Min(distance, _config.LungeDistance);
            if (travel <= 0f || _config.LungeSpeed <= 0f)
            {
                // Already on the spot: nothing to jump, recover right away.
                Enter(DogState.Recovery);
                Lunged?.Invoke();
                return;
            }

            Direction = toTarget / distance;
            _lungeTime = travel / _config.LungeSpeed;
            Enter(DogState.Lunge);
            Lunged?.Invoke();
        }

        // Spends the time of the current state; true when it reached duration, with the rest left in left.
        private bool Use(ref float left, float duration)
        {
            float needed = duration - _elapsed;
            if (left < needed)
            {
                _elapsed += left;
                left = 0f;
                return false;
            }

            left -= Mathf.Max(0f, needed);
            return true;
        }

        private void Enter(DogState state)
        {
            State = state;
            _elapsed = 0f;
        }
    }
}
