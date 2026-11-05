using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Frames per second for device tests (issue #13), averaged over half a second of real time, and below them the
    /// frame time of the last seconds: mean and the slowest 1 % (issue #189).
    /// Shown only in development builds and the editor; release builds hide it.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class FpsCounter : MonoBehaviour
    {
        private const float SampleSeconds = 0.5f;
        private const int MaxShown = 240;
        // About five seconds at 60 FPS.
        private const int KeptFrames = 300;
        // Tenths of a millisecond above this are shown as this.
        private const int MaxTenths = 9999;

        // Labels are built once per combination, so a steady game reuses them and allocates no strings.
        private static readonly Dictionary<long, string> Labels = new Dictionary<long, string>();

        private readonly FrameTimeStats _stats = new FrameTimeStats(KeptFrames);
        private Text _text;
        private float _elapsed;
        private int _frames;

        /// <summary>Last measured frames per second.</summary>
        public int Fps { get; private set; }

        /// <summary>Mean frame time of the last seconds in milliseconds.</summary>
        public float MeanMilliseconds { get; private set; }

        /// <summary>Mean of the slowest 1 % of the last seconds in milliseconds.</summary>
        public float WorstPercentMilliseconds { get; private set; }

        /// <summary>Text for <paramref name="fps"/> and the two frame times, e.g. „FPS 60“ over „16,7 / 21,3 ms“.</summary>
        public static string Format(int fps, float meanMilliseconds, float worstMilliseconds)
        {
            int clampedFps = Mathf.Clamp(fps, 0, MaxShown);
            int mean = Mathf.Clamp(Mathf.RoundToInt(meanMilliseconds * 10f), 0, MaxTenths);
            int worst = Mathf.Clamp(Mathf.RoundToInt(worstMilliseconds * 10f), 0, MaxTenths);
            long key = ((long)clampedFps * (MaxTenths + 1) + mean) * (MaxTenths + 1) + worst;
            if (!Labels.TryGetValue(key, out string label))
            {
                label = $"FPS {clampedFps}\n{Tenths(mean)} / {Tenths(worst)} ms";
                Labels.Add(key, label);
            }

            return label;
        }

        private static string Tenths(int tenths)
        {
            return $"{tenths / 10},{tenths % 10}";
        }

        private void Awake()
        {
            if (!Debug.isDebugBuild)
            {
                gameObject.SetActive(false);
                return;
            }

            _text = GetComponent<Text>();
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            _stats.Add(delta);
            _elapsed += delta;
            _frames++;
            if (_elapsed < SampleSeconds)
            {
                return;
            }

            Fps = Mathf.RoundToInt(_frames / _elapsed);
            MeanMilliseconds = _stats.MeanMilliseconds();
            WorstPercentMilliseconds = _stats.WorstPercentMilliseconds();
            _text.text = Format(Fps, MeanMilliseconds, WorstPercentMilliseconds);
            _elapsed = 0f;
            _frames = 0;
        }
    }
}
