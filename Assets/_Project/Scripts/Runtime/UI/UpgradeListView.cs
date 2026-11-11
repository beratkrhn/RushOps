using RushOps.Endless;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// The upgrades taken in the running endless run with their levels, in the pause overlay (issue #279). Refreshed
    /// each time the overlay opens.
    /// </summary>
    public class UpgradeListView : MonoBehaviour
    {
        [SerializeField] private EndlessRunController _controller;
        [SerializeField] private Text _list;

        /// <summary>The text shown, e.g. for tests.</summary>
        public string Text => _list != null ? _list.text : string.Empty;

        private void OnEnable()
        {
            if (_controller != null && _list != null)
            {
                _list.text = EndlessText.UpgradeList(_controller.Upgrades);
            }
        }
    }
}
