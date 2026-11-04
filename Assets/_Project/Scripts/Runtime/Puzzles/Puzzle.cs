using System;
using RushOps.Audio;
using RushOps.Levels;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Base of all puzzles (F-LVL-04, issue #30): inactive until its <see cref="Zone"/> becomes active (at once
    /// without a zone), then active until the puzzle type calls <see cref="Solve"/>. Solving plays a sound; a
    /// <see cref="PuzzleSolvedCondition"/> lets a gate open on it, and the mission tracker shows
    /// <see cref="Display"/>. The state rules live in <see cref="PuzzleLogic"/>. Puzzle types react in
    /// <see cref="OnActivated"/> and <see cref="OnSolved"/> and must not declare their own Awake, OnEnable, Start or
    /// OnDisable, which would hide the base ones.
    /// </summary>
    public abstract class Puzzle : MonoBehaviour
    {
        [Tooltip("Zone whose activation makes the puzzle active; empty for active from the start.")]
        [SerializeField] private Zone _zone;
        [SerializeField] private PuzzleConfig _config;
        [SerializeField] private SfxPlayer _sfx;
        [Tooltip("Played when the puzzle is solved.")]
        [SerializeField] private AudioClip _solvedClip;

        private readonly PuzzleLogic _logic = new PuzzleLogic();

        /// <summary>Raised once when the puzzle becomes active.</summary>
        public event Action<Puzzle> Activated;

        /// <summary>Raised once when the puzzle is solved.</summary>
        public event Action<Puzzle> Solved;

        /// <summary>Current state.</summary>
        public PuzzleState State => _logic.State;

        /// <summary>Zone whose activation makes the puzzle active, or null.</summary>
        public Zone Zone => _zone;

        /// <summary>What the mission tracker shows for this puzzle right now.</summary>
        public abstract PuzzleDisplay Display { get; }

        /// <summary>Activates the puzzle if it is inactive, e.g. when its zone becomes active.</summary>
        /// <returns>True if the state changed.</returns>
        public bool Activate()
        {
            return _logic.Activate();
        }

        /// <summary>Solves the puzzle if it is active.</summary>
        /// <returns>True if the state changed.</returns>
        protected bool Solve()
        {
            return _logic.Solve();
        }

        /// <summary>Called once when the puzzle became active.</summary>
        protected virtual void OnActivated()
        {
        }

        /// <summary>Called once when the puzzle was solved, after the sound started.</summary>
        protected virtual void OnSolved()
        {
        }

        private void Awake()
        {
            _logic.Activated += HandleActivated;
            _logic.Solved += HandleSolved;
        }

        private void OnEnable()
        {
            if (_zone != null)
            {
                _zone.Activated += OnZoneActivated;
            }
        }

        private void Start()
        {
            // Covers a zone that was active before this subscribed, and puzzles without a zone.
            if (_zone == null || _zone.State != ZoneState.Inactive)
            {
                Activate();
            }
        }

        private void OnDisable()
        {
            if (_zone != null)
            {
                _zone.Activated -= OnZoneActivated;
            }
        }

        private void OnZoneActivated(Zone zone)
        {
            Activate();
        }

        private void HandleActivated()
        {
            OnActivated();
            Activated?.Invoke(this);
        }

        private void HandleSolved()
        {
            if (_sfx != null && _config != null)
            {
                _sfx.Play(_solvedClip, _config.SolvedVolume, 0f);
            }

            OnSolved();
            Solved?.Invoke(this);
        }
    }
}
