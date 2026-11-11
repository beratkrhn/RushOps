using System;

namespace RushOps.Endless
{
    /// <summary>
    /// The course of one endless run (F-END-03, F-END-06, F-END-08, F-END-11): pause with „Runde X“ → round running →
    /// round cleared (+hearts) → upgrade choice after some rounds → pause before the next round … until the player has
    /// no hearts left. Knows nothing about enemies; the
    /// caller reports cleared rounds and the player's hearts.
    /// </summary>
    public class EndlessRun
    {
        private readonly float _pauseDuration;
        private readonly int _heartsPerRound;
        private float _timer;

        /// <summary>
        /// Creates an idle run with <paramref name="hearts"/> of <paramref name="maxHearts"/> hearts, using pause and
        /// hearts per round of <paramref name="config"/>.
        /// </summary>
        public EndlessRun(EndlessConfig config, int hearts, int maxHearts)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _pauseDuration = config.PauseBetweenRounds;
            _heartsPerRound = config.HeartsPerRound;
            MaxHearts = Math.Max(0, maxHearts);
            Hearts = Math.Min(Math.Max(0, hearts), MaxHearts);
        }

        /// <summary>Current phase.</summary>
        public EndlessPhase Phase { get; private set; } = EndlessPhase.Idle;

        /// <summary>The round being played, the next one during a pause, the cleared one during a choice; 0 before the start.</summary>
        public int Round { get; private set; }

        /// <summary>Rounds cleared in this run.</summary>
        public int RoundsCleared { get; private set; }

        /// <summary>Hearts as last reported or given.</summary>
        public int Hearts { get; private set; }

        /// <summary>Most hearts the player can have.</summary>
        public int MaxHearts { get; private set; }

        /// <summary>Seconds left of the current pause; 0 outside a pause.</summary>
        public float PauseRemaining => Phase == EndlessPhase.Pause ? Math.Max(0f, _timer) : 0f;

        /// <summary>Raised when the pause before a round starts, with the round number to show („Runde X“).</summary>
        public event Action<int> PauseStarted;

        /// <summary>Raised when a round starts, with its number.</summary>
        public event Action<int> RoundStarted;

        /// <summary>Raised when a round was cleared, with its number and the hearts given back (0 at full health).</summary>
        public event Action<int, int> RoundCleared;

        /// <summary>Raised when the upgrade choice after a cleared round starts, with that round.</summary>
        public event Action<int> ChoiceStarted;

        /// <summary>Raised once when the run ends, with the round reached.</summary>
        public event Action<int> Ended;

        /// <summary>
        /// Starts the run with the pause before <paramref name="firstRound"/> (1 normally; higher from the test menu).
        /// Does nothing unless idle. Ends right away if the player has no hearts.
        /// </summary>
        public void Start(int firstRound = 1)
        {
            if (Phase != EndlessPhase.Idle)
            {
                return;
            }

            if (Hearts <= 0)
            {
                Round = Math.Max(1, firstRound);
                End();
                return;
            }

            BeginPause(Math.Max(1, firstRound));
        }

        /// <summary>Advances the pause by <paramref name="deltaTime"/> seconds; starts the round once it is over.</summary>
        public void Tick(float deltaTime)
        {
            if (Phase != EndlessPhase.Pause)
            {
                return;
            }

            _timer -= deltaTime;
            if (_timer > 0f)
            {
                return;
            }

            _timer = 0f;
            Phase = EndlessPhase.Running;
            RoundStarted?.Invoke(Round);
        }

        /// <summary>
        /// Reports that all enemies of the running round are beaten: gives hearts up to the maximum, then either waits for
        /// an upgrade choice (<paramref name="offerChoice"/>, see <see cref="FinishChoice"/>) or pauses before the next
        /// round. Returns the hearts given; 0 if no round was running.
        /// </summary>
        public int NotifyRoundCleared(bool offerChoice = false)
        {
            if (Phase != EndlessPhase.Running)
            {
                return 0;
            }

            int gained = Math.Max(0, Math.Min(_heartsPerRound, MaxHearts - Hearts));
            Hearts += gained;
            RoundsCleared++;
            int cleared = Round;
            RoundCleared?.Invoke(cleared, gained);
            if (Phase != EndlessPhase.Running)
            {
                // The hearts given ended nothing, but a listener may have ended the run.
                return gained;
            }

            if (offerChoice)
            {
                Phase = EndlessPhase.Choice;
                ChoiceStarted?.Invoke(cleared);
            }
            else
            {
                BeginPause(cleared + 1);
            }

            return gained;
        }

        /// <summary>Ends the upgrade choice and pauses before the next round; does nothing outside a choice.</summary>
        public void FinishChoice()
        {
            if (Phase == EndlessPhase.Choice)
            {
                BeginPause(Round + 1);
            }
        }

        /// <summary>
        /// Reports a new maximum and the hearts with it, e.g. after the +1 heart upgrade (F-END-12). Hearts above the new
        /// maximum are cut; at 0 hearts the run ends.
        /// </summary>
        public void NotifyMaxHearts(int maxHearts, int hearts)
        {
            MaxHearts = Math.Max(0, maxHearts);
            NotifyHearts(hearts);
        }

        /// <summary>Reports the player's hearts, e.g. after a hit or a heart perk; at 0 the run ends.</summary>
        public void NotifyHearts(int hearts)
        {
            if (Phase == EndlessPhase.Ended)
            {
                return;
            }

            Hearts = Math.Min(Math.Max(0, hearts), MaxHearts);
            if (Hearts == 0 && Phase != EndlessPhase.Idle)
            {
                End();
            }
        }

        private void BeginPause(int round)
        {
            Round = round;
            _timer = _pauseDuration;
            Phase = EndlessPhase.Pause;
            PauseStarted?.Invoke(round);
        }

        private void End()
        {
            Phase = EndlessPhase.Ended;
            Ended?.Invoke(Round);
        }
    }
}
