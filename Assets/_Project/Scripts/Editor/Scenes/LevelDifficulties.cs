using RushOps.Levels;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// The difficulty assets of the three islands (decision C3) under <c>Data/Levels</c>. New assets start with the
    /// decided values; existing ones keep their tuned values.
    /// </summary>
    internal static class LevelDifficulties
    {
        private const string Dir = "Assets/_Project/Data/Levels/";

        /// <summary>Insel 1: as set in waves and configs.</summary>
        public const string Island1Path = Dir + "Difficulty_Insel1.asset";

        /// <summary>Insel 2: waves 20 % denser, hold zones 3 s longer.</summary>
        public const string Island2Path = Dir + "Difficulty_Insel2.asset";

        /// <summary>Insel 3: like island 2 and switch time limits −20 %.</summary>
        public const string Island3Path = Dir + "Difficulty_Insel3.asset";

        /// <summary>Path of the difficulty of <paramref name="island"/> (0-based).</summary>
        public static string PathOf(int island)
        {
            return island <= 0 ? Island1Path : island == 1 ? Island2Path : Island3Path;
        }

        /// <summary>Loads the difficulty at <paramref name="path"/> or creates it with the starting values.</summary>
        public static LevelDifficulty LoadOrCreate(string path)
        {
            bool exists = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDifficulty>(path) != null;
            var difficulty = ArenaTestSceneBuilder.LoadOrCreateConfig<LevelDifficulty>(path);
            if (!exists)
            {
                if (path == Island2Path)
                {
                    difficulty.Configure(1.2f, 1f, 3f);
                }
                else if (path == Island3Path)
                {
                    difficulty.Configure(1.2f, 0.8f, 3f);
                }
                else
                {
                    difficulty.Configure(1f, 1f, 0f);
                }

                UnityEditor.EditorUtility.SetDirty(difficulty);
                UnityEditor.AssetDatabase.SaveAssets();
            }

            return difficulty;
        }
    }
}
