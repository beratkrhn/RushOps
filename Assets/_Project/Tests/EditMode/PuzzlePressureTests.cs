using NUnit.Framework;
using RushOps.Enemies;
using RushOps.Puzzles;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class PuzzlePressureTests
    {
        [Test]
        public void WavesAfter_Resume_ReturnsZoneWaves()
        {
            var zoneWaves = ScriptableObject.CreateInstance<WaveDefinition>();

            Assert.AreSame(zoneWaves, PuzzlePressure.WavesAfter(AfterPuzzle.ResumeZoneWaves, zoneWaves));
            Object.DestroyImmediate(zoneWaves);
        }

        [Test]
        public void WavesAfter_End_ReturnsNone()
        {
            var zoneWaves = ScriptableObject.CreateInstance<WaveDefinition>();

            Assert.IsNull(PuzzlePressure.WavesAfter(AfterPuzzle.EndWaves, zoneWaves));
            Object.DestroyImmediate(zoneWaves);
        }

        [Test]
        public void WaveSequence_KnowsItsDefinition()
        {
            var waves = ScriptableObject.CreateInstance<WaveDefinition>();

            Assert.AreSame(waves, new WaveSequence(waves).Definition);
            Object.DestroyImmediate(waves);
        }
    }
}
