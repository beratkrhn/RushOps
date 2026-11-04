using UnityEngine;

namespace RushOps.Scoring
{
    /// <summary>Combo rules of the score (F-SCO-02, issue #28). The points per kill come from each enemy type.</summary>
    [CreateAssetMenu(fileName = "ScoreConfig", menuName = "RushOps/Score Config")]
    public class ScoreConfig : ScriptableObject
    {
        [Tooltip("Seconds after a kill in which the next kill keeps the combo.")]
        [SerializeField, Min(0f)] private float _comboWindow = 2f;

        [Tooltip("Combo kills per multiplier step.")]
        [SerializeField, Min(1)] private int _killsPerStep = 5;

        [Tooltip("Highest multiplier.")]
        [SerializeField, Min(1)] private int _maxMultiplier = 5;

        /// <summary>Seconds after a kill in which the next kill keeps the combo.</summary>
        public float ComboWindow => _comboWindow;

        /// <summary>Combo kills per multiplier step.</summary>
        public int KillsPerStep => _killsPerStep;

        /// <summary>Highest multiplier.</summary>
        public int MaxMultiplier => _maxMultiplier;
    }
}
