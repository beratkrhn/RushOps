using NUnit.Framework;
using RushOps.Save;

namespace RushOps.Tests.PlayMode
{
    /// <summary>
    /// Points the save game service at memory for every PlayMode test (issue #123): scenes save at the end of a level
    /// and when they unload, and tests must not change the developer's save game in <c>persistentDataPath</c>.
    /// </summary>
    [SetUpFixture]
    public class SaveGameIsolation
    {
        private const string ServicePath = "Assets/_Project/Data/Save/SaveGameService.asset";

        /// <summary>The files of the current in-memory save.</summary>
        public static InMemorySaveFiles Files { get; private set; }

        /// <summary>The service the scenes use; null outside the editor.</summary>
        public static SaveGameService Service { get; private set; }

        /// <summary>Starts a new empty in-memory save game and returns the service.</summary>
        public static SaveGameService Reset()
        {
            Files = new InMemorySaveFiles();
            if (Service != null)
            {
                Service.Use(new SaveGameStore(Files, "playmode-tests"));
            }

            return Service;
        }

        [OneTimeSetUp]
        public void Isolate()
        {
#if UNITY_EDITOR
            Service = UnityEditor.AssetDatabase.LoadAssetAtPath<SaveGameService>(ServicePath);
#endif
            Reset();
        }
    }
}
