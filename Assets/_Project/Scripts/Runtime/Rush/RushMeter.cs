using System;

namespace RushOps.Rush
{
    /// <summary>
    /// The rush meter (F-RSH-01, F-RSH-02, F-RSH-06, decisions RM4, N15, N16): every kill fills it by the combo
    /// multiplier of that kill, up to full. Only a full meter can be triggered; that empties it and runs the rush for
    /// its duration. While the rush runs the meter does not fill. A hit taken does not touch the meter. Death and level
    /// end call <see cref="End"/>; a new level or run starts with a new meter or calls <see cref="Reset"/>.
    /// </summary>
    public sealed class RushMeter
    {
        // Durations that are whole multiples of the time step end on that step despite float rounding.
        private const float Epsilon = 1e-4f;

        /// <param name="fillTarget">Fill at which the meter is full, at least 1.</param>
        /// <param name="duration">Seconds a triggered rush lasts.</param>
        public RushMeter(int fillTarget, float duration)
        {
            FillTarget = fillTarget > 1 ? fillTarget : 1;
            Duration = duration > 0f ? duration : 0f;
        }

        /// <summary>Meter with the values of <paramref name="config"/>.</summary>
        public RushMeter(RushConfig config)
            : this(config != null ? config.KillsToFill : 1, config != null ? config.Duration : 0f)
        {
        }

        /// <summary>Raised when <see cref="Fill"/> changes, with the new fill and the target.</summary>
        public event Action<int, int> FillChanged;

        /// <summary>Raised once when the meter becomes full.</summary>
        public event Action Filled;

        /// <summary>Raised when a rush starts.</summary>
        public event Action Started;

        /// <summary>Raised when a rush ends, by running out or through <see cref="End"/>.</summary>
        public event Action Ended;

        /// <summary>Current fill, 0 to <see cref="FillTarget"/>.</summary>
        public int Fill { get; private set; }

        /// <summary>Fill at which the meter is full.</summary>
        public int FillTarget { get; }

        /// <summary>Seconds a rush lasts.</summary>
        public float Duration { get; }

        /// <summary>True while the rush runs.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Seconds left of the running rush; 0 without one.</summary>
        public float Remaining { get; private set; }

        /// <summary>True if <see cref="TryTrigger"/> would start a rush now.</summary>
        public bool IsFull => !IsActive && Fill >= FillTarget;

        /// <summary>Fill for the HUD, 0 (empty) to 1 (full).</summary>
        public float FillProgress => (float)Fill / FillTarget;

        /// <summary>Share of the running rush that is left for the HUD, 1 at the start to 0; 0 without a rush.</summary>
        public float RemainingProgress => IsActive && Duration > 0f ? Remaining / Duration : 0f;

        /// <summary>Adds a kill made at <paramref name="multiplier"/>; ignored while the rush runs or when full.</summary>
        public void RegisterKill(int multiplier)
        {
            if (IsActive || multiplier <= 0 || Fill >= FillTarget)
            {
                return;
            }

            SetFill(Math.Min(FillTarget, Fill + multiplier));
            if (Fill >= FillTarget)
            {
                Filled?.Invoke();
            }
        }

        /// <summary>Starts the rush if the meter is full; empties it.</summary>
        /// <returns>True if a rush started.</returns>
        public bool TryTrigger()
        {
            if (!IsFull)
            {
                return false;
            }

            SetFill(0);
            IsActive = true;
            Remaining = Duration;
            Started?.Invoke();
            if (Remaining <= 0f)
            {
                Stop();
            }

            return true;
        }

        /// <summary>Advances the running rush by <paramref name="deltaTime"/> seconds; a run-out rush ends.</summary>
        public void Tick(float deltaTime)
        {
            if (!IsActive || deltaTime <= 0f)
            {
                return;
            }

            Remaining -= deltaTime;
            if (Remaining <= Epsilon)
            {
                Stop();
            }
        }

        /// <summary>Ends a running rush at once, e.g. on death or level end. The fill stays.</summary>
        public void End()
        {
            if (IsActive)
            {
                Stop();
            }
        }

        /// <summary>Ends a running rush and empties the meter, e.g. for a new level or run.</summary>
        public void Reset()
        {
            End();
            SetFill(0);
        }

        private void Stop()
        {
            IsActive = false;
            Remaining = 0f;
            Ended?.Invoke();
        }

        private void SetFill(int fill)
        {
            if (fill == Fill)
            {
                return;
            }

            Fill = fill;
            FillChanged?.Invoke(Fill, FillTarget);
        }
    }
}
