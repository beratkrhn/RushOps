using System.IO;
using NUnit.Framework;
using RushOps.Editor.Build;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class AppIconTests
    {
        [Test]
        public void Icon_Is1024Square_Opaque()
        {
            // Read the file itself: the imported texture may be scaled or compressed.
            var icon = new Texture2D(2, 2);
            Assert.IsTrue(icon.LoadImage(File.ReadAllBytes(AppIconSetup.IconPath)), "icon PNG unreadable");

            Assert.AreEqual(AppIconSetup.Size, icon.width);
            Assert.AreEqual(AppIconSetup.Size, icon.height);
            foreach (Color32 pixel in icon.GetPixels32())
            {
                if (pixel.a != 255)
                {
                    Assert.Fail("App Store icons must not be transparent");
                }
            }

            Object.DestroyImmediate(icon);
        }

        [Test]
        public void Icon_IsTheDefaultPlayerIcon()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconSetup.IconPath);
            Texture2D[] icons = PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Any);

            Assert.IsNotNull(icon);
            Assert.IsNotEmpty(icons, "run RushOps → Build → App-Icon setzen");
            Assert.AreSame(icon, icons[0]);
        }
    }
}
