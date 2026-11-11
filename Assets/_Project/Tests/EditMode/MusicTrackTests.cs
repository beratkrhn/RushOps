using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>The two chiptune tracks made by <c>Tools/gen_music.py</c> (issue #298, F-AUD-01).</summary>
    public class MusicTrackTests
    {
        private const string MusicDir = "Assets/_Project/Audio/Music/";

        [TestCase("Music_Menu", 30f, 45f)]
        [TestCase("Music_Game", 60f, 90f)]
        public void Track_Exists_WithALoopOfTheExpectedLength(string name, float min, float max)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicDir + name + ".wav");

            Assert.IsNotNull(clip, $"no clip at {MusicDir}{name}.wav; run Tools/gen_music.py");
            Assert.That(clip.length, Is.InRange(min, max));
            Assert.AreEqual(1, clip.channels, "music is mono");
        }

        [TestCase("Music_Menu")]
        [TestCase("Music_Game")]
        public void Track_IsStreamedVorbis(string name)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(MusicDir + name + ".wav");

            Assert.IsNotNull(importer);
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            Assert.AreEqual(AudioClipLoadType.Streaming, settings.loadType, "music streams instead of sitting in memory");
            Assert.AreEqual(AudioCompressionFormat.Vorbis, settings.compressionFormat);
            Assert.IsTrue(importer.forceToMono);
        }
    }
}
