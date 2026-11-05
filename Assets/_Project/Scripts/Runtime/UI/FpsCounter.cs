using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Frames per second for device tests (issue #13), averaged over half a second of real time, and below them the
    /// frame time of the last seconds: mean and the slowest 1 % (issue #189), and the memory the app uses (issue #190).
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
        private const int MaxMegabytes = 9999;
        private const long BytesPerMegabyte = 1024L * 1024L;

        // Labels are built once per combination, so a steady game reuses them and allocates no strings.
        private static readonly Dictionary<long, string> Labels = new Dictionary<long, string>();

        private readonly FrameTimeStats _stats = new FrameTimeStats(KeptFrames);
        private ProfilerRecorder _memory;
        private Text _text;
        private float _elapsed;
        private int _frames;

        /// <summary>Last measured frames per second.</summary>
        public int Fps { get; private set; }

        /// <summary>Mean frame time of the last seconds in milliseconds.</summary>
        public float MeanMilliseconds { get; private set; }

        /// <summary>Mean of the slowest 1 % of the last seconds in milliseconds.</summary>
        public float WorstPercentMilliseconds { get; private set; }

        /// <summary>Memory the app uses in megabytes; negative where the platform does not report it.</summary>
        public int MemoryMegabytes { get; private set; } = -1;

        /// <summary>
        /// Text for <paramref name="fps"/>, the two frame times and the memory, e.g. „FPS 60“ over „16,7 / 21,3 ms“
        /// over „RAM 180 MB“. A negative <paramref name="memoryMegabytes"/> leaves out the last line.
        /// </summary>
        public static string Format(int fps, float meanMilliseconds, float worstMilliseconds, int memoryMegabytes = -1)
        {
            int clampedFps = Mathf.Clamp(fps, 0, MaxShown);
            int mean = Mathf.Clamp(Mathf.RoundToInt(meanMilliseconds * 10f), 0, MaxTenths);
            int worst = Mathf.Clamp(Mathf.RoundToInt(worstMilliseconds * 10f), 0, MaxTenths);
            int memory = memoryMegabytes < 0 ? -1 : Mathf.Min(memoryMegabytes, MaxMegabytes);
            long key = (((long)clampedFps * (MaxTenths + 1) + mean) * (MaxTenths + 1) + worst) * (MaxMegabytes + 2)
                + memory + 1;
            if (!Labels.TryGetValue(key, out string label))
            {
                label = memory < 0
                    ? $"FPS {clampedFps}\n{Tenths(mean)} / {Tenths(worst)} ms"
                    : $"FPS {clampedFps}\n{Tenths(mean)} / {Tenths(worst)} ms\nRAM {memory} MB";
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
            // Up to three lines: grow upwards from the bottom edge instead of past it.
            _text.alignment = TextAnchor.LowerCenter;
            // Resident memory of the app as the system counts it; the editor reports its own process, so leave it out.
            if (!Application.isEditor)
            {
                _memory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            }
        }

        private void OnDestroy()
        {
            _memory.Dispose();
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
            MemoryMegabytes = _memory.Valid && _memory.LastValue > 0 ? (int)(_memory.LastValue / BytesPerMegabyte) : -1;
            _text.text = Format(Fps, MeanMilliseconds, WorstPercentMilliseconds, MemoryMegabytes);
            _elapsed = 0f;
            _frames = 0;
        }
    }
}
