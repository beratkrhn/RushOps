using System.Collections.Generic;
using UnityEngine;

namespace RushOps.Scoring
{
    /// <summary>
    /// Estimates the score of a level from what it sends (issue #301, F-SCO-03, N33): every expected kill is worth its
    /// points plus its coins, both times the average multiplier of a run. A solid run gives the threshold for 2 stars,
    /// a strong run the one for 3 stars.
    /// </summary>
    public static class StarScoreEstimator
    {
        /// <summary>Expected score with the average <paramref name="multiplier"/>.</summary>
        public static float Estimate(IReadOnlyList<KillTally> kills, int pointsPerCoin, float multiplier, float killShare,
            float coinShare)
        {
            float total = 0f;
            if (kills == null)
            {
                return total;
            }

            for (int i = 0; i < kills.Count; i++)
            {
                KillTally kill = kills[i];
                float worth = kill.Points + kill.Gold * coinShare * pointsPerCoin;
                total += kill.Count * killShare * worth * multiplier;
            }

            return total;
        }

        /// <summary>
        /// Proposed thresholds: x for 2 stars (solid run), y for 3 stars (strong run), rounded to the config's step;
        /// 3 stars always need more than 2.
        /// </summary>
        public static Vector2Int Thresholds(IReadOnlyList<KillTally> kills, int pointsPerCoin, StarEstimateConfig config)
        {
            float solid = Estimate(kills, pointsPerCoin, config.SolidMultiplier, config.KillShare, config.CoinShare);
            float strong = Estimate(kills, pointsPerCoin, config.StrongMultiplier, config.KillShare, config.CoinShare);
            return Thresholds(solid, strong, config.RoundTo);
        }

        /// <summary>Rounds the two estimates to <paramref name="step"/> and keeps 3 stars above 2.</summary>
        public static Vector2Int Thresholds(float solid, float strong, int step)
        {
            step = Mathf.Max(1, step);
            int two = Mathf.Max(step, Mathf.RoundToInt(solid / step) * step);
            int three = Mathf.Max(two + step, Mathf.RoundToInt(strong / step) * step);
            return new Vector2Int(two, three);
        }
    }
}
