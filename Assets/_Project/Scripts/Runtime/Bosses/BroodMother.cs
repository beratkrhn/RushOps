using System;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// Boss of level 2-3 (issue #155, Lastenheft 2.4): a slow, bloated mother that walks at the player and gives birth
    /// to a runner every few seconds, more often below half her health (C8). Her runners come from the level's runner
    /// pool, so they count towards the most enemies at once (F-GEG-03) and give points and perk drops like any other.
    /// Rules in <see cref="BroodMotherLogic"/>, values in <see cref="BroodMotherDefinition"/>.
    /// </summary>
    public class BroodMother : Boss
    {
        [Tooltip("Pool her runners come from; the level's runner pool.")]
        [SerializeField] private EnemyPool _brood;
        [Tooltip("Counts the enemies in the level; a birth waits while it has no free slot. Empty = no limit.")]
        [SerializeField] private WaveDirector _director;
        [Tooltip("Layers a runner must not appear in, usually the walls.")]
        [SerializeField] private LayerMask _wallLayers;
        [Tooltip("A runner appears this far from her feet towards the player.")]
        [SerializeField, Min(0f)] private float _birthDistance = 1.1f;
        [Tooltip("Radius kept clear of walls where a runner appears.")]
        [SerializeField, Min(0f)] private float _birthClearance = 0.3f;

        private BroodMotherLogic _logic;

        /// <summary>Raised when a runner came out of her.</summary>
        public event Action<BroodMother, Enemy> Birthed;

        /// <summary>Current phase.</summary>
        public BroodMotherPhase Phase => Logic.Phase;

        /// <summary>Runners born in this fight.</summary>
        public int BornCount { get; private set; }

        /// <summary>Seconds from one runner to the next at her current health.</summary>
        public float CurrentInterval => Logic.Interval(HealthShare);

        private BroodMotherDefinition Values => Definition as BroodMotherDefinition;

        private BroodMotherLogic Logic => _logic ??= Values != null
            ? new BroodMotherLogic(Values.SpawnInterval, Values.EnragedSpawnInterval, Values.EnragedHealthShare, Values.BirthTime)
            : new BroodMotherLogic(3f, 2f, 0.5f, 0.6f);

        private float HealthShare => MaxHealth > 0 ? CurrentHealth / (float)MaxHealth : 0f;

        private bool HasRoom => _brood != null && (_director == null || _director.FreeSlots > 0);

        /// <inheritdoc />
        protected override void TickBehaviour(float deltaTime)
        {
            BroodMotherPhase before = Logic.Phase;
            bool born = Logic.Tick(deltaTime, HealthShare, HasRoom);
            if (Logic.Phase != before && View != null)
            {
                View.SetAlert(Logic.Phase == BroodMotherPhase.Birth);
            }

            if (born)
            {
                GiveBirth();
            }

            if (Logic.Phase == BroodMotherPhase.Walk)
            {
                Chase(Definition != null ? Definition.MoveSpeed : 0f);
            }
            else
            {
                Body.linearVelocity = Vector2.zero;
            }
        }

        private void GiveBirth()
        {
            Vector2 feet = Body.position;
            Vector2 toTarget = Target != null ? (Vector2)Target.position - feet : Vector2.zero;
            Vector2 direction = toTarget.sqrMagnitude > 1e-4f ? toTarget.normalized : Vector2.down;
            Vector2 spot = feet + direction * Mathf.Min(_birthDistance, toTarget.magnitude);
            if (Physics2D.OverlapCircle(spot, _birthClearance, _wallLayers) != null)
            {
                spot = feet;
            }

            Enemy runner = _brood.Spawn(spot);
            if (runner == null)
            {
                return;
            }

            BornCount++;
            Birthed?.Invoke(this, runner);
        }
    }
}
