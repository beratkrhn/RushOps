namespace RushOps.Enemies
{
    /// <summary>An enemy waiting to be spawned: its type and the index of the zone that asked for it.</summary>
    public readonly struct SpawnRequest
    {
        /// <summary>Creates a request of zone <paramref name="owner"/> for an enemy of type <paramref name="enemy"/>.</summary>
        public SpawnRequest(int owner, EnemyDefinition enemy)
        {
            Owner = owner;
            Enemy = enemy;
        }

        /// <summary>Index of the zone that asked for the enemy.</summary>
        public int Owner { get; }

        /// <summary>Enemy type.</summary>
        public EnemyDefinition Enemy { get; }

        /// <summary>True for bosses, which do not count towards the enemy limit.</summary>
        public bool IsBoss => Enemy != null && Enemy.IsBoss;
    }
}
