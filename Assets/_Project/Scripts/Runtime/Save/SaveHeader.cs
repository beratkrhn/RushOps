using System;

namespace RushOps.Save
{
    /// <summary>Only the version of a save file, read before the file is migrated and parsed as a whole.</summary>
    [Serializable]
    internal class SaveHeader
    {
        public int Version;
    }
}
