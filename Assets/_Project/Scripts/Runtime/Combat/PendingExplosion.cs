using UnityEngine;

namespace RushOps.Combat
{
    /// <summary>An explosion waiting in an <see cref="ExplosionQueue"/>.</summary>
    public struct PendingExplosion
    {
        public PendingExplosion(Vector2 position, ExplosionDefinition definition, float remaining)
        {
            Position = position;
            Definition = definition;
            Remaining = remaining;
        }

        /// <summary>Center of the explosion.</summary>
        public Vector2 Position { get; }

        /// <summary>What explodes.</summary>
        public ExplosionDefinition Definition { get; }

        /// <summary>Seconds until it goes off.</summary>
        public float Remaining { get; set; }
    }
}
