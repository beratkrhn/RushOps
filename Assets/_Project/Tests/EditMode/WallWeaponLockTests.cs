using NUnit.Framework;
using RushOps.Combat;
using UnityEditor;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    /// <summary>Locked state of a wall weapon in the endless mode (issue #209): dark silhouette, then free with a glow.</summary>
    public class WallWeaponLockTests
    {
        private GameObject _go;
        private WallWeapon _wall;
        private SpriteRenderer _silhouette;
        private WallWeaponConfig _config;

        [SetUp]
        public void Create()
        {
            _go = new GameObject("Wall");
            _silhouette = _go.AddComponent<SpriteRenderer>();
            _config = ScriptableObject.CreateInstance<WallWeaponConfig>();
            _wall = _go.AddComponent<WallWeapon>();
            var serialized = new SerializedObject(_wall);
            serialized.FindProperty("_silhouette").objectReferenceValue = _silhouette;
            serialized.FindProperty("_config").objectReferenceValue = _config;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void Destroy()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void New_IsNotLocked_AsInTheCampaign()
        {
            Assert.IsFalse(_wall.IsLocked);
            Assert.AreEqual(Color.white, _silhouette.color);
        }

        [Test]
        public void SetLocked_MakesTheSilhouetteDark()
        {
            _wall.SetLocked(true);

            Assert.IsTrue(_wall.IsLocked);
            Assert.AreEqual(_config.LockedTint, _silhouette.color);
            Assert.Less(_silhouette.color.r, 0.5f, "a dark shape");
        }

        [Test]
        public void Unlock_FreesItWithAGlow()
        {
            _wall.SetLocked(true);

            _wall.Unlock();

            Assert.IsFalse(_wall.IsLocked);
            Assert.AreEqual(_config.UnlockGlowTint, _silhouette.color, "the glow starts bright and fades out afterwards");
        }

        [Test]
        public void Unlock_WithoutLock_DoesNothing()
        {
            _wall.Unlock();

            Assert.AreEqual(Color.white, _silhouette.color);
        }

        [Test]
        public void SetLockedFalse_FreesItWithoutGlow()
        {
            _wall.SetLocked(true);

            _wall.SetLocked(false);

            Assert.IsFalse(_wall.IsLocked);
            Assert.AreEqual(Color.white, _silhouette.color);
        }
    }
}
