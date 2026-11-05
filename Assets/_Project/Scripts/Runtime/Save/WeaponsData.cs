using System;
using System.Collections.Generic;

namespace RushOps.Save
{
    /// <summary>Weapons in the save game (issue #176, decisions W4, W5): the chosen weapon and the unlocked ones.</summary>
    [Serializable]
    public class WeaponsData
    {
        /// <summary>Id of the chosen weapon; empty for the start weapon.</summary>
        public string Current = "";

        /// <summary>Ids of the weapons unlocked by beating a boss; weapons available from the start are not listed.</summary>
        public List<string> Unlocked = new List<string>();
    }
}
