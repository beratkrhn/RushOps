using RushOps.Bosses;
using RushOps.Characters;
using RushOps.Editor.Art;
using RushOps.Enemies;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>Creates the Brutmutter boss (issue #155) with her animations, tied to the level's runner pool.</summary>
    internal static class BroodMotherBuilder
    {
        /// <summary>Definition of the Brutmutter.</summary>
        public const string DefinitionPath = "Assets/_Project/Data/Bosses/Brutmutter.asset";

        private const string RunnerPoolName = "EnemyPool_Runner";

        // Body at the feet of a 96 px figure, a bit wider than the brute's for the egg sac.
        private const float ColliderRadius = 0.75f;
        private static readonly Vector2 ColliderOffset = new Vector2(0f, 0.75f);

        /// <summary>
        /// Starting values (decisions C5, C10): about 90 s without perks at 6 shots per second, with time lost on her
        /// runners; slow; 1000 points. The brood values keep the defaults of <see cref="BroodMotherDefinition"/> (C8).
        /// </summary>
        public static void Configure(BossDefinition definition)
        {
            definition.Configure("Brutmutter", 320, 0.9f, 1, 1000);
        }

        /// <summary>Creates the Brutmutter at <paramref name="position"/> (her feet).</summary>
        public static Boss Create(ArenaScene arena, BossDefinition definition, Vector2 position)
        {
            DirectionalAnimationBuilder.BuildAll();
            BroodMother mother = BossBuilder.CreateBody<BroodMother>(
                "Brutmutter", arena, definition, position, Load(DirectionalAnimationBuilder.BrutmutterWalkPath),
                Load(DirectionalAnimationBuilder.BrutmutterHitPath), Load(DirectionalAnimationBuilder.BrutmutterDeathPath),
                1f, ColliderRadius, ColliderOffset);
            var view = new SerializedObject(mother.GetComponent<DirectionalSpriteAnimator>());
            view.FindProperty("_alert").objectReferenceValue = Load(DirectionalAnimationBuilder.BrutmutterBirthPath);
            view.ApplyModifiedPropertiesWithoutUndo();

            var serialized = new SerializedObject(mother);
            serialized.FindProperty("_brood").objectReferenceValue = RunnerPool(arena);
            serialized.FindProperty("_director").objectReferenceValue = arena.Director;
            serialized.FindProperty("_wallLayers").intValue = 1 << ArenaTestSceneBuilder.RequireLayer(ArenaTestSceneBuilder.WallsLayer);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return mother;
        }

        private static EnemyPool RunnerPool(ArenaScene arena)
        {
            foreach (EnemyPool pool in arena.Enemies)
            {
                if (pool != null && pool.name == RunnerPoolName)
                {
                    return pool;
                }
            }

            throw new System.InvalidOperationException($"No {RunnerPoolName} in the level");
        }

        private static DirectionalAnimation Load(string path)
        {
            var animation = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(path);
            if (animation == null)
            {
                throw new System.InvalidOperationException($"Animation not found at {path}; run Tools/gen_pixelart.py");
            }

            return animation;
        }
    }
}
