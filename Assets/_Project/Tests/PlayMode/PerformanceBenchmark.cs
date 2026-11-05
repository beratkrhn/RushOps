using System.Collections;
using System.IO;
using NUnit.Framework;
using RushOps.UI;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Measures frame times and memory in the editor (issue #189, decision R8) and writes one JSON file per scene to
    /// <c>Logs/perf/</c>. Explicit, so normal test runs skip it; <c>Tools/unity-perf.sh</c> runs it and builds the table
    /// for <c>docs/testprotokolle/performance.md</c>.
    /// </summary>
    [Explicit("Measurement, not a check: run with Tools/unity-perf.sh")]
    [Category("Benchmark")]
    public class PerformanceBenchmark
    {
        private const float WarmUpSeconds = 3f;
        private const float MeasureSeconds = 20f;
        private const int MaxFrames = 20000;
        private const double NanosecondsPerMillisecond = 1e6;
        private const double BytesPerMegabyte = 1024.0 * 1024.0;
        // iPhone screen in portrait, so the editor renders about as many pixels as the device.
        private const int TargetWidth = 1179;
        private const int TargetHeight = 2556;

        private static int _camerasRendered;

        [UnityTest]
        public IEnumerator Stress_MegaBoomerAndFullWaves()
        {
            var scenario = new StressScenario();
            yield return scenario.Load(StressScenario.StressScene, StressScenario.FastestWeapon);
            yield return scenario.EnterStressArena();
            yield return Measure(StressScenario.StressScene, scenario);
        }

        [UnityTest]
        public IEnumerator Level([Values("Level_1-1", "Level_2-1", "Level_3-1")] string scene)
        {
            var scenario = new StressScenario();
            yield return scenario.Load(scene);
            yield return Measure(scene, scenario);
        }

        [UnityTest]
        public IEnumerator MainMenu()
        {
            var scenario = new StressScenario();
            yield return scenario.Load(UI.MainMenu.SceneName);
            yield return Measure(UI.MainMenu.SceneName, scenario);
        }

        private static IEnumerator Measure(string scene, StressScenario scenario)
        {
            float warmUp = 0f;
            while (warmUp < WarmUpSeconds)
            {
                scenario.Tick();
                warmUp += Time.unscaledDeltaTime;
                yield return null;
            }

            // Batch mode has no game view, so cameras only render into a texture.
            var target = new RenderTexture(TargetWidth, TargetHeight, 24);
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
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

            // Preallocated, so the measurement itself adds no garbage.
            var game = new FrameTimeStats(MaxFrames);
            var frame = new FrameTimeStats(MaxFrames);
            double scriptsTotal = 0;
            double physicsTotal = 0;
            long gcTotal = 0;
            long gcWorst = 0;
            int framesWithGc = 0;
            long peakTextures = 0;
            float elapsed = 0f;
            yield return null;

            while (elapsed < MeasureSeconds && frame.Count < MaxFrames)
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
                gcWorst = System.Math.Max(gcWorst, gc);
                framesWithGc += gc > 0 ? 1 : 0;
                peakTextures = System.Math.Max(peakTextures, textureMemory.Valid ? textureMemory.LastValue : 0);
            }

            RenderPipelineManager.endCameraRendering -= CountCamera;
            foreach (Camera camera in cameras)
            {
                if (camera != null)
                {
                    camera.targetTexture = null;
                }
            }

            target.Release();
            Object.Destroy(target);
            int measured = System.Math.Max(1, game.Count);
            var result = new Result
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
                camerasRenderedPerFrame = (float)_camerasRendered / measured,
                enemiesAlive = CountEnemies(),
                graphics = SystemInfo.graphicsDeviceType.ToString(),
            };

            string dir = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", "Logs", "perf");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, scene + ".json"), JsonUtility.ToJson(result, true));
            Debug.Log($"[Perf] {scene}: {JsonUtility.ToJson(result)}");
            Assert.Greater(frame.Count, 0);
        }

        private static void CountCamera(ScriptableRenderContext context, Camera camera)
        {
            _camerasRendered++;
        }

        // Recorders of markers that did not run this frame are invalid or 0.
        private static double Milliseconds(ProfilerRecorder recorder)
        {
            return recorder.Valid ? recorder.LastValue / NanosecondsPerMillisecond : 0.0;
        }

        private static int CountEnemies()
        {
            int alive = 0;
            foreach (Enemies.WaveDirector director in Object.FindObjectsByType<Enemies.WaveDirector>(FindObjectsSortMode.None))
            {
                alive += director.AliveCount;
            }

            return alive;
        }

        [System.Serializable]
        private sealed class Result
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
            public float camerasRenderedPerFrame;
            public int enemiesAlive;
            public string graphics;
        }
    }
}
