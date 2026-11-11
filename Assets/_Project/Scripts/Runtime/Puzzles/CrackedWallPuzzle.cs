using RushOps.Combat;
using RushOps.Enemies;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>
    /// Cracked wall only a boomer breaks (issue #290, F-RAE-11, F-RAE-12, F-RAE-13, N27): thin adapter on
    /// <see cref="CrackedWallLogic"/>. It stands in front of a gate whose bars are hidden; the gate keeps blocking bodies,
    /// shots and the flow field and opens once this puzzle is solved. Only the explosion of a boomer in reach breaks it:
    /// shots never reach the logic, and the rocket, the scarecrow and the nuke do not raise
    /// <see cref="ExplosionSystem.EnemyExploded"/> with the boomer's blast. A hint bubble with a boomer head floats above
    /// it, and the mission tracker explains it the first time (<see cref="PuzzleHints"/>).
    /// </summary>
    public class CrackedWallPuzzle : Puzzle, IReinforcedPuzzle
    {
        [SerializeField] private CrackedWallConfig _wallConfig;
        [SerializeField] private ExplosionSystem _explosions;
        [Tooltip("Pool of the boomers; its enemies' blast breaks the wall.")]
        [SerializeField] private EnemyPool _boomers;
        [Tooltip("Remembers the shown hint; empty = always explain.")]
        [SerializeField] private SaveGameService _save;
        [Tooltip("Size of the wall in tiles, centered on this object.")]
        [SerializeField] private Vector2 _size = new Vector2(4f, 1f);
        [SerializeField] private SpriteRenderer[] _tiles = System.Array.Empty<SpriteRenderer>();
        [SerializeField] private Sprite _rubbleSprite;
        [SerializeField] private SpriteRenderer _hint;
        [SerializeField] private AudioClip _collapseClip;

        private CrackedWallLogic _logic;
        private bool _firstTime = true;
        private bool _listening;

        /// <summary>The wall's rules.</summary>
        public CrackedWallLogic Logic => _logic ??= new CrackedWallLogic(
            new Rect((Vector2)transform.position - _size * 0.5f, _size), _wallConfig != null ? _wallConfig.ExtraReach : 0.5f);

        /// <summary>True while the explaining hint text shows.</summary>
        public bool ExplainsHint => _firstTime;

        /// <inheritdoc />
        public float ReinforcementInterval => _wallConfig != null ? _wallConfig.BoomerInterval : 10f;

        /// <inheritdoc />
        public int MaxReinforcements => _wallConfig != null ? _wallConfig.MaxBoomers : 2;

        /// <inheritdoc />
        public EnemyDefinition NeededEnemy => _boomers != null ? _boomers.Definition : null;

        /// <inheritdoc />
        public override PuzzleDisplay Display =>
            PuzzleDisplay.TextOnly(PuzzleHints.Text(ZombiePuzzleKind.CrackedWall, _firstTime));

        /// <summary>Test hook: breaks an active wall as if a boomer had exploded right at it.</summary>
        public void DebugBreak()
        {
            OnEnemyExploded(transform.position, NeededEnemy != null ? NeededEnemy.Explosion : null);
        }

        /// <inheritdoc />
        protected override void OnActivated()
        {
            _firstTime = _save == null || !PuzzleHints.IsSeen(_save.Data.Hints, ZombiePuzzleKind.CrackedWall);
            if (_explosions != null && !_listening)
            {
                _explosions.EnemyExploded += OnEnemyExploded;
                _listening = true;
            }
        }

        /// <inheritdoc />
        protected override void OnSolved()
        {
            StopListening();
            if (_save != null)
            {
                // Saved with the next autosave (level done, death, leaving).
                PuzzleHints.MarkSeen(_save.Data.Hints, ZombiePuzzleKind.CrackedWall);
            }

            foreach (SpriteRenderer tile in _tiles)
            {
                if (tile != null)
                {
                    tile.sprite = _rubbleSprite;
                    tile.sortingOrder = Mathf.Min(tile.sortingOrder, 1);
                    tile.transform.localPosition = new Vector3(tile.transform.localPosition.x, 0f, 0f);
                }
            }

            if (_hint != null)
            {
                _hint.enabled = false;
            }
        }

        private void OnDestroy()
        {
            StopListening();
        }

        private void StopListening()
        {
            if (_explosions != null && _listening)
            {
                _explosions.EnemyExploded -= OnEnemyExploded;
            }

            _listening = false;
        }

        private void OnEnemyExploded(Vector2 center, ExplosionDefinition definition)
        {
            if (State != PuzzleState.Active || definition == null)
            {
                return;
            }

            ExplosionDefinition boomerBlast = NeededEnemy != null ? NeededEnemy.Explosion : null;
            if (Logic.NotifyExplosion(center, definition.Radius, definition == boomerBlast))
            {
                PlaySound(_collapseClip, _wallConfig != null ? _wallConfig.Volume : 0.9f);
                Solve();
            }
        }

        private void Update()
        {
            if (_hint == null || !_hint.enabled || _wallConfig == null)
            {
                return;
            }

            float y = _wallConfig.HintHeight + _wallConfig.HintBob * Mathf.Sin(Time.time * 3f);
            _hint.transform.localPosition = new Vector3(0f, y, 0f);
        }
    }
}
