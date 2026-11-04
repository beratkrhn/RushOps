using System;
using System.Collections.Generic;

namespace RushOps.Save
{
    /// <summary>Skins in the save game (issue #128, F-DAT-04): the chosen skin and the unlocked ones.</summary>
    [Serializable]
    public class SkinsData
    {
        /// <summary>Id of the chosen skin; empty for the default skin.</summary>
        public string Current = "";

        /// <summary>Ids of the skins unlocked by playing; default skins are not listed.</summary>
        public List<string> Unlocked = new List<string>();
    }
}
