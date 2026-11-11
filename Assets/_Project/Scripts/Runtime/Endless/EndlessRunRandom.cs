using RushOps.Audio;
using RushOps.Enemies;
using RushOps.Loot;
using RushOps.Perks;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Endless
{
    /// <summary>
    /// Thin adapter that gives every system of the endless arena its random source when a run begins (F-END-13, N21,
    /// ADR 0015): in the daily run one stream per system from the seed of the date (<see cref="DailySeed"/>,
    /// <see cref="RunRandom"/>), so every try of the day starts the same; in a normal run fresh unseeded sources. Covers
    /// upgrade offers, spawn points, pack offsets, perk and gold drops, the drops of each boss, the weapon spread and the
    /// sound pitch.
    /// </summary>
    public class EndlessRunRandom : MonoBehaviour
    {
        [SerializeField] private EndlessRunController _controller;
        [SerializeField] private WaveDirector _director;
        [SerializeField] private PerkDropper _perks;
        [SerializeField] private GoldDropper _gold;
        [SerializeField] private PlayerWeapon _weapon;
        [SerializeField] private SfxPlayer _sfx;

        /// <summary>Seed of the running run; 0 for a normal run.</summary>
        public int Seed { get; private set; }

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.RunBegan += OnRunBegan;
                _controller.RoundStarted += OnRoundStarted;
            }
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.RunBegan -= OnRunBegan;
                _controller.RoundStarted -= OnRoundStarted;
            }
        }

        private void OnRunBegan(EndlessRunController run)
        {
            Seed = run.IsDaily ? DailySeed.FromKey(run.DailyDate) : 0;
            run.SetRandomSource(Source(RunRandom.Offers));
            if (_director != null)
            {
                _director.SetRandomSources(Source(RunRandom.SpawnPoints), Source(RunRandom.Packs));
            }

            if (_perks != null)
            {
                _perks.SetRandomSource(Source(RunRandom.Perks));
            }

            if (_gold != null)
            {
                _gold.SetRandomSource(Source(RunRandom.Gold));
            }

            if (_weapon != null)
            {
                _weapon.SetRandomSource(Source(RunRandom.Spread));
            }

            if (_sfx != null)
            {
                _sfx.SetRandomSource(Source(RunRandom.Sounds));
            }
        }

        // Every boss fight gets its own stream, named after its round, for its perk drops.
        private void OnRoundStarted(EndlessRound round)
        {
            if (_controller.CurrentFight != null)
            {
                _controller.CurrentFight.SetRandomSource(Source(RunRandom.Boss + round.Number));
            }
        }

        private IRandomSource Source(string stream)
        {
            return _controller.IsDaily ? RunRandom.Stream(Seed, stream) : new SystemRandomSource();
        }
    }
}
