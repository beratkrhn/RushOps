using System;
using System.Collections.Generic;

namespace RushOps.Save
{
    /// <summary>
    /// Best scores of the endless mode in the save game (issue #211, version 3, F-DAT-04, F-END-08): the local top 10,
    /// the number of runs and the highest round reached.
    /// </summary>
    [Serializable]
    public class EndlessData
    {
        /// <summary>The best runs, best first (see <see cref="EndlessRecords"/>).</summary>
        public List<EndlessScore> Top = new List<EndlessScore>();

        /// <summary>Endless runs played to the end.</summary>
        public int Runs;

        /// <summary>Highest round any run reached.</summary>
        public int HighestRound;
    }
}
