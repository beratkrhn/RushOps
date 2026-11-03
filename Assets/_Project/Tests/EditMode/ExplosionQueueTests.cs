using NUnit.Framework;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class ExplosionQueueTests
    {
        [Test]
        public void NoDelay_IsDueAtOnce()
        {
            var queue = new ExplosionQueue();
            queue.Schedule(new Vector2(3f, 4f), null, 0f);

            Assert.IsTrue(queue.TryTakeDue(out PendingExplosion explosion));
            Assert.AreEqual(new Vector2(3f, 4f), explosion.Position);
            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void ChainDelay_GoesOffOnlyAfterTheDelay()
        {
            var queue = new ExplosionQueue();
            queue.Schedule(Vector2.zero, null, 0.12f);

            queue.Advance(1f / 60f);
            Assert.IsFalse(queue.TryTakeDue(out _), "a chain explosion must wait a few frames");
            for (int i = 0; i < 7; i++)
            {
                queue.Advance(1f / 60f);
            }

            Assert.IsTrue(queue.TryTakeDue(out _));
            Assert.IsFalse(queue.TryTakeDue(out _), "each explosion goes off once");
        }

        [Test]
        public void SeveralDue_ComeOutOldestFirst()
        {
            var queue = new ExplosionQueue();
            queue.Schedule(new Vector2(1f, 0f), null, 0.1f);
            queue.Schedule(new Vector2(2f, 0f), null, 0.05f);
            queue.Schedule(new Vector2(3f, 0f), null, 1f);

            queue.Advance(0.1f);

            Assert.IsTrue(queue.TryTakeDue(out PendingExplosion first));
            Assert.IsTrue(queue.TryTakeDue(out PendingExplosion second));
            Assert.IsFalse(queue.TryTakeDue(out _));
            Assert.AreEqual(1f, first.Position.x);
            Assert.AreEqual(2f, second.Position.x);
            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void Clear_DropsEverything()
        {
            var queue = new ExplosionQueue();
            queue.Schedule(Vector2.zero, null, 0f);
            queue.Clear();

            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(queue.TryTakeDue(out _));
        }
    }
}
