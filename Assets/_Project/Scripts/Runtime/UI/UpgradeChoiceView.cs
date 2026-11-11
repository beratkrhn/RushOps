using System.Collections.Generic;
using RushOps.Endless;
using UnityEngine;

namespace RushOps.UI
{
    /// <summary>
    /// The upgrade choice of the endless run (F-END-11, F-END-12, decisions T2 and N20, issue #279): when the run offers
    /// upgrades it holds the game and shows one card per offered upgrade; tapping a card takes it and the normal break
    /// „Runde X“ follows. There is no skip. The pause button keeps working on top of it.
    /// </summary>
    public class UpgradeChoiceView : MonoBehaviour
    {
        [SerializeField] private EndlessRunController _controller;
        [SerializeField] private PauseController _pause;
        [Tooltip("Dim panel with the title and the cards; shown only during a choice.")]
        [SerializeField] private GameObject _panel;
        [Tooltip("Cards top to bottom; as many show as upgrades are offered.")]
        [SerializeField] private UpgradeCardView[] _cards = System.Array.Empty<UpgradeCardView>();

        /// <summary>True while the cards show.</summary>
        public bool IsShowing => _panel != null && _panel.activeSelf;

        /// <summary>The cards, e.g. for tests.</summary>
        public IReadOnlyList<UpgradeCardView> Cards => _cards;

        /// <summary>Number of cards showing.</summary>
        public int ShownCards
        {
            get
            {
                int shown = 0;
                foreach (UpgradeCardView card in _cards)
                {
                    if (card != null && card.gameObject.activeSelf)
                    {
                        shown++;
                    }
                }

                return shown;
            }
        }

        /// <summary>Takes <paramref name="upgrade"/>; called by the cards.</summary>
        public void Pick(UpgradeDefinition upgrade)
        {
            if (_controller != null)
            {
                _controller.Choose(upgrade);
            }
        }

        private void Awake()
        {
            SetPanel(false);
        }

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.UpgradeOffered += OnOffered;
                _controller.UpgradeChosen += OnChosen;
                _controller.ChoiceCancelled += Close;
            }
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.UpgradeOffered -= OnOffered;
                _controller.UpgradeChosen -= OnChosen;
                _controller.ChoiceCancelled -= Close;
            }
        }

        private void OnOffered(IReadOnlyList<UpgradeDefinition> offer)
        {
            RunUpgrades upgrades = _controller.Upgrades;
            for (int i = 0; i < _cards.Length; i++)
            {
                if (i < offer.Count)
                {
                    _cards[i].Show(offer[i], upgrades.Level(offer[i]) + 1);
                }
                else
                {
                    _cards[i].Hide();
                }
            }

            SetPanel(true);
            if (_pause != null)
            {
                _pause.Hold();
            }
        }

        private void OnChosen(UpgradeDefinition upgrade, int level)
        {
            Close();
        }

        private void Close()
        {
            SetPanel(false);
            if (_pause != null)
            {
                _pause.Release();
            }
        }

        private void SetPanel(bool visible)
        {
            if (_panel != null)
            {
                _panel.SetActive(visible);
            }
        }
    }
}
