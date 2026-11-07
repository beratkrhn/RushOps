using System;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>A weapon on the arena wall and the round from whose start on it can be picked up (F-END-07, N4).</summary>
    [Serializable]
    public struct EndlessWeaponRound
    {
        [Tooltip("Round from whose start on the weapon is available.")]
        [SerializeField, Min(1)] private int _round;

        [Tooltip("Weapon on the wall.")]
        [SerializeField] private WeaponDefinition _weapon;

        /// <summary>Creates an entry.</summary>
        public EndlessWeaponRound(int round, WeaponDefinition weapon)
        {
            _round = Math.Max(1, round);
            _weapon = weapon;
        }

        /// <summary>Round from whose start on the weapon is available.</summary>
        public int Round => Math.Max(1, _round);

        /// <summary>Weapon on the wall.</summary>
        public WeaponDefinition Weapon => _weapon;
    }
}
