using UnityEngine;

namespace RushOps.Puzzles
{
    /// <summary>Values of the object transport (F-RAE-06, F-RAE-07, issue #33).</summary>
    [CreateAssetMenu(fileName = "CarryConfig", menuName = "RushOps/Carry Config")]
    public class CarryConfig : ScriptableObject
    {
        [Tooltip("Factor on the walking speed while carrying, e.g. 0.8 for 20 percent slower.")]
        [SerializeField, Range(0.1f, 1f)] private float _speedFactor = 0.8f;

        [Tooltip("Tracker text when one cell is needed.")]
        [SerializeField] private string _singleText = "Energiezelle zum Generator bringen";

        [Tooltip("Tracker text when several cells are needed, followed by the count, e.g. „Energiezellen 1/3“.")]
        [SerializeField] private string _multiText = "Energiezellen";

        [SerializeField, Range(0f, 1f)] private float _pickUpVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float _handInVolume = 0.8f;

        /// <summary>Factor on the walking speed while carrying.</summary>
        public float SpeedFactor => _speedFactor;

        /// <summary>Tracker text for one cell.</summary>
        public string SingleText => _singleText;

        /// <summary>Tracker text for several cells, before the count.</summary>
        public string MultiText => _multiText;

        /// <summary>Volume when a cell is picked up.</summary>
        public float PickUpVolume => _pickUpVolume;

        /// <summary>Volume when a cell is handed in.</summary>
        public float HandInVolume => _handInVolume;
    }
}
