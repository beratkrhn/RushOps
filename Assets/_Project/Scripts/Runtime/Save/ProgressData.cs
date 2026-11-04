using System;
using System.Collections.Generic;

namespace RushOps.Save
{
    /// <summary>Level progress in the save game (F-DAT-04): one record per level that was played to the end.</summary>
    [Serializable]
    public class ProgressData
    {
        /// <summary>Records of the finished levels.</summary>
        public List<LevelRecord> Levels = new List<LevelRecord>();
    }
}
