using System;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>One wave of a <see cref="WaveDefinition"/>: its enemies by type and number (F-GEG-02).</summary>
    [Serializable]
    public class Wave
    {
        [SerializeField] private WaveEntry[] _entries = Array.Empty<WaveEntry>();

        /// <summary>Creates a wave of the given entries.</summary>
        public Wave(params WaveEntry[] entries)
        {
            _entries = entries ?? Array.Empty<WaveEntry>();
        }

        /// <summary>Enemy types and numbers; spawned alternately, one of each type in turn.</summary>
        public WaveEntry[] Entries => _entries ?? Array.Empty<WaveEntry>();

        /// <summary>Total number of enemies in the wave.</summary>
        public int TotalCount
        {
            get
            {
                int total = 0;
                foreach (WaveEntry entry in Entries)
                {
                    total += entry.Count;
                }

                return total;
            }
        }
    }
}
