using RushOps.Audio;
using RushOps.Levels;
using RushOps.Puzzles;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Mission tracker top right (F-HUD-03): follows the exit gate of the zone the player is in. For a kill gate it
    /// shows „Tor gesperrt: 4/10 Kills“; when the gate opens it switches to „Tor offen!“ with a chime and hides
    /// after a moment. For a puzzle gate it shows the current puzzle's text with a progress bar or a countdown (issue #30).
    /// A puzzle in a zone whose gate has no such condition, e.g. the last zone, is shown on its own and ends with
    /// „Geschafft!“ (issue #32). In the zone of the level exit it shows „Alle Gegner besiegen“ while the exit is locked
    /// and „Zum Ausgang!“ once it is open. Gates with other conditions are not shown.
    /// </summary>
    public class MissionTracker : MonoBehaviour
    {
        [SerializeField] private HudConfig _config;
        [SerializeField] private Text _label;
        [Tooltip("Shown under the text for puzzles with progress.")]
        [SerializeField] private GameObject _progressBar;
        [Tooltip("Fill of the progress bar; its right anchor follows the progress.")]
        [SerializeField] private RectTransform _progressFill;
        [Tooltip("Zones of the level; the tracker follows the exit gate of the zone activated last.")]
        [SerializeField] private Zone[] _zones = System.Array.Empty<Zone>();
        [SerializeField] private SfxPlayer _sfx;
        [Tooltip("Played when the tracked gate opens.")]
        [SerializeField] private AudioClip _doneClip;
        [Tooltip("Puzzles of the level; one in a zone without a kill or puzzle gate is shown on its own.")]
        [SerializeField] private Puzzle[] _levelPuzzles = System.Array.Empty<Puzzle>();
        [Tooltip("Level exit; in its zone the tracker leads the player to it.")]
        [SerializeField] private LevelExit _exit;

        private Gate _gate;
        private KillGateCondition _kills;
        private PuzzleSolvedCondition _puzzles;
        private Puzzle _shownPuzzle;
        private Puzzle _loose;
        private Zone _looseZone;
        private string _shownText;
        private PuzzleDisplayKind _shownKind;
        private int _shownSeconds = -1;
        private int _shownKills = -1;
        private float _doneTime = -1f;
        private bool _tracksExit;

        /// <summary>Text currently shown; empty while hidden.</summary>
        public string Text => _label != null && _label.enabled ? _label.text : string.Empty;

        /// <summary>True while the progress bar shows.</summary>
        public bool ShowsProgress => _progressBar != null && _progressBar.activeSelf;

        /// <summary>Fill of the progress bar, 0..1.</summary>
        public float Progress => _progressFill != null ? _progressFill.anchorMax.x : 0f;

        private void OnEnable()
        {
            foreach (Zone zone in _zones)
            {
                if (zone != null)
                {
                    zone.Activated += Track;
                    if (zone.State == ZoneState.Active)
                    {
                        Track(zone);
                    }
                }
            }

            if (_gate == null)
            {
                Hide();
            }
        }

        private void Start()
        {
            if (_exit != null)
            {
                _exit.Opened += OnExitOpened;
            }
        }

        private void OnDestroy()
        {
            if (_exit != null)
            {
                _exit.Opened -= OnExitOpened;
            }
        }

        private void OnDisable()
        {
            foreach (Zone zone in _zones)
            {
                if (zone != null)
                {
                    zone.Activated -= Track;
                }
            }

            SetGate(null);
        }

        private void Update()
        {
            if (_doneTime >= 0f)
            {
                _doneTime += Time.deltaTime;
                if (_doneTime >= (_config != null ? _config.MissionDoneDuration : 0f))
                {
                    _doneTime = -1f;
                    Hide();
                    // The next puzzle of the same zone, if any, follows the confirmation.
                    Puzzle next = _looseZone != null ? LoosePuzzleIn(_looseZone) : null;
                    if (next != null)
                    {
                        SetLoose(next);
                        _shownPuzzle = null;
                    }
                }

                return;
            }

            if (_tracksExit)
            {
                ShowExit();
                return;
            }

            if (_kills != null && _gate.State == GateState.Closed && _kills.CurrentKills != _shownKills)
            {
                _shownKills = _kills.CurrentKills;
                Show(MissionText.KillGate(_shownKills, _kills.RequiredKills));
            }

            if (_puzzles != null && _gate.State == GateState.Closed)
            {
                ShowPuzzle(_puzzles.Current);
            }

            if (_loose != null)
            {
                ShowPuzzle(_loose);
            }
        }

        // Rebuilds the text only when puzzle, kind, text or shown seconds change; the bar follows every frame.
        // Puzzles cache their texts, so comparing them costs no allocation.
        private void ShowPuzzle(Puzzle puzzle)
        {
            if (puzzle == null)
            {
                return;
            }

            PuzzleDisplay display = puzzle.Display;
            int seconds = display.Kind == PuzzleDisplayKind.Countdown ? MissionText.CountdownSeconds(display.Value) : -1;
            if (puzzle != _shownPuzzle || display.Kind != _shownKind || seconds != _shownSeconds || !_label.enabled
                || !string.Equals(display.Text, _shownText, System.StringComparison.Ordinal))
            {
                _shownPuzzle = puzzle;
                _shownText = display.Text;
                _shownKind = display.Kind;
                _shownSeconds = seconds;
                Show(MissionText.Puzzle(display));
            }

            bool progress = display.Kind == PuzzleDisplayKind.Progress;
            SetProgressVisible(progress);
            if (progress && _progressFill != null)
            {
                _progressFill.anchorMax = new Vector2(display.Value, 1f);
            }
        }

        private void SetProgressVisible(bool visible)
        {
            if (_progressBar != null && _progressBar.activeSelf != visible)
            {
                _progressBar.SetActive(visible);
            }
        }

        private void Track(Zone zone)
        {
            Gate gate = zone.ExitGate;
            var kills = gate != null ? gate.Condition as KillGateCondition : null;
            var puzzles = gate != null ? gate.Condition as PuzzleSolvedCondition : null;
            if ((kills == null && puzzles == null) || gate.State != GateState.Closed)
            {
                Puzzle loose = LoosePuzzleIn(zone);
                if (loose != null)
                {
                    SetGate(null);
                    SetLoose(loose);
                    _looseZone = zone;
                    _shownPuzzle = null;
                    SetProgressVisible(false);
                    _doneTime = -1f;
                    return;
                }

                SetGate(null);
                if (_exit != null && _exit.Zone == zone && !_exit.IsReached)
                {
                    _tracksExit = true;
                    _doneTime = -1f;
                    SetProgressVisible(false);
                    ShowExit();
                    return;
                }

                // Keep a running „Tor offen!“ until it times out; otherwise nothing to show in this zone.
                if (_doneTime < 0f)
                {
                    Hide();
                }

                return;
            }

            SetGate(gate);
            _kills = kills;
            _puzzles = puzzles;
            _shownKills = -1;
            _shownPuzzle = null;
            SetProgressVisible(false);
            _doneTime = -1f;
        }

        private void SetGate(Gate gate)
        {
            if (_gate != null)
            {
                _gate.Opening -= OnGateOpening;
            }

            SetLoose(null);
            _looseZone = null;
            _tracksExit = false;
            _gate = gate;
            _kills = null;
            _puzzles = null;
            if (_gate != null)
            {
                _gate.Opening += OnGateOpening;
            }
        }

        private Puzzle LoosePuzzleIn(Zone zone)
        {
            foreach (Puzzle puzzle in _levelPuzzles)
            {
                if (puzzle != null && puzzle.Zone == zone && puzzle.State != PuzzleState.Solved)
                {
                    return puzzle;
                }
            }

            return null;
        }

        private void SetLoose(Puzzle puzzle)
        {
            if (_loose != null)
            {
                _loose.Solved -= OnLooseSolved;
            }

            _loose = puzzle;
            if (_loose != null)
            {
                _loose.Solved += OnLooseSolved;
            }
        }

        // The puzzle plays its own sound; the tracker only confirms in text.
        private void OnLooseSolved(Puzzle puzzle)
        {
            SetLoose(null);
            SetProgressVisible(false);
            Show(MissionText.PuzzleSolved);
            _doneTime = 0f;
        }

        private void OnGateOpening(Gate gate)
        {
            SetGate(null);
            SetProgressVisible(false);
            Show(MissionText.GateOpen);
            _doneTime = 0f;
            if (_sfx != null && _config != null)
            {
                _sfx.Play(_doneClip, _config.MissionDoneVolume, 0f);
            }
        }

        private void ShowExit()
        {
            if (_label == null)
            {
                return;
            }

            string text = _exit.IsOpen ? MissionText.ToExit : MissionText.ClearZone;
            if (!_label.enabled || !string.Equals(_label.text, text, System.StringComparison.Ordinal))
            {
                Show(text);
            }
        }

        private void OnExitOpened(LevelExit exit)
        {
            if (_tracksExit && _sfx != null && _config != null)
            {
                _sfx.Play(_doneClip, _config.MissionDoneVolume, 0f);
            }
        }

        private void Show(string text)
        {
            if (_label != null)
            {
                _label.text = text;
                _label.enabled = true;
            }
        }

        private void Hide()
        {
            if (_label != null)
            {
                _label.enabled = false;
            }

            SetProgressVisible(false);
        }
    }
}
