using NUnit.Framework;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Every regular enemy type has its own crawl-out frames (issue #314, F-END-15).</summary>
    public class EnemyEmergeFramesTests
    {
        [TestCase("Walker")]
        [TestCase("Runner")]
        [TestCase("Brute")]
        [TestCase("Boomer")]
        [TestCase("Dog")]
        public void EnemyType_HasSixDistinctCrawlOutFrames(string type)
        {
            var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>($"Assets/_Project/Data/Enemies/{type}.asset");

            Assert.IsNotNull(definition);
            Sprite[] frames = definition.EmergeFrames;
            Assert.AreEqual(6, frames.Length);
            CollectionAssert.AllItemsAreNotNull(frames);
            CollectionAssert.AllItemsAreUnique(frames);
            foreach (Sprite frame in frames)
            {
                StringAssert.Contains($"{type}_Emerge_", frame.name);
            }
        }

        [Test]
        public void TheFrameSetsDiffer_BetweenTypes()
        {
            var walker = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/Data/Enemies/Walker.asset");
            var brute = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/Data/Enemies/Brute.asset");

            CollectionAssert.AreNotEqual(walker.EmergeFrames, brute.EmergeFrames);
        }
    }
}
