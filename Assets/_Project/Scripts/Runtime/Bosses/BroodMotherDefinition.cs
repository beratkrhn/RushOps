using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>Values of the Brutmutter (issue #155, decision C8): how often she gives birth to a runner.</summary>
    [CreateAssetMenu(fileName = "BroodMotherDefinition", menuName = "RushOps/Brood Mother Definition")]
    public class BroodMotherDefinition : BossDefinition
    {
        [Header("Brood")]
        [Tooltip("Seconds from one runner to the next, birth included (C8).")]
        [SerializeField, Min(0.1f)] private float _spawnInterval = 3f;

        [Tooltip("Seconds from one runner to the next below the enraged health share (C8).")]
        [SerializeField, Min(0.1f)] private float _enragedSpawnInterval = 2f;

        [Tooltip("Below this share of her health (0..1) she uses the shorter interval.")]
        [SerializeField, Range(0f, 1f)] private float _enragedHealthShare = 0.5f;

        [Tooltip("Seconds she stands still while a runner comes out.")]
        [SerializeField, Min(0f)] private float _birthTime = 0.6f;

        /// <summary>Seconds from one runner to the next.</summary>
        public float SpawnInterval => _spawnInterval;

        /// <summary>Seconds from one runner to the next below <see cref="EnragedHealthShare"/>.</summary>
        public float EnragedSpawnInterval => _enragedSpawnInterval;

        /// <summary>Health share (0..1) below which the shorter interval applies.</summary>
        public float EnragedHealthShare => _enragedHealthShare;

        /// <summary>Seconds of one birth.</summary>
        public float BirthTime => _birthTime;
    }
}
