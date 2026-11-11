using System;

namespace RushOps.Scoring
{
    /// <summary>
    /// Follows one try of a level for the badges (issue #287, F-SCO-04): hearts lost, play time and the best multiplier.
    /// Healing does not undo a lost heart, and a multiplier once reached counts even after it is lost. The adapter only
    /// advances the time while the game runs, so a pause does not count.
    /// </summary>
    public sealed class BadgeTracker
    {
        /// <summary>Hearts lost in this try.</summary>
        public int HeartsLost { get; private set; }

        /// <summary>Seconds played in this try.</summary>
        public float Seconds { get; private set; }

        /// <summary>Highest multiplier reached in this try.</summary>
        public int BestMultiplier { get; private set; } = 1;

        /// <summary>Counts <paramref name="hearts"/> lost hearts; 0 or less is ignored.</summary>
        public void NotifyHeartsLost(int hearts)
        {
            if (hearts > 0)
            {
                HeartsLost += hearts;
            }
        }

        /// <summary>Remembers <paramref name="multiplier"/> if it is the highest so far.</summary>
        public void NotifyMultiplier(int multiplier)
        {
            BestMultiplier = Math.Max(BestMultiplier, multiplier);
        }

        /// <summary>Adds <paramref name="deltaTime"/> seconds of play; 0 or less is ignored.</summary>
        public void Advance(float deltaTime)
        {
            if (deltaTime > 0f)
            {
                Seconds += deltaTime;
            }
        }

        /// <summary>Starts a new try.</summary>
        public void Reset()
        {
            HeartsLost = 0;
            Seconds = 0f;
            BestMultiplier = 1;
        }

        /// <summary>Badges this try earns if it finished the level (see <see cref="BadgeRules.Earned"/>).</summary>
        public BadgeKind Earned(bool finished, float targetSeconds, int comboTarget)
        {
            return BadgeRules.Earned(finished, HeartsLost, Seconds, targetSeconds, BestMultiplier, comboTarget);
        }
    }
}
