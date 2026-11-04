using UnityEngine;

namespace RushOps.UI
{
    /// <summary>Lines of the credits screen in the settings (issue #126, F-UI-04). Edit them in the asset.</summary>
    [CreateAssetMenu(fileName = "CreditsConfig", menuName = "RushOps/Credits Config")]
    public class CreditsConfig : ScriptableObject
    {
        [Tooltip("One entry per line; empty entries leave a gap.")]
        [SerializeField, TextArea] private string[] _lines =
        {
            "RushOps",
            "",
            "Spielidee und Design",
            "beratkrhn",
            "",
            "Code, Pixelart und Sounds",
            "per Skript erzeugt",
            "mit Claude Code",
            "",
            "Engine",
            "Unity 6",
        };

        /// <summary>Lines of the credits, top to bottom.</summary>
        public string[] Lines => _lines;
    }
}
