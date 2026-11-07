using System;
using System.Collections.Generic;
using RushOps.Levels;
using RushOps.Loot;
using RushOps.Perks;
using RushOps.Scoring;
using RushOps.UI;
using UnityEngine;

namespace RushOps.Bosses
{
    /// <summary>
    /// Runs a boss fight (F-BOS-01, F-LVL-05, decisions C6, C9, C10): when the player enters the arena zone, the
    /// door closes behind them, the health bar appears and the boss starts. Each health threshold drops a random perk
    /// other than nuke and insta-kill. When all bodies are dead the boss's points are added and, after a short delay,
    /// „Level geschafft!“ shows. Bodies split off during the fight join with <see cref="AddBody"/> (Mega-Boomer).
    /// </summary>
    public class BossFight : MonoBehaviour
    {
        [Tooltip("Arena zone; entering it starts the fight.")]
        [SerializeField] private Zone _arena;
        [SerializeField] private ArenaDoor _door;
        [Tooltip("Bodies of the boss at the start, usually one.")]
        [SerializeField] private Boss[] _bodies = Array.Empty<Boss>();
        [SerializeField] private BossHealthBar _bar;
        [SerializeField] private Transform _player;
        [SerializeField] private LevelScore _score;
        [SerializeField] private LevelResults _results;
        [Tooltip("Drops the perks; its loot config lists the perks to pick from.")]
        [SerializeField] private PerkDropper _dropper;
        [SerializeField] private LootConfig _loot;
        [Tooltip("Drops the boss's gold coins when it is beaten (F-GLD-01); none if empty.")]
        [SerializeField] private GoldDropper _gold;

        private readonly List<Boss> _all = new List<Boss>();
        private BossPerkThresholds _thresholds;
        private PerkPool _perks;
        private IRandomSource _random;
        private float _victoryTime = -1f;
        private int _startHealth;

        /// <summary>Raised when the fight starts.</summary>
        public event Action<BossFight> Started;

        /// <summary>Raised when the last body died.</summary>
        public event Action<BossFight> Won;

        /// <summary>True from the start of the fight until the boss is beaten.</summary>
        public bool IsRunning { get; private set; }

        /// <summary>True once all bodies are dead.</summary>
        public bool IsWon { get; private set; }

        /// <summary>The body the fight starts with, as set in the scene; also on an inactive template.</summary>
        public Boss FirstBody => _bodies != null && _bodies.Length > 0 ? _bodies[0] : null;

        /// <summary>All bodies of the boss so far.</summary>
        public IReadOnlyList<Boss> Bodies => _all;

        /// <summary>Health of all living bodies.</summary>
        public int CurrentHealth
        {
            get
            {
                int sum = 0;
                foreach (Boss body in _all)
                {
                    sum += body != null && body.IsInFight ? body.CurrentHealth : 0;
                }

                return sum;
            }
        }

        /// <summary>Health at the start of the fight; the bar and the perk thresholds refer to it.</summary>
        public int StartHealth => _startHealth;

        /// <summary>Replaces the random source for perk picks, e.g. in tests.</summary>
        public void SetRandomSource(IRandomSource random)
        {
            _random = random;
            _perks = null;
        }

        /// <summary>Starts the fight, if it did not start yet; normally called by entering the arena.</summary>
        public void Begin()
        {
            if (IsRunning || IsWon)
            {
                return;
            }

            IsRunning = true;
            if (_door != null)
            {
                _door.Close();
            }

            BossDefinition definition = Main != null ? Main.Definition : null;
            _thresholds = new BossPerkThresholds(definition != null ? definition.PerkThresholds : null);
            _startHealth = 0;
            foreach (Boss body in _all)
            {
                _startHealth += body.MaxHealth;
                body.Activate(_player);
            }

            if (_bar != null)
            {
                _bar.Show(definition != null ? definition.DisplayName : string.Empty);
            }

            Started?.Invoke(this);
        }

        /// <summary>Adds a body that split off during the fight; it shares the health bar and must die too.</summary>
        public void AddBody(Boss body)
        {
            if (body == null || _all.Contains(body))
            {
                return;
            }

            Track(body);
            if (IsRunning)
            {
                body.Activate(_player);
            }
        }

        private Boss Main => _all.Count > 0 ? _all[0] : null;

        private PerkPool Perks => _perks ??= new PerkPool(
            BossMath.DroppablePerks(_loot != null ? _loot.Perks : null), _random ?? new SystemRandomSource());

        private void Awake()
        {
            foreach (Boss body in _bodies)
            {
                Track(body);
            }
        }

        private void OnEnable()
        {
            if (_arena != null)
            {
                _arena.Activated += OnArenaActivated;
            }
        }

        private void OnDisable()
        {
            if (_arena != null)
            {
                _arena.Activated -= OnArenaActivated;
            }
        }

        private void OnDestroy()
        {
            foreach (Boss body in _all)
            {
                if (body != null)
                {
                    body.Hit -= OnBodyHit;
                    body.Died -= OnBodyDied;
                }
            }
        }

        private void Update()
        {
            if (_victoryTime < 0f)
            {
                return;
            }

            _victoryTime += Time.deltaTime;
            float delay = Main != null && Main.Definition != null ? Main.Definition.VictoryDelay : 0f;
            if (_victoryTime >= delay)
            {
                _victoryTime = -1f;
                if (_results != null)
                {
                    _results.ShowDone();
                }
            }
        }

        private void Track(Boss body)
        {
            if (body == null)
            {
                return;
            }

            _all.Add(body);
            body.Hit += OnBodyHit;
            body.Died += OnBodyDied;
        }

        private void OnArenaActivated(Zone zone)
        {
            Begin();
        }

        private void OnBodyHit(Boss body)
        {
            if (!IsRunning)
            {
                return;
            }

            int current = CurrentHealth;
            if (_bar != null)
            {
                _bar.SetFill(BossMath.Fill(current, _startHealth));
            }

            int drops = _thresholds != null ? _thresholds.Advance(current, _startHealth) : 0;
            for (int i = 0; i < drops && _dropper != null; i++)
            {
                _dropper.Drop(body.transform.position, Perks.Pick());
            }
        }

        private void OnBodyDied(Boss body)
        {
            if (!IsRunning)
            {
                return;
            }

            foreach (Boss other in _all)
            {
                if (other != null && other.IsInFight)
                {
                    return;
                }
            }

            IsRunning = false;
            IsWon = true;
            if (_bar != null)
            {
                _bar.SetFill(0f);
            }

            if (_score != null && Main != null && Main.Definition != null)
            {
                _score.Keeper.AddPoints(Main.Definition.Points);
            }

            if (_gold != null && Main != null && Main.Definition != null)
            {
                _gold.Drop(body.transform.position, Main.Definition.Gold);
            }

            _victoryTime = 0f;
            Won?.Invoke(this);
        }
    }
}
