using NUnit.Framework;
using RushOps.Enemies;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class SpawnQueueTests
    {
        private EnemyDefinition _walker;
        private EnemyDefinition _boss;

        [SetUp]
        public void CreateEnemies()
        {
            _walker = ScriptableObject.CreateInstance<EnemyDefinition>();
            _boss = ScriptableObject.CreateInstance<EnemyDefinition>();
            var serialized = new UnityEditor.SerializedObject(_boss);
            serialized.FindProperty("_isBoss").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void DestroyEnemies()
        {
            Object.DestroyImmediate(_walker);
            Object.DestroyImmediate(_boss);
        }

        [Test]
        public void Enqueue_KeepsOrder()
        {
            var queue = new SpawnQueue();
            for (int i = 0; i < 40; i++)
            {
                queue.Enqueue(new SpawnRequest(i, _walker));
            }

            Assert.AreEqual(40, queue.Count);
            for (int i = 0; i < 40; i++)
            {
                Assert.AreEqual(i, queue.RemoveAt(0).Owner);
            }

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void PushFront_ComesFirst()
        {
            var queue = new SpawnQueue();
            queue.Enqueue(new SpawnRequest(1, _walker));

            queue.PushFront(new SpawnRequest(2, _walker));

            Assert.AreEqual(2, queue[0].Owner);
            Assert.AreEqual(1, queue[1].Owner);
        }

        [Test]
        public void RemoveOwner_DropsOnlyThatZoneAndKeepsOrder()
        {
            var queue = new SpawnQueue();
            queue.Enqueue(new SpawnRequest(0, _walker));
            queue.Enqueue(new SpawnRequest(1, _walker));
            queue.Enqueue(new SpawnRequest(0, _walker));
            queue.Enqueue(new SpawnRequest(2, _walker));

            Assert.AreEqual(2, queue.RemoveOwner(0));

            Assert.AreEqual(2, queue.Count);
            Assert.AreEqual(1, queue[0].Owner);
            Assert.AreEqual(2, queue[1].Owner);
        }

        [Test]
        public void FindReady_NoFreeSlot_WaitsUnlessBoss()
        {
            var queue = new SpawnQueue();
            queue.Enqueue(new SpawnRequest(0, _walker));
            queue.Enqueue(new SpawnRequest(0, _boss));

            Assert.AreEqual(0, queue.FindReady(1, null));
            Assert.AreEqual(1, queue.FindReady(0, null), "bosses ignore the limit");
        }

        [Test]
        public void FindReady_SkipsRequestsWithoutSpawnPoint()
        {
            var queue = new SpawnQueue();
            queue.Enqueue(new SpawnRequest(0, _walker));
            queue.Enqueue(new SpawnRequest(1, _walker));

            Assert.AreEqual(1, queue.FindReady(5, r => r.Owner == 1));
            Assert.AreEqual(-1, queue.FindReady(5, r => false));
        }

        [Test]
        public void FreeSlots_LimitCountsAliveAndAnnounced()
        {
            // The director asks the queue with the slots left under the cap of 10 (F-GEG-03).
            Assert.AreEqual(0, SpawnRules.FreeSlots(10, 8, 2));
            Assert.AreEqual(3, SpawnRules.FreeSlots(10, 6, 1));
        }
    }
}
