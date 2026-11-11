using RushOps.Enemies;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Pressure plate only a brute pushes down (issue #289, F-RAE-09, F-RAE-10, F-RAE-13, N26): thin adapter on
    /// <see cref="PressurePlateLogic"/>. Counts the brutes of the brute pool whose feet stand on the plate; the player and
    /// other zombies never count. A ring fills while one stands on it; after the hold time the plate locks and the puzzle
    /// is solved. A hint bubble with a brute head floats above it, and the mission tracker explains it the first time
    /// (<see cref="PuzzleHints"/>); that it was explained is remembered once the plate is solved.
    /// </summary>
    public class PressurePlatePuzzle : Puzzle, IReinforcedPuzzle
    {
        [SerializeField] private PressurePlateConfig _plateConfig;
        [Tooltip("Pool of the brutes; only its enemies press the plate.")]
        [SerializeField] private EnemyPool _brutes;
        [Tooltip("Remembers the shown hint; empty = always explain.")]
        [SerializeField] private SaveGameService _save;
        [SerializeField] private SpriteRenderer _plate;
        [SerializeField] private Sprite _upSprite;
        [SerializeField] private Sprite _downSprite;
        [SerializeField] private Sprite _lockedSprite;
        [Tooltip("Ring that fills while a brute stands on the plate, one frame per step.")]
        [SerializeField] private SpriteRenderer _ring;
        [SerializeField] private Sprite[] _ringFrames = System.Array.Empty<Sprite>();
        [SerializeField] private SpriteRenderer _hint;
        [SerializeField] private AudioClip _sinkClip;
        [SerializeField] private AudioClip _lockClip;

        private PressurePlateLogic _logic;
        private bool _firstTime = true;
        private bool _wasPressed;
        private int _shownFrame = -1;

        /// <summary>The plate's rules.</summary>
        public PressurePlateLogic Logic => _logic ??= new PressurePlateLogic(_plateConfig != null ? _plateConfig.HoldSeconds : 1.5f);

        /// <summary>Brutes standing on the plate on the last frame.</summary>
        public int BrutesOnPlate { get; private set; }

        /// <summary>True while the explaining hint text shows.</summary>
        public bool ExplainsHint => _firstTime;

        /// <inheritdoc />
        public float ReinforcementInterval => _plateConfig != null ? _plateConfig.BruteInterval : 12f;

        /// <inheritdoc />
        public int MaxReinforcements => _plateConfig != null ? _plateConfig.MaxBrutes : 2;

        /// <inheritdoc />
        public EnemyDefinition NeededEnemy => _brutes != null ? _brutes.Definition : null;

        /// <inheritdoc />
        public override PuzzleDisplay Display => Logic.IsPressed && !Logic.IsLocked
            ? PuzzleDisplay.WithProgress("Druckplatte", Logic.Progress)
            : PuzzleDisplay.TextOnly(PuzzleHints.Text(ZombiePuzzleKind.PressurePlate, _firstTime));

        /// <summary>Radius in tiles in which a brute's feet press the plate.</summary>
        public float Radius => _plateConfig != null ? _plateConfig.Radius : 0.8f;

        /// <summary>Test hook: locks an active plate as if a brute had stood on it long enough.</summary>
        public void DebugLock()
        {
            if (State == PuzzleState.Active && Logic.Tick(Logic.Elapsed + 1000f, 1))
            {
                PlaySound(_lockClip, Volume);
                Solve();
            }
        }

        /// <inheritdoc />
        protected override void OnActivated()
        {
            _firstTime = _save == null || !PuzzleHints.IsSeen(_save.Data.Hints, ZombiePuzzleKind.PressurePlate);
        }

        /// <inheritdoc />
        protected override void OnSolved()
        {
            if (_save != null)
            {
                // Saved with the next autosave (level done, death, leaving).
                PuzzleHints.MarkSeen(_save.Data.Hints, ZombiePuzzleKind.PressurePlate);
            }

            if (_hint != null)
            {
                _hint.enabled = false;
            }
        }

        private void Update()
        {
            if (State == PuzzleState.Active)
            {
                BrutesOnPlate = CountBrutes();
                if (Logic.Tick(Time.deltaTime, BrutesOnPlate))
                {
                    PlaySound(_lockClip, Volume);
                    Solve();
                }
                else if (Logic.IsPressed && !_wasPressed)
                {
                    PlaySound(_sinkClip, Volume);
                }

                _wasPressed = Logic.IsPressed;
            }

            ShowPlate();
            BobHint();
        }

        private float Volume => _plateConfig != null ? _plateConfig.Volume : 0.8f;

        private int CountBrutes()
        {
            if (_brutes == null)
            {
                return 0;
            }

            float radius = Radius;
            float radiusSquared = radius * radius;
            Vector2 center = transform.position;
            int count = 0;
            var active = _brutes.Active;
            for (int i = 0; i < active.Count; i++)
            {
                Enemy brute = active[i];
                if (brute != null && !brute.IsDead && ((Vector2)brute.transform.position - center).sqrMagnitude <= radiusSquared)
                {
                    count++;
                }
            }

            return count;
        }

        // Sprites change only when the state or the ring frame changes.
        private void ShowPlate()
        {
            int frame = Logic.IsLocked ? -2 : Logic.IsPressed && _ringFrames.Length > 0
                ? Mathf.Min(_ringFrames.Length - 1, (int)(Logic.Progress * _ringFrames.Length))
                : -1;
            if (frame == _shownFrame)
            {
                return;
            }

            _shownFrame = frame;
            if (_plate != null)
            {
                _plate.sprite = Logic.IsLocked ? _lockedSprite : Logic.IsPressed ? _downSprite : _upSprite;
            }

            if (_ring != null)
            {
                _ring.enabled = frame >= 0;
                if (frame >= 0)
                {
                    _ring.sprite = _ringFrames[frame];
                }
            }
        }

        private void BobHint()
        {
            if (_hint == null || !_hint.enabled || _plateConfig == null)
            {
                return;
            }

            float y = _plateConfig.HintHeight + _plateConfig.HintBob * Mathf.Sin(Time.time * 3f);
            _hint.transform.localPosition = new Vector3(0f, y, 0f);
        }
    }
}
