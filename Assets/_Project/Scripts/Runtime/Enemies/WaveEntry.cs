using System;
using UnityEngine;

namespace RushOps.Enemies
{
    /// <summary>Part of a <see cref="Wave"/>: how many enemies of one type it brings.</summary>
    [Serializable]
    public struct WaveEntry
    {
        [SerializeField] private EnemyDefinition _enemy;
        [SerializeField, Min(0)] private int _count;

        /// <summary>Creates an entry of <paramref name="count"/> enemies of type <paramref name="enemy"/>.</summary>
        public WaveEntry(EnemyDefinition enemy, int count)
        {
            _enemy = enemy;
            _count = count;
        }

        /// <summary>Enemy type.</summary>
        public EnemyDefinition Enemy => _enemy;

        /// <summary>Number of enemies of this type.</summary>
        public int Count => _count > 0 ? _count : 0;
    }
}
