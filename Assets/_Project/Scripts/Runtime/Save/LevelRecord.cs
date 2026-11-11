using System;

namespace RushOps.Save
{
    /// <summary>Saved result of one level (F-DAT-04, F-SCO-03).</summary>
    [Serializable]
    public class LevelRecord
    {
        /// <summary>Stable id of the level, e.g. its scene name.</summary>
        public string LevelId = "";

        /// <summary>True once the level was finished.</summary>
        public bool Completed;

        /// <summary>Best stars reached, 0 to 3.</summary>
        public int Stars;

        /// <summary>Best score reached.</summary>
        public int BestScore;

        /// <summary>Badges earned, a <see cref="Scoring.BadgeKind"/> as int (issue #287, version 6).</summary>
        public int Badges;
    }
}
