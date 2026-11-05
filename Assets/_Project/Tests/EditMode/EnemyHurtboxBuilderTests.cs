using NUnit.Framework;
using RushOps.Characters;
using RushOps.Editor.Art;
using RushOps.Editor.Scenes;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class EnemyHurtboxBuilderTests
    {
        [Test]
        public void ToLocalUnits_IsRelativeToTheFootPivot()
        {
            // 64x64 canvas, pivot bottom center (32, 0); body from x 12 to 51 and y 1 to 56.
            Rect local = EnemyHurtboxBuilder.ToLocalUnits(new RectInt(12, 1, 40, 56), new Vector2(32f, 0f), 32f);

            Assert.AreEqual(-20f / 32f, local.xMin, 1e-5f);
            Assert.AreEqual(20f / 32f, local.xMax, 1e-5f);
            Assert.AreEqual(1f / 32f, local.yMin, 1e-5f);
            Assert.AreEqual(57f / 32f, local.yMax, 1e-5f);
        }

        [TestCase(DirectionalAnimationBuilder.WalkerWalkPath)]
        [TestCase(DirectionalAnimationBuilder.BruteWalkPath)]
        public void BodyBounds_CoverTheWholeFigure_NotJustTheFeet(string path)
        {
            var walk = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(path);
            Assert.IsNotNull(walk, path);
            Rect bounds = EnemyHurtboxBuilder.BodyBounds(walk);

            Sprite frame = walk.GetFrame(Direction8.S, 0);
            float spriteHeight = frame.rect.height / frame.pixelsPerUnit;
            Assert.Less(bounds.yMin, 0.2f, "reaches down to the feet");
            Assert.Greater(bounds.yMax, spriteHeight * 0.75f, "reaches up to the head");
            Assert.Less(bounds.xMin, -0.4f);
            Assert.Greater(bounds.xMax, 0.4f);
        }
    }
}
