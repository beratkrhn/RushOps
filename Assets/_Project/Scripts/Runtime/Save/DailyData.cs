using System;
using System.Collections.Generic;

namespace RushOps.Save
{
    /// <summary>
    /// Best daily runs in the save game (issue #276, version 5, F-DAT-04, F-END-14): one entry per day, newest first, the
    /// last <see cref="DailyRecords.MaxDays"/> days.
    /// </summary>
    [Serializable]
    public class DailyData
    {
        /// <summary>Best run per day, newest first (see <see cref="DailyRecords"/>).</summary>
        public List<DailyScore> Days = new List<DailyScore>();
    }
}
