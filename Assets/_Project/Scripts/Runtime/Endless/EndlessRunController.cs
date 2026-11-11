using System;
using RushOps.Bosses;
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
    /// spawner config (F-GEG-03). Every boss round brings a boss of <see cref="EndlessBosses"/> with the health of the
    /// round instead (F-END-05); it ends when the boss is beaten. A dog round has dogs only, in packs (F-END-10). Pause banner, mission tracker and result screen are shown by the <see cref="EndlessHud"/>.
    /// </summary>
    public class EndlessRunController : MonoBehaviour
    {
        [SerializeField] private EndlessConfig _config;
        [SerializeField] private WaveDirector _director;
        [Tooltip("The zone over the whole arena; its waves are replaced by the rounds.")]
        [SerializeField] private Zone _zone;
        [SerializeField] private PlayerHealth _player;
        [SerializeField] private EndlessHud _hud;
        [Tooltip("The weapons on the arena wall, in the order of the config; locked until their round (F-END-07).")]
        [SerializeField] private WallWeapon[] _wallWeapons = Array.Empty<WallWeapon>();
        [Tooltip("Brings the boss of every boss round (F-END-05).")]
        [SerializeField] private EndlessBosses _bosses;
        [Tooltip("Starts a run in Start; off for tests that start it themselves.")]
        [SerializeField] private bool _autoStart = true;

        private EndlessRules _rules;
        private EndlessRun _run;
        private WaveSequence _sequence;
        private WaveDefinition _wave;
        private BossFight _fight;
        private int _shownRemaining = -1;
        private string _notice;
        private float _noticeRemaining;
        private bool _shownNotice;
        private RunUpgrades _upgrades;

        /// <summary>The run in progress, or null before the first <see cref="BeginRun"/>.</summary>
        public EndlessRun Run => _run;

        /// <summary>
        /// The upgrades of the running run (F-END-12); empty at the start of every run and never saved (E10). Created
        /// on first use over the upgrades of the config.
        /// </summary>
        public RunUpgrades Upgrades => _upgrades ??= new RunUpgrades(_config != null ? _config.Upgrades : null);

        /// <summary>The rules built on the config.</summary>
        public EndlessRules Rules => _rules;

        /// <summary>The round being played, or null during a pause and outside a run.</summary>
        public EndlessRound CurrentRound { get; private set; }

        /// <summary>The boss fight of the running boss round, or null.</summary>
        public BossFight CurrentFight => _fight;

        /// <summary>Raised when a round starts, with everything it brings.</summary>
        public event Action<EndlessRound> RoundStarted;

        /// <summary>Raised once when a run ends at 0 hearts, with the round reached (issue #211: autosave).</summary>
        public event Action<int> RunEnded;

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
            _fight = null;
            if (_bosses != null)
            {
                _bosses.Clear();
            }

            _shownRemaining = -1;
            _notice = null;
            _noticeRemaining = 0f;
            _shownNotice = false;
            // A new run starts without upgrades (F-END-12); this also drops their extra hearts before the run reads them.
            Upgrades.Clear();
            // Weapons of the rounds before the first one hang unlocked; the first round's own weapon unlocks with it.
            ApplyWallWeapons(firstRound - 1, false);
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
            if (_run.Phase != EndlessPhase.Running || (_sequence == null && _fight == null))
            {
                return;
            }

            // A boss round ends with the boss, whatever its wave (it has none) or the brood do.
            bool finished = _fight != null ? _fight.IsWon : _sequence.Phase == WavePhase.Finished;
            if (finished)
            {
                // The boss or the last enemy of the round died (or the round had none).
                _run.NotifyRoundCleared();
                return;
            }

            if (_noticeRemaining > 0f)
            {
                _noticeRemaining -= Time.deltaTime;
            }

            int remaining = _fight != null ? 0 : _sequence.Remaining;
            bool notice = _noticeRemaining > 0f && _notice != null;
            if (remaining != _shownRemaining || notice != _shownNotice)
            {
                _shownRemaining = remaining;
                _shownNotice = notice;
                string shown = notice ? _notice : null;
                _hud.SetTracker(_fight != null
                    ? EndlessText.BossTracker(_run.Round, shown)
                    : CurrentRound != null && CurrentRound.IsDogRound
                        ? EndlessText.DogTracker(_run.Round, remaining, shown)
                        : EndlessText.Tracker(_run.Round, remaining, shown));
            }
        }

        private void OnPauseStarted(int round)
        {
            CurrentRound = null;
            _hud.SetTracker(string.Empty);
            _hud.ShowBanner(EndlessText.Round(round), _rules.IsDogRound(round) ? EndlessText.DogRound : null);
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
            _fight = built.IsBossRound && _bosses != null
                ? _bosses.Spawn(built.Boss, _rules.BossHealth(built.Boss.Health, round))
                : null;
            _shownRemaining = -1;
            ApplyWallWeapons(round, true);
            _notice = built.NewWeapon != null ? EndlessText.NewWeapon(built.NewWeapon.DisplayName) : null;
            _noticeRemaining = _notice != null ? _config.NewWeaponNoticeSeconds : 0f;
            RoundStarted?.Invoke(built);
        }

        // Locks every wall weapon whose round has not come; with glow, the ones that just became available light up.
        private void ApplyWallWeapons(int round, bool glow)
        {
            foreach (WallWeapon wall in _wallWeapons)
            {
                if (wall == null)
                {
                    continue;
                }

                if (!_rules.IsWeaponAvailable(wall.Weapon, round))
                {
                    wall.SetLocked(true);
                }
                else if (glow)
                {
                    wall.Unlock();
                }
                else
                {
                    wall.SetLocked(false);
                }
            }
        }

        private void OnRoundCleared(int round, int hearts)
        {
            if (_fight != null)
            {
                _bosses.Finish();
                _fight = null;
            }

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
            _fight = null;
            if (_bosses != null)
            {
                _bosses.Finish();
            }

            _director.StopWaves(_zone);
            _hud.SetTracker(string.Empty);
            _hud.HideBanner();
            _hud.ReportRound(round);
            RunEnded?.Invoke(round);
        }

        private void OnHealthChanged(int current, int max)
        {
            if (_run == null)
            {
                return;
            }

            if (max != _run.MaxHearts)
            {
                _run.NotifyMaxHearts(max, current);
            }
            else
            {
                _run.NotifyHearts(current);
            }
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
