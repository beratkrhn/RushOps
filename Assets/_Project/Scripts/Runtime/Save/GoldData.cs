using System;

namespace RushOps.Save
{
    /// <summary>Gold in the save game (issue #236, version 4, F-GLD-05, F-DAT-04): the spendable balance and the gold earned over all time.</summary>
    [Serializable]
    public class GoldData
    {
        /// <summary>Gold the player can spend.</summary>
        public int Balance;

        /// <summary>Gold earned over all time (statistic), not lowered by spending.</summary>
        public int TotalEarned;
    }
}
