namespace RushOps.Bosses
{
    /// <summary>
    /// When a boss drops perks (decision C9): once per threshold, when its health share falls to or below it. A big
    /// hit that passes two thresholds drops two perks. Thresholds are expected from high to low.
    /// </summary>
    public class BossPerkThresholds
    {
        private readonly float[] _thresholds;
        private int _passed;

        /// <summary>Creates the rule for <paramref name="thresholds"/> (health shares 0..1, high to low).</summary>
        public BossPerkThresholds(float[] thresholds)
        {
            _thresholds = thresholds ?? System.Array.Empty<float>();
        }

        /// <summary>Thresholds passed so far.</summary>
        public int Passed => _passed;

        /// <summary>
        /// Reports the boss's health as <paramref name="current"/> of <paramref name="max"/>; returns how many perks
        /// drop now. A dead boss drops none, its death ends the fight.
        /// </summary>
        public int Advance(int current, int max)
        {
            if (max <= 0 || current <= 0)
            {
                return 0;
            }

            float share = current / (float)max;
            int drops = 0;
            while (_passed < _thresholds.Length && share <= _thresholds[_passed])
            {
                _passed++;
                drops++;
            }

            return drops;
        }
    }
}
