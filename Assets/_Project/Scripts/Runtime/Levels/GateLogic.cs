using System;

namespace RushOps.Levels
{
    /// <summary>
    /// State of one gate (F-LVL-02): closed until its <see cref="IGateCondition"/> is met, then opening for
    /// a fixed duration, then open for good. Without a condition the gate stays closed.
    /// </summary>
    public class GateLogic
    {
        private readonly IGateCondition _condition;
        private readonly float _openDuration;
        private float _elapsed;

        /// <summary>Creates a closed gate that waits for <paramref name="condition"/> and opens over <paramref name="openDuration"/> seconds.</summary>
        public GateLogic(IGateCondition condition, float openDuration)
        {
            _condition = condition;
            _openDuration = openDuration > 0f ? openDuration : 0f;
        }

        /// <summary>Current state.</summary>
        public GateState State { get; private set; } = GateState.Closed;

        /// <summary>0 while closed, rising to 1 during the opening, 1 when open.</summary>
        public float OpenProgress
        {
            get
            {
                switch (State)
                {
                    case GateState.Open:
                        return 1f;
                    case GateState.Opening:
                        return _openDuration > 0f ? Math.Min(_elapsed / _openDuration, 1f) : 1f;
                    default:
                        return 0f;
                }
            }
        }

        /// <summary>Raised once when the condition is met and the opening starts.</summary>
        public event Action Opening;

        /// <summary>Raised once when the passage is free.</summary>
        public event Action Opened;

        /// <summary>Advances the gate by <paramref name="deltaTime"/> seconds: checks the condition while closed and runs the opening.</summary>
        public void Tick(float deltaTime)
        {
            if (State == GateState.Closed)
            {
                if (_condition == null || !_condition.IsMet)
                {
                    return;
                }

                State = GateState.Opening;
                _elapsed = 0f;
                Opening?.Invoke();
                if (_openDuration <= 0f)
                {
                    Finish();
                }

                return;
            }

            if (State == GateState.Opening)
            {
                _elapsed += deltaTime;
                if (_elapsed >= _openDuration)
                {
                    Finish();
                }
            }
        }

        private void Finish()
        {
            State = GateState.Open;
            Opened?.Invoke();
        }
    }
}
