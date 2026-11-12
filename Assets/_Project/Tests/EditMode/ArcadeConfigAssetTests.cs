using NUnit.Framework;
using RushOps.Arcade;
using RushOps.Editor.Arcade;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class ArcadeConfigAssetTests
    {
        private ArcadeConfig _config;

        [SetUp]
        public void Load()
        {
            _config = AssetDatabase.LoadAssetAtPath<ArcadeConfig>(ArcadeConfigBuilder.ConfigPath);
            Assert.IsNotNull(_config, ArcadeConfigBuilder.ConfigPath);
        }

        [Test]
        public void Screen_IsAboutSquareAndShowsTenByTenTiles()
        {
            Assert.AreEqual(new Vector2Int(360, 640), _config.Reference);
            Assert.AreEqual(new Vector2Int(320, 320), _config.Screen.size);
            Assert.AreEqual(new Vector2Int(320, 320), _config.ScreenResolution);
            Assert.AreEqual(10f, _config.ScreenResolution.x / 32f);
        }

        [Test]
        public void Parts_StackFromMarqueeToPanel()
        {
            Assert.AreEqual(0, _config.Marquee.y);
            Assert.AreEqual(_config.Marquee.yMax, _config.Bezel.y);
            Assert.AreEqual(_config.Bezel.yMax, _config.Panel.y);
            Assert.AreEqual(_config.Reference.y, _config.Panel.yMax);
            Assert.IsTrue(Contains(_config.Bezel, _config.Screen), "screen inside the bezel");
            Assert.IsTrue(Contains(_config.Panel, _config.StickLeft), "left stick on the panel");
            Assert.IsTrue(Contains(_config.Panel, _config.StickRight), "right stick on the panel");
            Assert.IsTrue(Contains(_config.Panel, _config.DashButton), "dash button on the panel");
            Assert.Less(_config.StickLeft.center.x, _config.DashButton.center.x);
            Assert.Greater(_config.StickRight.center.x, _config.DashButton.center.x);
        }

        [Test]
        public void Sprites_AreLinked()
        {
            Assert.IsNotNull(_config.MarqueeSprite);
            Assert.IsNotNull(_config.BezelSprite);
            Assert.IsNotNull(_config.ScreenFrameSprite);
            Assert.IsNotNull(_config.ScreenGlossSprite);
            Assert.IsNotNull(_config.PanelSprite);
            Assert.IsNotNull(_config.BodySprite);
            Assert.AreEqual(9, _config.StickSprites.Length);
            CollectionAssert.AllItemsAreNotNull(_config.StickSprites);
            Assert.IsNotNull(_config.DashSprite);
            Assert.IsNotNull(_config.DashPressedSprite);
            Assert.AreEqual(_config.DecoButtons.Length, _config.DecoSprites.Length);
            CollectionAssert.AllItemsAreNotNull(_config.DecoSprites);
        }

        private static bool Contains(RectInt outer, RectInt inner)
        {
            return inner.xMin >= outer.xMin && inner.yMin >= outer.yMin && inner.xMax <= outer.xMax && inner.yMax <= outer.yMax;
        }
    }
}
