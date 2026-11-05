using System;

namespace RushOps.Tests.PlayMode
{
    /// <summary>Values of one measurement by <see cref="FrameSampler"/>; field names are the JSON keys in <c>Logs/perf/</c>.</summary>
    [Serializable]
    internal sealed class FrameSample
    {
        public string scene;
        public float seconds;
        public int frames;
        public float frameMeanMs;
        public float frameWorstPercentMs;
        public float gameMeanMs;
        public float gameWorstPercentMs;
        public float scriptsMeanMs;
        public float physicsMeanMs;
        public float gcBytesPerFrame;
        public long gcWorstFrameBytes;
        public int framesWithGc;
        public float peakTextureMemoryMb;
        public float drawCallsPerFrame;
        public long drawCallsWorst;
        public float batchesPerFrame;
        public float setPassCallsPerFrame;
        public float camerasRenderedPerFrame;
        public int enemiesAlive;
        public string graphics;
    }
}
