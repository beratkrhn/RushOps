using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Enemy pressure during a puzzle (Lastenheft 2.6, issue #34): while the puzzle on this object is active, its zone
    /// runs these waves instead of its own, e.g. with shorter spawn intervals. Once solved the zone goes back to its
    /// normal waves or ends them, as set here. Set per puzzle in the level; a puzzle without it keeps the zone's waves.
    /// </summary>
    [RequireComponent(typeof(Puzzle))]
    public class PuzzlePressure : MonoBehaviour
    {
        [SerializeField] private WaveDirector _director;
        [Tooltip("Waves of the zone while the puzzle is active.")]
        [SerializeField] private WaveDefinition _waves;
        [Tooltip("What the zone's waves do once the puzzle is solved.")]
        [SerializeField] private AfterPuzzle _afterSolved = AfterPuzzle.ResumeZoneWaves;

        private Puzzle _puzzle;

        /// <summary>Waves of the zone while the puzzle is active.</summary>
        public WaveDefinition Waves => _waves;

        /// <summary>What the zone's waves do once the puzzle is solved.</summary>
        public AfterPuzzle AfterSolved => _afterSolved;

        /// <summary>The waves the zone runs after the puzzle: its own, or none.</summary>
        public static WaveDefinition WavesAfter(AfterPuzzle after, WaveDefinition zoneWaves)
        {
            return after == AfterPuzzle.ResumeZoneWaves ? zoneWaves : null;
        }

        private void Awake()
        {
            _puzzle = GetComponent<Puzzle>();
        }

        // Subscribed before the puzzle's Start, so an activation in that Start is seen.
        private void OnEnable()
        {
            _puzzle.Activated += OnActivated;
            _puzzle.Solved += OnSolved;
            if (_puzzle.State == PuzzleState.Active)
            {
                OnActivated(_puzzle);
            }
        }

        private void OnDisable()
        {
            _puzzle.Activated -= OnActivated;
            _puzzle.Solved -= OnSolved;
        }

        private void OnActivated(Puzzle puzzle)
        {
            if (_director != null && puzzle.Zone != null && _waves != null)
            {
                _director.UseWaves(puzzle.Zone, _waves);
            }
        }

        private void OnSolved(Puzzle puzzle)
        {
            if (_director == null || puzzle.Zone == null || _waves == null)
            {
                return;
            }

            WaveDefinition next = WavesAfter(_afterSolved, puzzle.Zone.Waves);
            if (next != null)
            {
                _director.UseWaves(puzzle.Zone, next);
            }
            else
            {
                _director.StopWaves(puzzle.Zone);
            }
        }
    }
}
