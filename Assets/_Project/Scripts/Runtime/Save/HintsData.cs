using System;
using System.Collections.Generic;

namespace RushOps.Save
{
    /// <summary>Puzzle hints already shown (issue #287, version 6, F-RAE-13, N29).</summary>
    [Serializable]
    public class HintsData
    {
        /// <summary>Ids of the puzzle kinds whose explaining text was shown.</summary>
        public List<string> Seen = new List<string>();
    }
}
