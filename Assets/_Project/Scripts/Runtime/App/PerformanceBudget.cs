using UnityEngine;

namespace RushOps.App
{
    /// <summary>
    /// Frame budget for the stress scene in the editor (issue #190, decision R8): a mid-range iPhone is about two to
    /// three times slower per core than the development Mac, so these values leave room for 60 FPS on the device
    /// (NF-01). Checked by the PlayMode test <c>PerformanceBudgetTests</c>; reasons in
    /// <c>docs/testprotokolle/performance.md</c>.
    /// </summary>
    [CreateAssetMenu(menuName = "RushOps/Leistungsbudget", fileName = "PerformanceBudget")]
    public class PerformanceBudget : ScriptableObject
    {
        [Tooltip("Mean CPU time of the game per frame in the editor, in milliseconds.")]
        [SerializeField, Min(0f)] private float _meanMilliseconds = 2f;

        [Tooltip("Mean CPU time of the slowest 1 % of frames in the editor, in milliseconds.")]
        [SerializeField, Min(0f)] private float _worstPercentMilliseconds = 5f;

        [Tooltip("Managed bytes the game may allocate in any frame after warming up.")]
        [SerializeField, Min(0)] private int _maxAllocatedBytesPerFrame;

        [Tooltip("Draw calls in any frame.")]
        [SerializeField, Min(1)] private int _maxDrawCalls = 150;

        [Tooltip("Memory of the app on the device in megabytes, read off the FPS display in the playtest.")]
        [SerializeField, Min(1)] private int _maxDeviceMemoryMegabytes = 300;

        /// <summary>Mean CPU time of the game per frame in the editor, in milliseconds.</summary>
        public float MeanMilliseconds => _meanMilliseconds;

        /// <summary>Mean CPU time of the slowest 1 % of frames in the editor, in milliseconds.</summary>
        public float WorstPercentMilliseconds => _worstPercentMilliseconds;

        /// <summary>Managed bytes the game may allocate in any frame after warming up.</summary>
        public int MaxAllocatedBytesPerFrame => _maxAllocatedBytesPerFrame;

        /// <summary>Draw calls in any frame.</summary>
        public int MaxDrawCalls => _maxDrawCalls;

        /// <summary>Memory of the app on the device in megabytes.</summary>
        public int MaxDeviceMemoryMegabytes => _maxDeviceMemoryMegabytes;
    }
}
