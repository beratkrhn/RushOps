using System;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Timed switches (F-RAE-03, F-RAE-05, issue #32): the first pressed switch starts the timer; when all are pressed
    /// before it runs out the sequence is complete, otherwise all switches reset and the next press starts over.
    /// </summary>
    public sealed class SwitchSequence
    {
        // Limits that are whole multiples of the time step end on that step despite float rounding.
        private const float Epsilon = 1e-4f;

        private readonly bool[] _pressed;
        private readonly float _timeLimit;

        /// <param name="count">Number of switches, at least 1.</param>
        /// <param name="timeLimit">Seconds from the first press to press all; 0 or less never runs out.</param>
        public SwitchSequence(int count, float timeLimit)
        {
            _pressed = new bool[count > 1 ? count : 1];
            _timeLimit = timeLimit > 0f ? timeLimit : 0f;
        }

        /// <summary>Raised when the timer ran out and all switches reset.</summary>
        public event Action Expired;

        /// <summary>Raised once when all switches are pressed in time.</summary>
        public event Action Completed;

        /// <summary>Number of switches.</summary>
        public int Count => _pressed.Length;

        /// <summary>Switches pressed in the current attempt.</summary>
        public int PressedCount { get; private set; }

        /// <summary>True while the timer runs.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>Seconds left on the timer; the full limit while it does not run.</summary>
        public float Remaining { get; private set; }

        /// <summary>True once all switches were pressed in time.</summary>
        public bool IsComplete { get; private set; }

        /// <summary>True if the switch at <paramref name="index"/> is pressed in the current attempt.</summary>
        public bool IsPressed(int index)
        {
            return index >= 0 && index < _pressed.Length && _pressed[index];
        }

        /// <summary>Presses the switch at <paramref name="index"/>; the first press starts the timer.</summary>
        /// <returns>True if the switch was not pressed before.</returns>
        public bool Press(int index)
        {
            if (IsComplete || index < 0 || index >= _pressed.Length || _pressed[index])
            {
                return false;
            }

            if (!IsRunning)
            {
                IsRunning = true;
                Remaining = _timeLimit;
            }

            _pressed[index] = true;
            PressedCount++;
            if (PressedCount == _pressed.Length)
            {
                IsRunning = false;
                IsComplete = true;
                Completed?.Invoke();
            }

            return true;
        }

        /// <summary>Advances the timer by <paramref name="deltaTime"/> seconds; running out resets all switches.</summary>
        /// <returns>True on the step the timer ran out.</returns>
        public bool Tick(float deltaTime)
        {
            if (!IsRunning || _timeLimit <= 0f || deltaTime <= 0f)
            {
                return false;
            }

            Remaining -= deltaTime;
            if (Remaining > Epsilon)
            {
                return false;
            }

            Array.Clear(_pressed, 0, _pressed.Length);
            PressedCount = 0;
            IsRunning = false;
            Remaining = _timeLimit;
            Expired?.Invoke();
            return true;
        }
    }
}
