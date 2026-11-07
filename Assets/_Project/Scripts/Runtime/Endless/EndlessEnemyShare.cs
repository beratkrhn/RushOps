using System;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// One row of the enemy mix table in <see cref="EndlessConfig"/> (F-END-04): from which round a heavier enemy type
    /// joins and how its share of a round's enemies grows from there. The rest of a round are walkers.
    /// </summary>
    [Serializable]
    public struct EndlessEnemyShare
    {
        [Tooltip("Enemy type.")]
        [SerializeField] private EnemyDefinition _enemy;

        [Tooltip("First round in which this type comes.")]
        [SerializeField, Min(1)] private int _firstRound;

        [Tooltip("Share of the round's enemies in its first round, 0 to 1.")]
        [SerializeField, Range(0f, 1f)] private float _startShare;

        [Tooltip("Share added with every further round.")]
        [SerializeField, Min(0f)] private float _sharePerRound;

        [Tooltip("Largest share this type reaches.")]
        [SerializeField, Range(0f, 1f)] private float _maxShare;

        /// <summary>Creates a row; values are clamped like in the inspector.</summary>
        public EndlessEnemyShare(EnemyDefinition enemy, int firstRound, float startShare, float sharePerRound, float maxShare)
        {
            _enemy = enemy;
            _firstRound = Math.Max(1, firstRound);
            _startShare = Mathf.Clamp01(startShare);
            _sharePerRound = Mathf.Max(0f, sharePerRound);
            _maxShare = Mathf.Clamp01(maxShare);
        }

        /// <summary>Enemy type.</summary>
        public EnemyDefinition Enemy => _enemy;

        /// <summary>First round in which this type comes.</summary>
        public int FirstRound => Math.Max(1, _firstRound);

        /// <summary>Share in its first round.</summary>
        public float StartShare => _startShare;

        /// <summary>Share added per further round.</summary>
        public float SharePerRound => _sharePerRound;

        /// <summary>Largest share.</summary>
        public float MaxShare => _maxShare;
    }
}
