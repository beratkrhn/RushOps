using NUnit.Framework;
using RushOps.Enemies;
using UnityEditor;

namespace RushOps.Tests.EditMode
{
    /// <summary>Checks the start values in Data/Enemies/DogConfig.asset and Dog.asset (issue #262, backlog v1.5.0).</summary>
    public class DogAssetTests
    {
        private const string ConfigPath = "Assets/_Project/Data/Enemies/DogConfig.asset";
        private const string DogPath = "Assets/_Project/Data/Enemies/Dog.asset";
        private const string RunnerPath = "Assets/_Project/Data/Enemies/Runner.asset";

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.IsNotNull(asset, $"missing {path}; run RushOps → Daten → Hund erzeugen");
            return asset;
        }

        [Test]
        public void Config_PackOfThree_LungeStartValues()
        {
            var config = Load<DogConfig>(ConfigPath);

            Assert.AreEqual(3, config.PackSize);
            Assert.Greater(config.PackSpread, 0f, "small offset at the common spawn point");
            Assert.Less(config.PackSpread, 1f);
            Assert.AreEqual(3f, config.LungeTriggerDistance, 1e-5f);
            Assert.AreEqual(0.4f, config.CrouchDuration, 1e-5f);
            Assert.AreEqual(3f, config.LungeDistance, 1e-5f);
            Assert.AreEqual(0.3f, config.LungeDuration, 1e-5f);
            Assert.AreEqual(1.5f, config.RecoveryDuration, 1e-5f);
        }

        [Test]
        public void Dog_LikeTheRunner_OneHeart_InPacks()
        {
            var dog = Load<EnemyDefinition>(DogPath);
            var runner = Load<EnemyDefinition>(RunnerPath);

            Assert.AreEqual(runner.Health, dog.Health);
            Assert.AreEqual(runner.MoveSpeed, dog.MoveSpeed, 1e-5f);
            Assert.AreEqual(1, dog.ContactDamage);
            Assert.AreEqual(runner.Points, dog.Points);
            Assert.AreEqual(runner.Gold, dog.Gold);
            Assert.IsFalse(dog.IsBoss);
            Assert.AreSame(Load<DogConfig>(ConfigPath), dog.Dog);
            Assert.AreEqual(3, dog.PackSize);
        }
    }
}
