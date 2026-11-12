using System;

namespace RushOps.Save
{
    /// <summary>
    /// Saved state of the arcade mode (F-ARC-08, F-DAT-04, since save version 7): whether the endless area shows the
    /// run in the cabinet, and the own top 10 and daily bests of arcade runs, which never mix with the normal ones
    /// because the arcade screen shows less of the arena (AR6). Runs, highest round, gold and statistics of arcade
    /// runs still count in <see cref="SaveData.Endless"/> and the other sections as for every run.
    /// </summary>
    [Serializable]
    public class ArcadeData
    {
        /// <summary>„Darstellung: Arcade“ is chosen in the endless area; default „Normal“.</summary>
        public bool Selected;

        /// <summary>Top 10 of the arcade runs; its run count is only bookkeeping of this list.</summary>
        public EndlessData Endless = new EndlessData();

        /// <summary>Best daily runs played in the arcade.</summary>
        public DailyData Daily = new DailyData();
    }
}
