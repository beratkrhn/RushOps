using System;
using System.Collections;
using RushOps.Enemies;
using RushOps.UI;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Runs a <see cref="StressScenario"/> and measures it (issues #189, #190): CPU time and allocations of the game
    /// per frame (<see cref="PlayerLoopProbe"/>), draw calls and texture memory. Batch mode has no game view, so the
    /// cameras render into a texture of iPhone size while it measures, if there is a graphics device.
    /// </summary>
    internal static class FrameSampler
    {
        private const int MaxFrames = 20000;
        private const double NanosecondsPerMillisecond = 1e6;
        private const double BytesPerMegabyte = 1024.0 * 1024.0;
        // iPhone screen in portrait, so the editor renders about as many pixels as the device.
        private const int TargetWidth = 1179;
        private const int TargetHeight = 2556;

        private static int _camerasRendered;

        /// <summary>Warms up for <paramref name="warmUpSeconds"/>, measures <paramref name="seconds"/>, then calls <paramref name="done"/>.</summary>
        public static IEnumerator Measure(
            string scene, StressScenario scenario, float warmUpSeconds, float seconds, Action<FrameSample> done)
        {
            float warmUp = 0f;
            while (warmUp < warmUpSeconds)
            {
                scenario.Tick();
                warmUp += Time.unscaledDeltaTime;
                yield return null;
            }

            // Without a graphics device (-nographics, as in Tools/unity-check.sh) nothing can render at all.
            bool canRender = SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null;
            RenderTexture target = canRender ? new RenderTexture(TargetWidth, TargetHeight, 24) : null;
            Camera[] cameras = canRender
                ? Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)
                : Array.Empty<Camera>();
            foreach (Camera camera in cameras)
            {
                camera.targetTexture = target;
            }

            _camerasRendered = 0;
            RenderPipelineManager.endCameraRendering += CountCamera;
            using var probe = new PlayerLoopProbe();
            using var scripts = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Update.ScriptRunBehaviourUpdate", 1);
            using var physics = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "FixedUpdate.Physics2DFixedUpdate", 1);
            using var textureMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory", 1);
            using var drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 1);
            using var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", 1);
            using var setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", 1);

            // Preallocated, so the measurement itself adds no garbage.
            var game = new FrameTimeStats(MaxFrames);
            var frame = new FrameTimeStats(MaxFrames);
            double scriptsTotal = 0;
            double physicsTotal = 0;
            long gcTotal = 0;
            long gcWorst = 0;
            int framesWithGc = 0;
            long peakTextures = 0;
            long drawCallsTotal = 0;
            long drawCallsWorst = 0;
            long batchesTotal = 0;
            long setPassTotal = 0;
            float elapsed = 0f;
            yield return null;

            while (elapsed < seconds && frame.Count < MaxFrames)
            {
                scenario.Tick();
                yield return null;
                float delta = Time.unscaledDeltaTime;
                elapsed += delta;
                frame.Add(delta);
                if (!probe.HasFrame)
                {
                    continue;
                }

                game.Add(probe.LastMilliseconds / 1000f);
                scriptsTotal += Milliseconds(scripts);
                physicsTotal += Milliseconds(physics);
                long gc = probe.LastAllocatedBytes;
                gcTotal += gc;
                gcWorst = Math.Max(gcWorst, gc);
                framesWithGc += gc > 0 ? 1 : 0;
                peakTextures = Math.Max(peakTextures, textureMemory.Valid ? textureMemory.LastValue : 0);
                long draws = DrawCalls(drawCalls);
                drawCallsTotal += draws;
                drawCallsWorst = Math.Max(drawCallsWorst, draws);
                batchesTotal += batches.Valid ? batches.LastValue : 0;
                setPassTotal += setPass.Valid ? setPass.LastValue : 0;
            }

            RenderPipelineManager.endCameraRendering -= CountCamera;
            foreach (Camera camera in cameras)
            {
                if (camera != null)
                {
                    camera.targetTexture = null;
                }
            }

            if (target != null)
            {
                target.Release();
                Object.Destroy(target);
            }
            int measured = Math.Max(1, game.Count);
            done(new FrameSample
            {
                scene = scene,
                seconds = elapsed,
                frames = frame.Count,
                frameMeanMs = frame.MeanMilliseconds(),
                frameWorstPercentMs = frame.WorstPercentMilliseconds(),
                gameMeanMs = game.MeanMilliseconds(),
                gameWorstPercentMs = game.WorstPercentMilliseconds(),
                scriptsMeanMs = (float)(scriptsTotal / measured),
                physicsMeanMs = (float)(physicsTotal / measured),
                gcBytesPerFrame = (float)gcTotal / measured,
                gcWorstFrameBytes = gcWorst,
                framesWithGc = framesWithGc,
                peakTextureMemoryMb = (float)(peakTextures / BytesPerMegabyte),
                drawCallsPerFrame = (float)drawCallsTotal / measured,
                drawCallsWorst = drawCallsWorst,
                batchesPerFrame = (float)batchesTotal / measured,
                setPassCallsPerFrame = (float)setPassTotal / measured,
                camerasRenderedPerFrame = (float)_camerasRendered / measured,
                enemiesAlive = CountEnemies(),
                graphics = SystemInfo.graphicsDeviceType.ToString(),
            });
        }

        private static void CountCamera(ScriptableRenderContext context, Camera camera)
        {
            _camerasRendered++;
        }

        // The profiler counter adds up every render pass of the frame. The open editor sometimes renders the game
        // twice in one frame, which doubled the count (issue #303: 238 against 119). UnityStats is the number of the
        // Game view's Stats overlay, one pass, so the editor uses it; builds and batch mode fall back to the counter.
        private static long DrawCalls(ProfilerRecorder profiler)
        {
#if UNITY_EDITOR
            int stats = UnityEditor.UnityStats.drawCalls;
            if (stats > 0)
            {
                return stats;
            }
#endif
            return profiler.Valid ? profiler.LastValue : 0;
        }

        // Recorders of markers that did not run this frame are invalid or 0.
        private static double Milliseconds(ProfilerRecorder recorder)
        {
            return recorder.Valid ? recorder.LastValue / NanosecondsPerMillisecond : 0.0;
        }

        private static int CountEnemies()
        {
            int alive = 0;
            foreach (WaveDirector director in Object.FindObjectsByType<WaveDirector>(FindObjectsSortMode.None))
            {
                alive += director.AliveCount;
            }

            return alive;
        }
    }
}
