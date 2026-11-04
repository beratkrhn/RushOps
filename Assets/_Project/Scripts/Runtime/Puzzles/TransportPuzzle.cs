using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Object transport (F-RAE-06 to F-RAE-08, issue #33): the player brings energy cells to a <see cref="Generator"/>;
    /// once it got the required number (1 by default) the puzzle is solved. The tracker shows the task, or the count
    /// with a bar for several cells. Rules in <see cref="TransportGoal"/>, values in <see cref="CarryConfig"/>.
    /// </summary>
    public class TransportPuzzle : Puzzle
    {
        [SerializeField] private CarryConfig _carryConfig;
        [SerializeField] private Generator _generator;
        [Tooltip("The player's hands; tells when a cell of this puzzle was picked up.")]
        [SerializeField] private PlayerCarrier _carrier;
        [Tooltip("Cells lying in the level for this generator.")]
        [SerializeField] private EnergyCell[] _cells = System.Array.Empty<EnergyCell>();
        [Tooltip("Cells the generator needs.")]
        [SerializeField, Min(1)] private int _requiredCells = 1;
        [SerializeField] private AudioClip _pickUpClip;
        [SerializeField] private AudioClip _handInClip;

        private TransportGoal _goal;
        private string[] _texts;

        /// <summary>Delivered and required cells.</summary>
        public TransportGoal Goal => _goal ??= new TransportGoal(_requiredCells);

        /// <summary>The target station.</summary>
        public Generator Generator => _generator;

        /// <summary>The cells for this generator.</summary>
        public EnergyCell[] Cells => _cells;

        /// <inheritdoc />
        public override PuzzleDisplay Display => Goal.Required > 1
            ? PuzzleDisplay.WithProgress(Text(Goal.Delivered), Goal.Progress)
            : PuzzleDisplay.TextOnly(_carryConfig != null ? _carryConfig.SingleText : string.Empty);

        /// <inheritdoc />
        protected override void OnActivated()
        {
            if (_generator != null)
            {
                _generator.Touched += OnGeneratorTouched;
                _generator.SetRunning(false);
            }

            if (_carrier != null)
            {
                _carrier.PickedUp += OnPickedUp;
            }
        }

        /// <inheritdoc />
        protected override void OnSolved()
        {
            Unsubscribe();
            if (_generator != null)
            {
                _generator.SetRunning(true);
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void OnGeneratorTouched(PlayerCarrier carrier)
        {
            if (State != PuzzleState.Active)
            {
                return;
            }

            EnergyCell cell = carrier.Carried;
            if (cell == null || System.Array.IndexOf(_cells, cell) < 0)
            {
                return;
            }

            carrier.HandIn();
            cell.Consume();
            Goal.Deliver();
            PlaySound(_handInClip, _carryConfig != null ? _carryConfig.HandInVolume : 0f);
            if (Goal.IsComplete)
            {
                Solve();
            }
        }

        private void OnPickedUp(EnergyCell cell)
        {
            if (System.Array.IndexOf(_cells, cell) >= 0)
            {
                PlaySound(_pickUpClip, _carryConfig != null ? _carryConfig.PickUpVolume : 0f);
            }
        }

        private void Unsubscribe()
        {
            if (_generator != null)
            {
                _generator.Touched -= OnGeneratorTouched;
            }

            if (_carrier != null)
            {
                _carrier.PickedUp -= OnPickedUp;
            }
        }

        // Cached per count, so the tracker reading Display every frame allocates nothing.
        private string Text(int delivered)
        {
            if (_texts == null)
            {
                string label = _carryConfig != null ? _carryConfig.MultiText : string.Empty;
                _texts = new string[Goal.Required + 1];
                for (int i = 0; i < _texts.Length; i++)
                {
                    _texts[i] = $"{label} {i}/{Goal.Required}";
                }
            }

            return _texts[Mathf.Clamp(delivered, 0, _texts.Length - 1)];
        }
    }
}
