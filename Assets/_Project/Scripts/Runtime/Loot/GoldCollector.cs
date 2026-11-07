using RushOps.Scoring;
using UnityEngine;

namespace RushOps.Loot
{
    /// <summary>
    /// Credits collected coins (F-GLD-02, F-SCO-02): one gold per coin into the <see cref="GoldWallet"/> and
    /// <see cref="GoldConfig.PointsPerCoin"/> times the multiplier into the level score. Thin adapter; the rules live
    /// in <see cref="GoldWallet"/> and <see cref="ScoreKeeper.RegisterGold"/>.
    /// </summary>
    public class GoldCollector : MonoBehaviour
    {
        [SerializeField] private GoldConfig _config;
        [SerializeField] private GoldPickupPool _coins;
        [SerializeField] private LevelScore _score;

        private GoldWallet _wallet;

        /// <summary>The player's gold.</summary>
        public GoldWallet Wallet => _wallet ??= new GoldWallet();

        private void OnEnable()
        {
            if (_coins != null)
            {
                _coins.Collected += OnCollected;
            }
        }

        private void OnDisable()
        {
            if (_coins != null)
            {
                _coins.Collected -= OnCollected;
            }
        }

        private void OnCollected(GoldPickup coin)
        {
            Wallet.Credit(1);
            if (_score != null && _config != null)
            {
                _score.Keeper.RegisterGold(_config.PointsPerCoin, _config.RestartsComboWindow);
            }
        }
    }
}
