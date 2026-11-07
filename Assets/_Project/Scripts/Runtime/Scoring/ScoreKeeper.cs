using System;

namespace RushOps.Scoring
{
    /// <summary>
    /// Score with a combo multiplier (F-SCO-01, F-SCO-02, issue #28). Every kill adds its points times the current
    /// multiplier. Kills within the combo window after the previous kill keep the combo; every
    /// <c>killsPerStep</c> combo kills raise the multiplier by 1, up to the maximum, so with 5 kills per step the
    /// kills 1 to 5 count ×1 and the 6th counts ×2. A hit on the player or a run-out window resets it to ×1.
    /// </summary>
    public sealed class ScoreKeeper
    {
        private readonly float _comboWindow;
        private readonly int _killsPerStep;
        private readonly int _maxMultiplier;

        /// <param name="comboWindow">Seconds after a kill in which the next kill keeps the combo.</param>
        /// <param name="killsPerStep">Combo kills per multiplier step, at least 1.</param>
        /// <param name="maxMultiplier">Highest multiplier, at least 1.</param>
        public ScoreKeeper(float comboWindow, int killsPerStep, int maxMultiplier)
        {
            _comboWindow = comboWindow > 0f ? comboWindow : 0f;
            _killsPerStep = killsPerStep > 1 ? killsPerStep : 1;
            _maxMultiplier = maxMultiplier > 1 ? maxMultiplier : 1;
        }

        /// <summary>Raised when <see cref="Score"/> changes, with the new score.</summary>
        public event Action<int> ScoreChanged;

        /// <summary>Raised when <see cref="Multiplier"/> changes, with the new multiplier.</summary>
        public event Action<int> MultiplierChanged;

        /// <summary>Points since level start.</summary>
        public int Score { get; private set; }

        /// <summary>Current multiplier, 1 to the maximum.</summary>
        public int Multiplier { get; private set; } = 1;

        /// <summary>Highest multiplier reached since level start (design question M6, best combo).</summary>
        public int BestMultiplier { get; private set; } = 1;

        /// <summary>Kills in the running combo.</summary>
        public int ComboKills { get; private set; }

        /// <summary>Seconds left before the combo runs out; 0 without a combo.</summary>
        public float ComboRemaining { get; private set; }

        /// <summary>Adds a kill worth <paramref name="points"/> and extends the combo.</summary>
        /// <returns>The points added, i.e. <paramref name="points"/> times the multiplier before this kill.</returns>
        public int RegisterKill(int points)
        {
            int awarded = points > 0 ? points * Multiplier : 0;
            if (awarded > 0)
            {
                Score += awarded;
                ScoreChanged?.Invoke(Score);
            }

            ComboKills++;
            ComboRemaining = _comboWindow;
            SetMultiplier(Math.Min(1 + ComboKills / _killsPerStep, _maxMultiplier));
            return awarded;
        }

        /// <summary>
        /// Adds <paramref name="points"/> as they are, without multiplier and without touching the combo, e.g. for
        /// beating a boss (decision C10).
        /// </summary>
        public void AddPoints(int points)
        {
            if (points <= 0)
            {
                return;
            }

            Score += points;
            ScoreChanged?.Invoke(Score);
        }

        /// <summary>
        /// Adds picked-up gold worth <paramref name="points"/> times the current multiplier (F-SCO-02). It takes no step
        /// towards the next multiplier. With <paramref name="restartWindow"/> it restarts the window of a running combo;
        /// an already ended combo stays ended.
        /// </summary>
        /// <returns>The points added.</returns>
        public int RegisterGold(int points, bool restartWindow)
        {
            if (points <= 0)
            {
                return 0;
            }

            int awarded = points * Multiplier;
            Score += awarded;
            ScoreChanged?.Invoke(Score);
            if (restartWindow && ComboKills > 0)
            {
                ComboRemaining = _comboWindow;
            }

            return awarded;
        }

        /// <summary>Ends the combo, e.g. when the player takes a hit.</summary>
        public void BreakCombo()
        {
            ComboKills = 0;
            ComboRemaining = 0f;
            SetMultiplier(1);
        }

        /// <summary>Advances the combo window by <paramref name="deltaTime"/> seconds; a run-out window ends the combo.</summary>
        public void Tick(float deltaTime)
        {
            if (ComboKills == 0 || deltaTime <= 0f)
            {
                return;
            }

            ComboRemaining -= deltaTime;
            if (ComboRemaining <= 0f)
            {
                BreakCombo();
            }
        }

        private void SetMultiplier(int multiplier)
        {
            if (multiplier == Multiplier)
            {
                return;
            }

            Multiplier = multiplier;
            if (multiplier > BestMultiplier)
            {
                BestMultiplier = multiplier;
            }

            MultiplierChanged?.Invoke(Multiplier);
        }
    }
}
