using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Frames per second for device tests (issue #13), averaged over half a second of real time.
    /// Shown only in development builds and the editor; release builds hide it.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class FpsCounter : MonoBehaviour
    {
        private const float SampleSeconds = 0.5f;
        private const int MaxShown = 240;

        // Prebuilt, so updating the label allocates no strings.
        private static string[] _labels;

        private Text _text;
        private float _elapsed;
        private int _frames;

        /// <summary>Last measured frames per second.</summary>
        public int Fps { get; private set; }

        private void Awake()
        {
            if (!Debug.isDebugBuild)
            {
                gameObject.SetActive(false);
                return;
            }

            _text = GetComponent<Text>();
            if (_labels == null)
            {
                _labels = new string[MaxShown + 1];
                for (int i = 0; i <= MaxShown; i++)
                {
                    _labels[i] = "FPS " + i;
                }
            }
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            _frames++;
            if (_elapsed < SampleSeconds)
            {
                return;
            }

            Fps = Mathf.RoundToInt(_frames / _elapsed);
            _text.text = _labels[Mathf.Clamp(Fps, 0, MaxShown)];
            _elapsed = 0f;
            _frames = 0;
        }
    }
}
