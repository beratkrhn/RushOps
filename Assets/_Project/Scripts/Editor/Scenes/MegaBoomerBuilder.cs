using RushOps.Bosses;
using RushOps.Characters;
using RushOps.Editor.Art;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Creates the Mega-Boomer boss (issue #159) with its animations, its two inactive halves and the pool of its
    /// puddles.
    /// </summary>
    internal static class MegaBoomerBuilder
    {
        /// <summary>Definition of the Mega-Boomer.</summary>
        public const string DefinitionPath = "Assets/_Project/Data/Bosses/MegaBoomer.asset";

        private const string PuddleSpritesDir = "Assets/_Project/Art/Sprites/Effects";
        private const int PuddleFrames = 2;

        // Body at the feet of a 128 px figure, like the Koloss; the halves scale it down with their transform.
        private const float ColliderRadius = 0.85f;
        private static readonly Vector2 ColliderOffset = new Vector2(0f, 0.85f);

        // Trigger over the flat 48x24 puddle, a bit smaller so only real contact hurts.
        private static readonly Vector2 PuddleSize = new Vector2(1.3f, 0.6f);

        /// <summary>
        /// Starting values (decisions C5, C10): about 90 s for both halves together without perks at 6 shots per
        /// second, with time lost dodging puddles; slow; 1000 points. Puddle and split values keep the defaults of
        /// <see cref="MegaBoomerDefinition"/> (C9a, C9b).
        /// </summary>
        public static void Configure(BossDefinition definition)
        {
            definition.Configure("Mega-Boomer", 3600, 1.1f, 1, 1000);
        }

        /// <summary>Creates the Mega-Boomer at <paramref name="position"/> (its feet), with its halves and puddles.</summary>
        public static Boss Create(ArenaScene arena, BossDefinition definition, Vector2 position)
        {
            DirectionalAnimationBuilder.BuildAll();
            PuddlePool puddles = CreatePuddlePool();
            int walls = 1 << ArenaTestSceneBuilder.RequireLayer(ArenaTestSceneBuilder.WallsLayer);
            MegaBoomer boss = CreateBody("MegaBoomer", arena, definition, position, 1f, puddles, walls);
            float halfScale = definition is MegaBoomerDefinition values ? values.HalfScale : 0.65f;
            var halves = new MegaBoomer[2];
            for (int i = 0; i < halves.Length; i++)
            {
                halves[i] = CreateBody($"MegaBoomer_Half_{i + 1}", arena, definition, position, halfScale, puddles, walls);
                halves[i].gameObject.SetActive(false);
            }

            var serialized = new SerializedObject(boss);
            SerializedProperty list = serialized.FindProperty("_halves");
            list.arraySize = halves.Length;
            for (int i = 0; i < halves.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = halves[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return boss;
        }

        /// <summary>Lets the boss hand its halves to <paramref name="fight"/>.</summary>
        public static void Connect(Boss boss, BossFight fight)
        {
            var serialized = new SerializedObject(boss);
            serialized.FindProperty("_fight").objectReferenceValue = fight;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static MegaBoomer CreateBody(string name, ArenaScene arena, BossDefinition definition, Vector2 position,
            float scale, PuddlePool puddles, int walls)
        {
            MegaBoomer body = BossBuilder.CreateBody<MegaBoomer>(
                name, arena, definition, position, Load(DirectionalAnimationBuilder.MegaBoomerWalkPath),
                Load(DirectionalAnimationBuilder.MegaBoomerHitPath), Load(DirectionalAnimationBuilder.MegaBoomerDeathPath),
                scale, ColliderRadius, ColliderOffset);
            var view = new SerializedObject(body.GetComponent<DirectionalSpriteAnimator>());
            view.FindProperty("_alert").objectReferenceValue = Load(DirectionalAnimationBuilder.MegaBoomerSplitPath);
            view.ApplyModifiedPropertiesWithoutUndo();

            var serialized = new SerializedObject(body);
            serialized.FindProperty("_puddles").objectReferenceValue = puddles;
            serialized.FindProperty("_wallLayers").intValue = walls;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return body;
        }

        private static PuddlePool CreatePuddlePool()
        {
            var pool = new GameObject("PuddlePool").AddComponent<PuddlePool>();
            var template = new GameObject("Puddle");
            template.transform.SetParent(pool.transform, false);
            var renderer = template.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = ArenaTestSceneBuilder.FloorMarkSortingOrder;
            var trigger = template.AddComponent<CapsuleCollider2D>();
            trigger.direction = CapsuleDirection2D.Horizontal;
            trigger.size = PuddleSize;
            trigger.isTrigger = true;
            var puddle = template.AddComponent<Puddle>();
            Sprite[] warm = LoadFrames("Puddle_Warm");
            renderer.sprite = warm[0];

            var serializedPuddle = new SerializedObject(puddle);
            serializedPuddle.FindProperty("_renderer").objectReferenceValue = renderer;
            SetFrames(serializedPuddle.FindProperty("_warmFrames"), warm);
            SetFrames(serializedPuddle.FindProperty("_activeFrames"), LoadFrames("Puddle_Active"));
            serializedPuddle.ApplyModifiedPropertiesWithoutUndo();
            template.SetActive(false);

            var serializedPool = new SerializedObject(pool);
            serializedPool.FindProperty("_template").objectReferenceValue = puddle;
            serializedPool.ApplyModifiedPropertiesWithoutUndo();
            return pool;
        }

        private static Sprite[] LoadFrames(string prefix)
        {
            var frames = new Sprite[PuddleFrames];
            for (int i = 0; i < frames.Length; i++)
            {
                string path = $"{PuddleSpritesDir}/{prefix}_{i}.png";
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (frames[i] == null)
                {
                    throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
                }
            }

            return frames;
        }

        private static void SetFrames(SerializedProperty list, Sprite[] frames)
        {
            list.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            }
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
