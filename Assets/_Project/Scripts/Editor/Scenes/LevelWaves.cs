using RushOps.Enemies;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Waves of a zone or puzzle in a <see cref="LevelBlueprint"/>: the asset under <c>Data/Waves</c> and how to fill it
    /// when it is created. An existing asset keeps its tuned values.
    /// </summary>
    internal sealed class LevelWaves
    {
        public LevelWaves(string assetName, System.Action<WaveDefinition, LevelEnemies> configure)
        {
            AssetName = assetName;
            Configure = configure;
        }

        /// <summary>File name under <c>Data/Waves</c>, e.g. <c>Level_1-1_Zone1.asset</c>.</summary>
        public string AssetName { get; }

        /// <summary>Fills a newly created asset.</summary>
        public System.Action<WaveDefinition, LevelEnemies> Configure { get; }

        /// <summary>Path of the asset.</summary>
        public string Path => "Assets/_Project/Data/Waves/" + AssetName;
    }
}
