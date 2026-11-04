using RushOps.Combat;
using RushOps.Levels;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Timed switches (F-RAE-03 to F-RAE-05, issue #32): the player steps on all <see cref="FloorSwitch"/>es; the first
    /// one starts a countdown shown in the mission tracker. If it runs out, all switches reset, blink red and a buzzer
    /// sounds; all pressed in time solves the puzzle. Rules in <see cref="SwitchSequence"/>, values in
    /// <see cref="TimedSwitchConfig"/>.
    /// </summary>
    public class TimedSwitchPuzzle : Puzzle
    {
        [SerializeField] private TimedSwitchConfig _switchConfig;
        [Tooltip("Difficulty of the level; scales the time limit. Empty = as in the switch config.")]
        [SerializeField] private LevelDifficulty _difficulty;
        [Tooltip("The plates, 2 to 5 per puzzle.")]
        [SerializeField] private FloorSwitch[] _switches = System.Array.Empty<FloorSwitch>();
        [SerializeField] private AudioClip _pressClip;
        [SerializeField] private AudioClip _resetClip;

        private SwitchSequence _sequence;
        private string[] _texts;
        private float _sinceReset = -1f;

        /// <summary>The switch rules of this puzzle.</summary>
        public SwitchSequence Sequence => _sequence ??= new SwitchSequence(
            _switches.Length,
            DifficultyMath.SwitchTimeLimit(_switchConfig != null ? _switchConfig.TimeLimit : 0f, _difficulty));

        /// <summary>The plates of this puzzle.</summary>
        public FloorSwitch[] Switches => _switches;

        /// <summary>True while the switches blink after a reset.</summary>
        public bool IsFlashingReset => _sinceReset >= 0f;

        /// <inheritdoc />
        public override PuzzleDisplay Display
        {
            get
            {
                string text = Text(Sequence.PressedCount);
                return Sequence.IsRunning ? PuzzleDisplay.WithCountdown(text, Sequence.Remaining) : PuzzleDisplay.TextOnly(text);
            }
        }

        /// <summary>Presses the switch at <paramref name="index"/> as if the player stepped on it.</summary>
        public void Press(int index)
        {
            if (State != PuzzleState.Active || !Sequence.Press(index))
            {
                return;
            }

            _switches[index].SetPressed(true);
            EndResetFlash();
            PlaySound(_pressClip, _switchConfig != null ? _switchConfig.PressVolume : 0f);
            if (Sequence.IsComplete)
            {
                Solve();
            }
        }

        /// <inheritdoc />
        protected override void OnActivated()
        {
            for (int i = 0; i < _switches.Length; i++)
            {
                if (_switches[i] != null)
                {
                    _switches[i].Stepped += OnStepped;
                    _switches[i].SetPressed(false);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnSolved()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (State == PuzzleState.Active && Sequence.Tick(Time.deltaTime))
            {
                OnExpired();
            }

            if (_sinceReset < 0f || _switchConfig == null)
            {
                return;
            }

            _sinceReset += Time.deltaTime;
            if (_sinceReset >= _switchConfig.ResetFlashDuration)
            {
                EndResetFlash();
                return;
            }

            bool red = !BlinkMath.IsVisible(_sinceReset, _switchConfig.ResetFlashInterval);
            Color tint = red ? _switchConfig.ResetColor : Color.white;
            for (int i = 0; i < _switches.Length; i++)
            {
                if (_switches[i] != null)
                {
                    _switches[i].SetTint(tint);
                }
            }
        }

        private void OnExpired()
        {
            for (int i = 0; i < _switches.Length; i++)
            {
                if (_switches[i] != null)
                {
                    _switches[i].SetPressed(false);
                }
            }

            _sinceReset = 0f;
            PlaySound(_resetClip, _switchConfig != null ? _switchConfig.ResetVolume : 0f);
        }

        private void EndResetFlash()
        {
            _sinceReset = -1f;
            for (int i = 0; i < _switches.Length; i++)
            {
                if (_switches[i] != null)
                {
                    _switches[i].SetTint(Color.white);
                }
            }
        }

        private void OnStepped(FloorSwitch plate)
        {
            Press(System.Array.IndexOf(_switches, plate));
        }

        // Cached per pressed count, so the tracker reading Display every frame allocates nothing.
        private string Text(int pressed)
        {
            if (_texts == null)
            {
                string label = _switchConfig != null ? _switchConfig.Text : string.Empty;
                _texts = new string[_switches.Length + 1];
                for (int i = 0; i < _texts.Length; i++)
                {
                    _texts[i] = $"{label} {i}/{_switches.Length}";
                }
            }

            return _texts[Mathf.Clamp(pressed, 0, _texts.Length - 1)];
        }

        private void Unsubscribe()
        {
            for (int i = 0; i < _switches.Length; i++)
            {
                if (_switches[i] != null)
                {
                    _switches[i].Stepped -= OnStepped;
                }
            }
        }
    }
}
