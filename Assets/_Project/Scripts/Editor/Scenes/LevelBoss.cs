using RushOps.Bosses;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// The boss of a boss level in a <see cref="LevelBlueprint"/> (issue #150): its definition, where it waits in the
    /// arena, the arena zone and the gate into it, behind which the door closes (decision C6). A boss level has no exit;
    /// beating the boss finishes it (F-LVL-05).
    /// </summary>
    internal sealed class LevelBoss
    {
        public LevelBoss(string definitionPath, Vector2 position, int arenaZone, int entryGate,
            System.Action<BossDefinition> configure)
        {
            DefinitionPath = definitionPath;
            Position = position;
            ArenaZone = arenaZone;
            EntryGate = entryGate;
            Configure = configure;
        }

        /// <summary>Asset path of the <see cref="BossDefinition"/>.</summary>
        public string DefinitionPath { get; }

        /// <summary>Where the boss stands until the fight starts (its feet).</summary>
        public Vector2 Position { get; }

        /// <summary>Index of the arena zone; entering it starts the fight.</summary>
        public int ArenaZone { get; }

        /// <summary>Index of the gate into the arena; the arena door closes in its gap.</summary>
        public int EntryGate { get; }

        /// <summary>Fills a newly created definition; an existing one keeps its tuned values.</summary>
        public System.Action<BossDefinition> Configure { get; }

        /// <summary>Type of a newly created definition, a <see cref="BossDefinition"/> or a subclass.</summary>
        public System.Type DefinitionType { get; set; } = typeof(BossDefinition);

        /// <summary>
        /// Creates the boss object in the scene; null builds the placeholder boss (a big brute). Bosses with own moves
        /// set their own builder.
        /// </summary>
        public System.Func<ArenaScene, BossDefinition, Vector2, Boss> Create { get; set; }
    }
}
