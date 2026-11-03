using NUnit.Framework;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Finds the pool of one enemy type in the loaded scene; scenes have one pool per type.</summary>
    internal static class EnemyPools
    {
        public const string Walker = "Walker";
        public const string Runner = "Runner";
        public const string Brute = "Brute";

        /// <summary>The pool whose prefab uses the enemy definition named <paramref name="definitionName"/>.</summary>
        public static EnemyPool Of(string definitionName)
        {
            foreach (EnemyPool pool in Object.FindObjectsByType<EnemyPool>(FindObjectsSortMode.None))
            {
                if (pool.Definition != null && pool.Definition.name == definitionName)
                {
                    return pool;
                }
            }

            Assert.Fail($"scene has no EnemyPool for {definitionName}");
            return null;
        }
    }
}
