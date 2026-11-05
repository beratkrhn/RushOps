using System.IO;
using NUnit.Framework;
using RushOps.Characters;
using RushOps.Player;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class MuzzleMathTests
    {
        private const string MuzzlesPath = "Assets/_Project/Art/Sprites/Player/Player_Muzzles.json";

        [Test]
        public void Offset_TakesTheMuzzleOfTheShownDirection()
        {
            var muzzles = new Vector2[Direction8Math.Count];
            muzzles[(int)Direction8.W] = new Vector2(-0.3f, 0.8f);

            Assert.AreEqual(new Vector2(-0.3f, 0.8f), MuzzleMath.Offset(muzzles, Direction8.W, Vector2.zero));
        }

        [Test]
        public void Offset_WithoutACompleteTable_UsesTheFallback()
        {
            Assert.AreEqual(Vector2.one, MuzzleMath.Offset(null, Direction8.E, Vector2.one));
            Assert.AreEqual(Vector2.one, MuzzleMath.Offset(new Vector2[3], Direction8.E, Vector2.one));
        }

        [Test]
        public void MuzzleTable_HasAllEightDirections_InFrontOfTheBody()
        {
            string json = File.ReadAllText(MuzzlesPath);
            foreach (string direction in new[] { "\"E\"", "\"NE\"", "\"N\"", "\"NW\"", "\"W\"", "\"SW\"", "\"S\"", "\"SE\"" })
            {
                StringAssert.Contains(direction, json);
            }
        }
    }
}
