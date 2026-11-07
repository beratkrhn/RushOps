using System;
using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Levels;
using RushOps.Player;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// Thin adapter that plays an endless run in the arena (F-END-03, F-END-04, F-END-06, F-END-08): it feeds the
    /// <see cref="EndlessRun"/> logic with the player's hearts and the cleared rounds, and turns every round of the
    /// <see cref="EndlessRules"/> into one wave of the arena's zone that the <see cref="WaveDirector"/> plays from the
    /// spawn points of all four gates (ADR 0013). The cap of enemies alive at once comes from the round, not from the
    /// spawner config (F-GEG-03). Pause banner, mission tracker and result screen are shown by the <see cref="EndlessHud"/>.
    /// </summary>
    public class EndlessRunController : MonoBehaviour
    {
        [SerializeField] private EndlessConfig _config;
        [SerializeField] private WaveDirector _director;
        [Tooltip("The zone over the whole arena; its waves are replaced by the rounds.")]
        [SerializeField] private Zone _zone;
        [SerializeField] private PlayerHealth _player;
        [SerializeField] private EndlessHud _hud;
        [Tooltip("Starts a run in Start; off for tests that start it themselves.")]
        [SerializeField] private bool _autoStart = true;

        private EndlessRules _rules;
        private EndlessRun _run;
        private WaveSequence _sequence;
        private WaveDefinition _wave;
        private int _shownRemaining = -1;

        /// <summary>The run in progress, or null before the first <see cref="BeginRun"/>.</summary>
        public EndlessRun Run => _run;

        /// <summary>The rules built on the config.</summary>
        public EndlessRules Rules => _rules;

        /// <summary>The round being played, or null during a pause and outside a run.</summary>
        public EndlessRound CurrentRound { get; private set; }

        /// <summary>Raised when a round starts, with everything it brings.</summary>
        public event Action<EndlessRound> RoundStarted;

        /// <summary>
        /// Starts a new run with the pause before <paramref name="firstRound"/> and drops the one in progress. The player's
        /// hearts at this moment are the hearts of the run.
        /// </summary>
        public void BeginRun(int firstRound)
        {
            Unsubscribe();
            if (_rules == null || _rules.Config != _config)
            {
                _rules = new EndlessRules(_config);
            }

            _director.StopWaves(_zone);
            CurrentRound = null;
            _sequence = null;
            _shownRemaining = -1;
            // A run always starts with the pistol, whatever weapon the menu has selected (F-END-07).
            if (_config.StartWeapon != null && _player.TryGetComponent(out PlayerWeapon weapon))
            {
                weapon.Slots.Reset(_config.StartWeapon);
            }

            _run = new EndlessRun(_config, _player.Current, _player.Max);
            _run.PauseStarted += OnPauseStarted;
            _run.RoundStarted += OnRoundStarted;
            _run.RoundCleared += OnRoundCleared;
            _run.Ended += OnEnded;
            _player.HealthChanged += OnHealthChanged;
            _run.Start(firstRound);
        }

        private void Start()
        {
            if (_autoStart)
            {
                BeginRun(EndlessStart.Consume());
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
            DestroyWave();
        }

        private void Update()
        {
            if (_run == null)
            {
                return;
            }

            _run.Tick(Time.deltaTime);
            if (_run.Phase != EndlessPhase.Running || _sequence == null)
            {
                return;
            }

            if (_sequence.Phase == WavePhase.Finished)
            {
                // The last enemy of the round died (or the round had none).
                _run.NotifyRoundCleared();
                return;
            }

            int remaining = _sequence.Remaining;
            if (remaining != _shownRemaining)
            {
                _shownRemaining = remaining;
                _hud.SetTracker(EndlessText.Tracker(_run.Round, remaining));
            }
        }

        private void OnPauseStarted(int round)
        {
            CurrentRound = null;
            _hud.SetTracker(string.Empty);
            _hud.ShowBanner(EndlessText.Round(round));
        }

        private void OnRoundStarted(int round)
        {
            EndlessRound built = _rules.Build(round);
            CurrentRound = built;
            _hud.HideBanner();
            _director.SetMaxActive(built.MaxActive);
            WaveDefinition previous = _wave;
            _wave = _rules.CreateWave(round);
            _director.UseWaves(_zone, _wave);
            DestroyWave(previous);
            _sequence = _director.SequenceOf(_zone);
            _shownRemaining = -1;
            RoundStarted?.Invoke(built);
        }

        private void OnRoundCleared(int round, int hearts)
        {
            _sequence = null;
            CurrentRound = null;
            if (hearts > 0)
            {
                _player.Heal(hearts);
            }
        }

        private void OnEnded(int round)
        {
            CurrentRound = null;
            _sequence = null;
            _director.StopWaves(_zone);
            _hud.SetTracker(string.Empty);
            _hud.HideBanner();
            _hud.ReportRound(round);
        }

        private void OnHealthChanged(int current, int max)
        {
            _run?.NotifyHearts(current);
        }

        private void Unsubscribe()
        {
            if (_run != null)
            {
                _run.PauseStarted -= OnPauseStarted;
                _run.RoundStarted -= OnRoundStarted;
                _run.RoundCleared -= OnRoundCleared;
                _run.Ended -= OnEnded;
            }

            if (_player != null)
            {
                _player.HealthChanged -= OnHealthChanged;
            }
        }

        private void DestroyWave()
        {
            DestroyWave(_wave);
            _wave = null;
        }

        private static void DestroyWave(WaveDefinition wave)
        {
            if (wave != null)
            {
                Destroy(wave);
            }
        }
    }
}
