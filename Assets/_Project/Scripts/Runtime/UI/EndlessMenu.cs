using System.Collections.Generic;
using RushOps.Endless;
using RushOps.Save;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// The area „Endlos“ of the main menu (F-UI-02, F-END-14, N22, issue #280): „Starten“ for a normal run, „Tageslauf“
    /// for the daily run, today's best and the best daily runs of the last 7 days (date, score, round). Refreshed from the
    /// save game each time it opens. The switch „Darstellung“ chooses between Normal and Arcade (F-ARC-08, issue #325);
    /// the choice is saved at once, and the daily bests shown are those of the chosen display.
    /// </summary>
    public class EndlessMenu : MonoBehaviour
    {
        [SerializeField] private SaveGameService _save;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _today;
        [Tooltip("Label of the switch „Darstellung“: „Normal“ or „Arcade“.")]
        [SerializeField] private Text _display;
        [Tooltip("Columns of the list of the last days: date, score, round.")]
        [SerializeField] private Text[] _columns = System.Array.Empty<Text>();

        /// <summary>True while the area shows.</summary>
        public bool IsOpen => _panel != null && _panel.activeSelf;

        /// <summary>True if the arcade display is chosen.</summary>
        public bool ArcadeSelected => _save != null && ArcadeRecords.IsSelected(_save.Data);

        /// <summary>Label of the switch „Darstellung“, e.g. for tests.</summary>
        public string DisplayText => _display != null ? _display.text : string.Empty;

        /// <summary>Text of today's line, e.g. for tests.</summary>
        public string TodayText => _today != null ? _today.text : string.Empty;

        /// <summary>Text of a column of the list (0 date, 1 score, 2 round), e.g. for tests.</summary>
        public string ColumnText(int column)
        {
            return column >= 0 && column < _columns.Length && _columns[column] != null ? _columns[column].text : string.Empty;
        }

        /// <summary>Shows the area with the current daily bests.</summary>
        public void Open()
        {
            Refresh();
            if (_panel != null)
            {
                _panel.SetActive(true);
            }
        }

        /// <summary>Switches between Normal and Arcade and saves the choice.</summary>
        public void ToggleDisplay()
        {
            if (_save == null)
            {
                return;
            }

            ArcadeRecords.Select(_save.Data, !ArcadeSelected);
            _save.Save();
            Refresh();
        }

        /// <summary>Hides the area.</summary>
        public void Close()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        /// <summary>Reads the daily bests of the save game into the texts.</summary>
        public void Refresh()
        {
            bool arcade = ArcadeSelected;
            EndlessStart.Arcade = arcade;
            if (_display != null)
            {
                _display.text = EndlessText.Display(arcade);
            }

            DailyData data = _save != null ? ArcadeRecords.Daily(_save.Data, arcade) : null;
            string today = DailySeed.Today();
            if (_today != null)
            {
                _today.text = EndlessText.DailyToday(DailyRecords.Find(data, today));
            }

            List<DailyScore> days = DailyRecords.LastDays(data, today);
            for (int i = 0; i < _columns.Length; i++)
            {
                if (_columns[i] != null)
                {
                    _columns[i].text = EndlessText.DailyColumn(days, i);
                }
            }
        }

        private void Awake()
        {
            Close();
        }
    }
}
