using System.Collections.Generic;
using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Perks;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Packs of dogs in the spawn system (issue #262, F-GEG-05, decisions H4 and H5).</summary>
    public class PackRulesTests
    {
        private sealed class FixedRandom : IRandomSource
        {
            private readonly float _value;

            public FixedRandom(float value) => _value = value;

            public float Value() => _value;
        }

        private readonly List<Object> _created = new List<Object>();
        private EnemyDefinition _walker;
        private EnemyDefinition _dog;

        [SetUp]
        public void Create()
        {
            _walker = Make<EnemyDefinition>();
            var config = Make<DogConfig>();
            config.ConfigurePack(3, 0.5f);
            _dog = Make<EnemyDefinition>();
            var serialized = new UnityEditor.SerializedObject(_dog);
            serialized.FindProperty("_dog").objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Destroy()
        {
            foreach (Object o in _created)
            {
                Object.DestroyImmediate(o);
            }

            _created.Clear();
        }

        [Test]
        public void Definition_PackSize_ThreeForDogs_OneForOthers()
        {
            Assert.AreEqual(3, _dog.PackSize);
            Assert.AreEqual(0.5f, _dog.PackSpread);
            Assert.AreEqual(1, _walker.PackSize);
            Assert.AreEqual(0f, _walker.PackSpread);
        }

        [TestCase(3, 3, true)]
        [TestCase(5, 3, true)]
        [TestCase(2, 3, false)]
        [TestCase(0, 3, false)]
        [TestCase(1, 1, true)]
        public void CanSpawn_OnlyWithAllPlacesFree(int free, int packSize, bool expected)
        {
            Assert.AreEqual(expected, PackRules.CanSpawn(free, packSize));
        }

        [TestCase(18, 18)]
        [TestCase(16, 18)]
        [TestCase(17, 18)]
        [TestCase(1, 3)]
        [TestCase(0, 0)]
        public void RoundUpToPacks_FullPacksOfThree(int count, int expected)
        {
            Assert.AreEqual(expected, PackRules.RoundUpToPacks(count, 3));
            Assert.AreEqual(expected / 3, PackRules.PackCount(count, 3));
        }

        [Test]
        public void Offsets_ThreeDogsAroundOnePoint_SmallEvenSpread()
        {
            var center = new Vector2(10f, 5f);
            var results = new Vector2[3];

            int written = PackRules.Offsets(center, 0.5f, 3, new FixedRandom(0.25f), results);

            Assert.AreEqual(3, written);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(0.5f, (results[i] - center).magnitude, 1e-4f, "small offset from the spawn point");
                Vector2 next = results[(i + 1) % 3];
                Assert.AreEqual(0.5f * Mathf.Sqrt(3f), (next - results[i]).magnitude, 1e-4f, "evenly apart");
            }

            Assert.AreEqual(center.x, results[0].x, 1e-4f, "random start angle of a quarter turn");
            Assert.AreEqual(center.y + 0.5f, results[0].y, 1e-4f);
        }

        [Test]
        public void Offsets_SingleOrWithoutSpread_OnThePoint()
        {
            var results = new Vector2[3];

            Assert.AreEqual(1, PackRules.Offsets(Vector2.one, 0.5f, 1, new FixedRandom(0.5f), results));
            Assert.AreEqual(Vector2.one, results[0]);
            Assert.AreEqual(3, PackRules.Offsets(Vector2.one, 0f, 3, new FixedRandom(0.5f), results));
            Assert.AreEqual(Vector2.one, results[2]);
        }

        [Test]
        public void Offsets_LimitedByTheResultBuffer()
        {
            Assert.AreEqual(2, PackRules.Offsets(Vector2.zero, 0.5f, 3, new FixedRandom(0f), new Vector2[2]));
        }

        [Test]
        public void Queue_ThreeDogsInARow_JoinOneRequest()
        {
            var queue = new SpawnQueue();
            for (int i = 0; i < 7; i++)
            {
                queue.Enqueue(new SpawnRequest(0, _dog));
            }

            Assert.AreEqual(3, queue.Count, "two full packs and one started");
            Assert.AreEqual(3, queue[0].Count);
            Assert.AreEqual(3, queue[1].Count);
            Assert.AreEqual(1, queue[2].Count);
            Assert.AreEqual(3, queue[0].SlotCost, "a pack takes 3 places (H4)");
        }

        [Test]
        public void Queue_OtherZoneOrType_StartsANewRequest()
        {
            var queue = new SpawnQueue();
            queue.Enqueue(new SpawnRequest(0, _dog));
            queue.Enqueue(new SpawnRequest(1, _dog));
            queue.Enqueue(new SpawnRequest(1, _walker));
            queue.Enqueue(new SpawnRequest(1, _walker));

            Assert.AreEqual(4, queue.Count, "walkers never join");
        }

        [Test]
        public void Queue_PackWithOnlyTwoFreePlaces_Waits()
        {
            var queue = new SpawnQueue();
            for (int i = 0; i < 3; i++)
            {
                queue.Enqueue(new SpawnRequest(0, _dog));
            }

            Assert.AreEqual(-1, queue.FindReady(2, null), "only 2 free places");
            Assert.AreEqual(0, queue.FindReady(3, null));
        }

        [Test]
        public void Queue_SingleEnemiesStillNeedOnePlace()
        {
            var queue = new SpawnQueue();
            queue.Enqueue(new SpawnRequest(0, _walker));

            Assert.AreEqual(-1, queue.FindReady(0, null));
            Assert.AreEqual(0, queue.FindReady(1, null));
        }

        [Test]
        public void WaveSequence_HandsOutAWholePackAtOnce_LastOneSmaller()
        {
            var waves = Make<WaveDefinition>();
            waves.Configure(new[] { new Wave(new WaveEntry(_dog, 7)) }, 1f, 0f, AfterLastWave.End);
            var sequence = new WaveSequence(waves);
            int requested = 0;
            sequence.SpawnRequested += _ => requested++;

            sequence.Start();
            sequence.Tick(0f);
            Assert.AreEqual(3, requested, "first pack");

            sequence.Tick(1f);
            Assert.AreEqual(6, requested, "second pack one interval later");

            sequence.Tick(1f);
            Assert.AreEqual(7, requested, "the rest");
            Assert.AreEqual(WavePhase.Clearing, sequence.Phase);
            Assert.AreEqual(7, sequence.Outstanding);
        }

        [Test]
        public void Request_Count_AtLeastOne_BossesTakeNoPlaces()
        {
            Assert.AreEqual(1, new SpawnRequest(0, _walker, 0).Count);
            var boss = Make<EnemyDefinition>();
            var serialized = new UnityEditor.SerializedObject(boss);
            serialized.FindProperty("_isBoss").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Assert.AreEqual(0, new SpawnRequest(0, boss).SlotCost);
        }

        private T Make<T>() where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            _created.Add(o);
            return o;
        }
    }
}
