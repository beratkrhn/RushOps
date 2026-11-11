using System;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Rules of the pressure plate (issue #287, F-RAE-09, N26): only brutes are heavy enough. Once one has stood on it
    /// for the hold time the plate locks for good. If the last brute leaves or dies before, the progress drops to 0.
    /// The player and other zombies are never counted by the adapter.
    /// </summary>
    public sealed class PressurePlateLogic
    {
        // Hold times that are whole multiples of the time step end on that step despite float rounding.
        private const float Epsilon = 1e-4f;

        private readonly float _holdSeconds;

        /// <param name="holdSeconds">Seconds a brute has to stand on the plate; 0 or less locks at once.</param>
        public PressurePlateLogic(float holdSeconds)
        {
            _holdSeconds = holdSeconds > 0f ? holdSeconds : 0f;
        }

        /// <summary>Raised once when the plate locks.</summary>
        public event Action Locked;

        /// <summary>Seconds a brute has stood on the plate so far.</summary>
        public float Elapsed { get; private set; }

        /// <summary>Progress 0..1 toward locking.</summary>
        public float Progress => IsLocked ? 1f : (_holdSeconds > 0f ? Elapsed / _holdSeconds : 0f);

        /// <summary>True while at least one brute pressed the plate on the last step, or once it is locked.</summary>
        public bool IsPressed { get; private set; }

        /// <summary>True once the plate has locked.</summary>
        public bool IsLocked { get; private set; }

        /// <summary>Advances by <paramref name="deltaTime"/> seconds with <paramref name="brutesOnPlate"/> brutes on it.</summary>
        /// <returns>True on the step the plate locked.</returns>
        public bool Tick(float deltaTime, int brutesOnPlate)
        {
            if (IsLocked)
            {
                return false;
            }

            IsPressed = brutesOnPlate > 0;
            if (!IsPressed)
            {
                Elapsed = 0f;
                return false;
            }

            if (deltaTime > 0f)
            {
                Elapsed += deltaTime;
            }

            if (Elapsed < _holdSeconds - Epsilon)
            {
                return false;
            }

            Elapsed = _holdSeconds;
            IsLocked = true;
            Locked?.Invoke();
            return true;
        }
    }
}
