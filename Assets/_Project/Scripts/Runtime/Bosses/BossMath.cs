using System.Collections.Generic;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>Pure rules of boss fights (F-BOS-01, F-BOS-02, L5).</summary>
    public static class BossMath
    {
        /// <summary>Fill of the health bar, 0..1, for <paramref name="current"/> of <paramref name="max"/> hit points.</summary>
        public static float Fill(int current, int max)
        {
            if (max <= 0 || current <= 0)
            {
                return 0f;
            }

            return current >= max ? 1f : current / (float)max;
        }

        /// <summary>
        /// Direction of the direct chase (decision L5, no flow field): straight at the target, zero once closer than
        /// <paramref name="stopDistance"/>.
        /// </summary>
        public static Vector2 ChaseDirection(Vector2 from, Vector2 to, float stopDistance)
        {
            Vector2 delta = to - from;
            float sqr = delta.sqrMagnitude;
            if (sqr <= stopDistance * stopDistance || sqr < 1e-8f)
            {
                return Vector2.zero;
            }

            return delta / Mathf.Sqrt(sqr);
        }

        /// <summary>True for perks a boss may drop: not nuke or insta-kill, which do nothing against bosses (C9).</summary>
        public static bool CanDrop(PerkDefinition perk)
        {
            return perk != null && perk.Effect != PerkEffect.Nuke && perk.Effect != PerkEffect.InstaKill;
        }

        /// <summary>The perks of <paramref name="perks"/> a boss may drop.</summary>
        public static List<PerkDefinition> DroppablePerks(IReadOnlyList<PerkDefinition> perks)
        {
            var result = new List<PerkDefinition>();
            if (perks == null)
            {
                return result;
            }

            foreach (PerkDefinition perk in perks)
            {
                if (CanDrop(perk))
                {
                    result.Add(perk);
                }
            }

            return result;
        }
    }
}
