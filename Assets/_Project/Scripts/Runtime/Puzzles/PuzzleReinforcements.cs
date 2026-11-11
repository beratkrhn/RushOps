using System.Collections.Generic;
using RushOps.Enemies;
using RushOps.Levels;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Keeps the zombie a puzzle needs coming while the puzzle is active (issue #289, F-RAE-10, F-RAE-12): thin adapter on
    /// <see cref="ReinforcementTimer"/>. Spawns from the needed zombie's pool at the zone's spawn point farthest from the
    /// player, so it never pops up in the player's face. Stops once the puzzle is solved; zombies already sent stay.
    /// </summary>
    [RequireComponent(typeof(Puzzle))]
    public class PuzzleReinforcements : MonoBehaviour
    {
        [Tooltip("Pools of the level; the one of the needed zombie is used.")]
        [SerializeField] private EnemyPool[] _pools = System.Array.Empty<EnemyPool>();
        [SerializeField] private Transform _player;

        private readonly List<Enemy> _sent = new List<Enemy>(4);
        private Puzzle _puzzle;
        private IReinforcedPuzzle _source;
        private EnemyPool _pool;
        private ReinforcementTimer _timer;

        /// <summary>Zombies sent by this puzzle that are still alive.</summary>
        public int Alive => _sent.Count;

        /// <summary>Total zombies sent so far.</summary>
        public int SentTotal { get; private set; }

        private void Awake()
        {
            _puzzle = GetComponent<Puzzle>();
            _source = _puzzle as IReinforcedPuzzle;
        }

        private void Start()
        {
            if (_source == null)
            {
                return;
            }

            EnemyDefinition needed = _source.NeededEnemy;
            foreach (EnemyPool pool in _pools)
            {
                if (pool != null && pool.Definition == needed)
                {
                    _pool = pool;
                    break;
                }
            }

            _timer = new ReinforcementTimer(_source.ReinforcementInterval, _source.MaxReinforcements);
        }

        private void Update()
        {
            if (_pool == null || _timer == null || _puzzle.State != PuzzleState.Active)
            {
                return;
            }

            for (int i = _sent.Count - 1; i >= 0; i--)
            {
                if (_sent[i] == null || _sent[i].IsDead || !_sent[i].isActiveAndEnabled)
                {
                    _sent.RemoveAt(i);
                }
            }

            if (!_timer.Tick(Time.deltaTime, _sent.Count))
            {
                return;
            }

            SpawnPoint point = FarthestPoint();
            if (point == null)
            {
                return;
            }

            Enemy enemy = _pool.Spawn(point.Position);
            if (enemy != null)
            {
                _sent.Add(enemy);
                SentTotal++;
            }
        }

        private SpawnPoint FarthestPoint()
        {
            Zone zone = _puzzle.Zone;
            if (zone == null)
            {
                return null;
            }

            Vector2 player = _player != null ? (Vector2)_player.position : (Vector2)transform.position;
            SpawnPoint best = null;
            float bestDistance = -1f;
            foreach (SpawnPoint point in zone.SpawnPoints)
            {
                if (point == null)
                {
                    continue;
                }

                float distance = (point.Position - player).sqrMagnitude;
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    best = point;
                }
            }

            return best;
        }
    }
}
