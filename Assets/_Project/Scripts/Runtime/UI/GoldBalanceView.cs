using RushOps.Save;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// The spendable gold balance of the save game next to a coin icon (F-GLD-06, issue #237), in the main menu and in
    /// the skin carousel. Thin adapter: it reads the balance when it shows and whenever <see cref="Refresh"/> is called.
    /// </summary>
    public class GoldBalanceView : MonoBehaviour
    {
        [SerializeField] private SaveGameService _save;
        [SerializeField] private Text _label;

        /// <summary>Balance text shown.</summary>
        public string TextShown => _label != null ? _label.text : string.Empty;

        /// <summary>Shows the current balance of the save game.</summary>
        public void Refresh()
        {
            if (_label != null)
            {
                _label.text = ScoreText.Gold(_save != null && _save.Data != null && _save.Data.Gold != null ? _save.Data.Gold.Balance : 0);
            }
        }

        private void OnEnable()
        {
            Refresh();
        }
    }
}
