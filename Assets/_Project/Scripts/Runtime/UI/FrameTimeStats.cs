using System;

namespace RushOps.UI
{
    /// <summary>
    /// Frame times of the last frames (issue #189): their mean and the mean of the slowest 1 %, the usual measure of
    /// stutter. Fixed buffers, so adding a frame and reading the values allocate nothing.
    /// </summary>
    public sealed class FrameTimeStats
    {
        private readonly float[] _frames;
        private readonly float[] _sorted;
        private int _next;

        /// <summary>Keeps the last <paramref name="capacity"/> frames.</summary>
        public FrameTimeStats(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _frames = new float[capacity];
            _sorted = new float[capacity];
        }

        /// <summary>Number of frames kept, at most the capacity.</summary>
        public int Count { get; private set; }

        /// <summary>Adds one frame of <paramref name="seconds"/>; the oldest drops out once the buffer is full.</summary>
        public void Add(float seconds)
        {
            _frames[_next] = seconds;
            _next = (_next + 1) % _frames.Length;
            if (Count < _frames.Length)
            {
                Count++;
            }
        }

        /// <summary>Forgets all frames.</summary>
        public void Clear()
        {
            Count = 0;
            _next = 0;
        }

        /// <summary>Mean frame time in milliseconds; 0 without frames.</summary>
        public float MeanMilliseconds()
        {
            if (Count == 0)
            {
                return 0f;
            }

            float sum = 0f;
            for (int i = 0; i < Count; i++)
            {
                sum += _frames[i];
            }

            return sum / Count * 1000f;
        }

        /// <summary>
        /// Mean of the slowest 1 % of the frames in milliseconds, at least the single slowest frame; 0 without frames.
        /// </summary>
        public float WorstPercentMilliseconds()
        {
            if (Count == 0)
            {
                return 0f;
            }

            Array.Copy(_frames, _sorted, Count);
            Array.Sort(_sorted, 0, Count);
            int worst = Math.Max(1, (int)Math.Ceiling(Count * 0.01));
            float sum = 0f;
            for (int i = Count - worst; i < Count; i++)
            {
                sum += _sorted[i];
            }

            return sum / worst * 1000f;
        }
    }
}
