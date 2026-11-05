using System;
using System.Diagnostics;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Measures only the game's part of a frame (issues #189, #190): CPU time and managed allocations on the main
    /// thread from the start of <see cref="EarlyUpdate"/> to the end of <see cref="PostLateUpdate"/>. That leaves out
    /// the wait for the target frame rate and the editor's own work between frames, which the profiler counters
    /// „Main Thread“ and „GC Allocated In Frame“ include.
    /// </summary>
    internal sealed class PlayerLoopProbe : IDisposable
    {
        private readonly Stopwatch _watch = new Stopwatch();
        private readonly PlayerLoopSystem _original;
        private long _startBytes;

        /// <summary>Hooks into the player loop until disposed.</summary>
        public PlayerLoopProbe()
        {
            _original = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopSystem loop = _original;
            PlayerLoopSystem[] phases = (PlayerLoopSystem[])loop.subSystemList.Clone();
            for (int i = 0; i < phases.Length; i++)
            {
                if (phases[i].type == typeof(EarlyUpdate))
                {
                    phases[i] = WithSystem(phases[i], typeof(Begin), OnBegin, first: true);
                }
                else if (phases[i].type == typeof(PostLateUpdate))
                {
                    phases[i] = WithSystem(phases[i], typeof(End), OnEnd, first: false);
                }
            }

            loop.subSystemList = phases;
            PlayerLoop.SetPlayerLoop(loop);
        }

        /// <summary>True once a whole frame was measured.</summary>
        public bool HasFrame { get; private set; }

        /// <summary>CPU milliseconds of the last measured frame.</summary>
        public float LastMilliseconds { get; private set; }

        /// <summary>Managed bytes allocated on the main thread in the last measured frame.</summary>
        public long LastAllocatedBytes { get; private set; }

        public void Dispose()
        {
            PlayerLoop.SetPlayerLoop(_original);
        }

        private void OnBegin()
        {
            _startBytes = GC.GetAllocatedBytesForCurrentThread();
            _watch.Restart();
        }

        private void OnEnd()
        {
            if (!_watch.IsRunning)
            {
                return;
            }

            _watch.Stop();
            LastMilliseconds = (float)_watch.Elapsed.TotalMilliseconds;
            LastAllocatedBytes = GC.GetAllocatedBytesForCurrentThread() - _startBytes;
            HasFrame = true;
        }

        private static PlayerLoopSystem WithSystem(
            PlayerLoopSystem phase, Type type, PlayerLoopSystem.UpdateFunction update, bool first)
        {
            PlayerLoopSystem[] systems = phase.subSystemList ?? Array.Empty<PlayerLoopSystem>();
            var list = new PlayerLoopSystem[systems.Length + 1];
            int offset = first ? 1 : 0;
            Array.Copy(systems, 0, list, offset, systems.Length);
            list[first ? 0 : systems.Length] = new PlayerLoopSystem { type = type, updateDelegate = update };
            phase.subSystemList = list;
            return phase;
        }

        private struct Begin
        {
        }

        private struct End
        {
        }
    }
}
