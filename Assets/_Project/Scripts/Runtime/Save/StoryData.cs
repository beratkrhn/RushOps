using System;

namespace RushOps.Save
{
    /// <summary>
    /// Story progress beyond the levels (issue #333, F-DAT-04): whether the intro was seen. Added without a version step;
    /// older saves load it as not seen, so they see the intro once.
    /// </summary>
    [Serializable]
    public class StoryData
    {
        /// <summary>True once the intro was played through or skipped.</summary>
        public bool IntroSeen;
    }
}
