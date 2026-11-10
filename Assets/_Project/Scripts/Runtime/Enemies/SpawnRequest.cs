namespace RushOps.Enemies
{
    /// <summary>
    /// Enemies waiting to be spawned together at one spawn point: their type, how many (more than one only for packs,
    /// F-GEG-05) and the index of the zone that asked for them.
    /// </summary>
    public readonly struct SpawnRequest
    {
        /// <summary>Creates a request of zone <paramref name="owner"/> for one enemy of type <paramref name="enemy"/>.</summary>
        public SpawnRequest(int owner, EnemyDefinition enemy)
            : this(owner, enemy, 1)
        {
        }

        /// <summary>Creates a request of zone <paramref name="owner"/> for <paramref name="count"/> enemies of type <paramref name="enemy"/>.</summary>
        public SpawnRequest(int owner, EnemyDefinition enemy, int count)
        {
            Owner = owner;
            Enemy = enemy;
            Count = count > 1 ? count : 1;
        }

        /// <summary>Index of the zone that asked for the enemies.</summary>
        public int Owner { get; }

        /// <summary>Enemy type.</summary>
        public EnemyDefinition Enemy { get; }

        /// <summary>Enemies that appear together, at least 1.</summary>
        public int Count { get; }

        /// <summary>True for bosses, which do not count towards the enemy limit.</summary>
        public bool IsBoss => Enemy != null && Enemy.IsBoss;

        /// <summary>Most enemies of this type that spawn together: the pack size, 1 for most types.</summary>
        public int PackSize => Enemy != null ? Enemy.PackSize : 1;

        /// <summary>True while more members can join, i.e. a pack that is not full yet.</summary>
        public bool CanGrow => Count < PackSize;

        /// <summary>Places of the enemy limit the request takes: 0 for bosses, otherwise one per enemy (H4).</summary>
        public int SlotCost => IsBoss ? 0 : Count;

        /// <summary>The same request with one more enemy.</summary>
        public SpawnRequest Grow()
        {
            return new SpawnRequest(Owner, Enemy, Count + 1);
        }
    }
}
