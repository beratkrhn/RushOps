using RushOps.Bosses;
using RushOps.Characters;
using RushOps.Editor.Art;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>Creates the Koloss boss (issue #151) with its animations and the floor line of its charge.</summary>
    internal static class ColossusBuilder
    {
        /// <summary>Definition of the Koloss.</summary>
        public const string DefinitionPath = "Assets/_Project/Data/Bosses/Koloss.asset";

        private const string LineSpritePath = "Assets/_Project/Art/Sprites/Effects/Koloss_ChargeLine.png";

        // Body at the feet of a 128 px figure: 1.3 times the brute's.
        private const float ColliderRadius = 0.85f;
        private static readonly Vector2 ColliderOffset = new Vector2(0f, 0.85f);

        /// <summary>
        /// Starting values (decisions C5, C7, C10): about 60 s without perks at 6 shots per second, 1000 points.
        /// </summary>
        public static void Configure(BossDefinition definition)
        {
            definition.Configure("Koloss", 250, 1.4f, 1, 1000);
        }

        /// <summary>Creates the Koloss at <paramref name="position"/> (its feet).</summary>
        public static Boss Create(ArenaScene arena, BossDefinition definition, Vector2 position)
        {
            DirectionalAnimationBuilder.BuildAll();
            var walk = Load(DirectionalAnimationBuilder.KolossWalkPath);
            Colossus colossus = BossBuilder.CreateBody<Colossus>(
                "Koloss", arena, definition, position, walk, Load(DirectionalAnimationBuilder.KolossHitPath),
                Load(DirectionalAnimationBuilder.KolossDeathPath), 1f, ColliderRadius, ColliderOffset);
            var view = new SerializedObject(colossus.GetComponent<DirectionalSpriteAnimator>());
            view.FindProperty("_alert").objectReferenceValue = Load(DirectionalAnimationBuilder.KolossWindupPath);
            view.FindProperty("_move").objectReferenceValue = Load(DirectionalAnimationBuilder.KolossChargePath);
            view.ApplyModifiedPropertiesWithoutUndo();

            var lineGo = new GameObject("ChargeLine");
            lineGo.transform.SetParent(colossus.transform, false);
            var line = lineGo.AddComponent<SpriteRenderer>();
            line.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LineSpritePath);
            if (line.sprite == null)
            {
                throw new System.InvalidOperationException($"Sprite not found at {LineSpritePath}; run Tools/gen_pixelart.py");
            }

            line.sortingOrder = ArenaTestSceneBuilder.FloorMarkSortingOrder;
            line.enabled = false;

            var serialized = new SerializedObject(colossus);
            serialized.FindProperty("_line").objectReferenceValue = line;
            serialized.FindProperty("_wallLayers").intValue = 1 << ArenaTestSceneBuilder.RequireLayer(ArenaTestSceneBuilder.WallsLayer);
            serialized.FindProperty("_walk").objectReferenceValue = walk;
            serialized.FindProperty("_stun").objectReferenceValue = Load(DirectionalAnimationBuilder.KolossStunPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return colossus;
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
