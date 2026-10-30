using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class WaveSequenceTests
    {
        private readonly List<Object> _created = new List<Object>();
        private EnemyDefinition _walker;
        private EnemyDefinition _runner;

        [SetUp]
        public void CreateEnemies()
        {
            _walker = Create<EnemyDefinition>();
            _runner = Create<EnemyDefinition>();
        }

        [TearDown]
        public void DestroyCreated()
        {
            foreach (Object o in _created)
            {
                Object.DestroyImmediate(o);
            }

            _created.Clear();
        }

        [Test]
        public void New_IsIdleAndRequestsNothing()
        {
            var sequence = new WaveSequence(Waves(AfterLastWave.End, 1f, 2f, new Wave(new WaveEntry(_walker, 3))));
            var requests = Record(sequence);

            sequence.Tick(10f);

            Assert.AreEqual(WavePhase.Idle, sequence.Phase);
            Assert.IsEmpty(requests);
        }

        [Test]
        public void Start_FirstEnemyAtOnceThenOnePerInterval()
        {
            var sequence = new WaveSequence(Waves(AfterLastWave.End, 1f, 2f, new Wave(new WaveEntry(_walker, 3))));
            var requests = Record(sequence);
            sequence.Start();

            sequence.Tick(0.01f);
            Assert.AreEqual(1, requests.Count);
            sequence.Tick(0.5f);
            Assert.AreEqual(1, requests.Count);
            sequence.Tick(0.5f);
            Assert.AreEqual(2, requests.Count);
            sequence.Tick(1f);

            Assert.AreEqual(3, requests.Count);
            Assert.AreEqual(WavePhase.Clearing, sequence.Phase);
            Assert.AreEqual(3, sequence.Outstanding);
        }

        [Test]
        public void MixedWave_AlternatesTypes()
        {
            var wave = new Wave(new WaveEntry(_walker, 3), new WaveEntry(_runner, 1));
            var sequence = new WaveSequence(Waves(AfterLastWave.End, 0f, 0f, wave));
            var requests = Record(sequence);
            sequence.Start();

            sequence.Tick(0.01f);

            CollectionAssert.AreEqual(new[] { _walker, _runner, _walker, _walker }, requests);
        }

        [Test]
        public void NextWave_StartsOnlyAfterAllKilledAndPause()
        {
            var sequence = new WaveSequence(Waves(AfterLastWave.End, 0f, 2f,
                new Wave(new WaveEntry(_walker, 2)), new Wave(new WaveEntry(_walker, 1))));
            var requests = Record(sequence);
            var started = new List<int>();
            sequence.WaveStarted += started.Add;
            sequence.Start();
            sequence.Tick(0.01f);

            sequence.NotifyKilled();
            sequence.Tick(5f);
            Assert.AreEqual(WavePhase.Clearing, sequence.Phase, "one enemy of wave 1 still alive");

            sequence.NotifyKilled();
            sequence.Tick(0.01f);
            Assert.AreEqual(WavePhase.Pause, sequence.Phase);
            Assert.AreEqual(1, sequence.WaveIndex);
            sequence.Tick(1.9f);
            Assert.AreEqual(2, requests.Count);
            sequence.Tick(0.2f);

            Assert.AreEqual(3, requests.Count);
            CollectionAssert.AreEqual(new[] { 0, 1 }, started);
        }

        [TestCase(AfterLastWave.RepeatAll, 0)]
        [TestCase(AfterLastWave.RepeatLast, 1)]
        public void AfterLastWave_Repeats(AfterLastWave after, int expectedWave)
        {
            var sequence = new WaveSequence(Waves(after, 0f, 0f,
                new Wave(new WaveEntry(_walker, 1)), new Wave(new WaveEntry(_runner, 1))));
            sequence.Start();

            for (int i = 0; i < 2; i++)
            {
                sequence.Tick(0.01f);
                sequence.NotifyKilled();
                sequence.Tick(0.01f);
            }

            sequence.Tick(0.01f);

            Assert.AreEqual(expectedWave, sequence.WaveIndex);
            Assert.AreEqual(WavePhase.Clearing, sequence.Phase);
        }

        [Test]
        public void AfterLastWave_End_Finishes()
        {
            var sequence = new WaveSequence(Waves(AfterLastWave.End, 0f, 0f, new Wave(new WaveEntry(_walker, 1))));
            var requests = Record(sequence);
            sequence.Start();
            sequence.Tick(0.01f);
            sequence.NotifyKilled();

            sequence.Tick(10f);

            Assert.AreEqual(WavePhase.Finished, sequence.Phase);
            Assert.AreEqual(1, requests.Count);
        }

        [Test]
        public void Stop_RequestsNoMoreEnemies()
        {
            var sequence = new WaveSequence(Waves(AfterLastWave.RepeatAll, 1f, 0f, new Wave(new WaveEntry(_walker, 5))));
            var requests = Record(sequence);
            sequence.Start();
            sequence.Tick(0.01f);

            sequence.Stop();
            sequence.Tick(10f);

            Assert.AreEqual(WavePhase.Finished, sequence.Phase);
            Assert.AreEqual(1, requests.Count);
        }

        [Test]
        public void Start_WithoutWaves_Finishes()
        {
            var sequence = new WaveSequence(null);

            sequence.Start();

            Assert.AreEqual(WavePhase.Finished, sequence.Phase);
        }

        [Test]
        public void EmptyWaves_RepeatAll_DoesNotHang()
        {
            var sequence = new WaveSequence(Waves(AfterLastWave.RepeatAll, 0f, 0f, new Wave()));
            var requests = Record(sequence);
            sequence.Start();

            for (int i = 0; i < 10; i++)
            {
                sequence.Tick(0.1f);
            }

            Assert.IsEmpty(requests);
        }

        private WaveDefinition Waves(AfterLastWave after, float interval, float pause, params Wave[] waves)
        {
            var definition = Create<WaveDefinition>();
            definition.Configure(waves, interval, pause, after);
            return definition;
        }

        private static List<EnemyDefinition> Record(WaveSequence sequence)
        {
            var requests = new List<EnemyDefinition>();
            sequence.SpawnRequested += requests.Add;
            return requests;
        }

        private T Create<T>() where T : ScriptableObject
        {
            var instance = ScriptableObject.CreateInstance<T>();
            _created.Add(instance);
            return instance;
        }
    }
}
