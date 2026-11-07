using RushOps.Bosses;
using RushOps.Characters;
using RushOps.Editor.Art;
using RushOps.Feel;
using RushOps.Perks;
using RushOps.Scoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Places a boss fight in a level scene (issue #150): the boss, the door that closes behind the player, the health
    /// bar in the HUD and the <see cref="BossFight"/> that ties them together.
    /// </summary>
    internal static class BossBuilder
    {
        private const string GateSpritePath = "Assets/_Project/Art/Sprites/Level/Gate_Bars.png";

        // The placeholder boss is the brute at twice the size (until the bosses get their own sprites).
        private const float PlaceholderScale = 2f;
        private const float PlaceholderColliderRadius = 0.45f;
        private static readonly Vector2 PlaceholderColliderOffset = new Vector2(0f, 0.45f);

        // Health bar under score and multiplier, across the screen (F-HUD-05). The name label (30 units high, 2 above
        // the bar) must clear the multiplier (down to 58) and a two-line mission text at the top right (44 to 84), so
        // the bar starts at 116 (issue #201).
        private const float BarTop = 116f;
        private const float BarHeight = 6f;
        private const float BarSideMargin = 24f;
        private static readonly Color BarBack = new Color32(31, 29, 43, 255);
        private static readonly Color BarFill = new Color32(200, 50, 60, 255);

        /// <summary>Builds the fight of <paramref name="spec"/> in <paramref name="arena"/>.</summary>
        public static BossFight Create(LevelBlueprint blueprint, LevelBoss spec, ArenaScene arena, Levels.Zone[] zones)
        {
            BossDefinition definition = LoadOrCreateDefinition(spec);
            Boss boss = spec.Create != null
                ? spec.Create(arena, definition, spec.Position)
                : CreatePlaceholder(arena, definition, spec.Position);
            LevelGate gate = blueprint.Gates[spec.EntryGate];
            ArenaDoor door = CreateDoor(arena, gate);
            BossHealthBar bar = CreateHealthBar((RectTransform)arena.Tracker.transform.parent);

            var fight = new GameObject("BossFight").AddComponent<BossFight>();
            var serialized = new SerializedObject(fight);
            serialized.FindProperty("_arena").objectReferenceValue = zones[spec.ArenaZone];
            serialized.FindProperty("_door").objectReferenceValue = door;
            SerializedProperty bodies = serialized.FindProperty("_bodies");
            bodies.arraySize = 1;
            bodies.GetArrayElementAtIndex(0).objectReferenceValue = boss;
            serialized.FindProperty("_bar").objectReferenceValue = bar;
            serialized.FindProperty("_player").objectReferenceValue = arena.Player;
            serialized.FindProperty("_score").objectReferenceValue = Object.FindAnyObjectByType<LevelScore>();
            serialized.FindProperty("_results").objectReferenceValue = arena.Results;
            var dropper = Object.FindAnyObjectByType<PerkDropper>();
            serialized.FindProperty("_dropper").objectReferenceValue = dropper;
            serialized.FindProperty("_loot").objectReferenceValue =
                dropper != null ? new SerializedObject(dropper).FindProperty("_config").objectReferenceValue : null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            spec.Connect?.Invoke(boss, fight);
            return fight;
        }

        /// <summary>
        /// A boss body with the walk, hit and death animations given, scaled by <paramref name="scale"/>; component
        /// <typeparamref name="T"/> carries its behaviour.
        /// </summary>
        public static T CreateBody<T>(string name, ArenaScene arena, BossDefinition definition, Vector2 position,
            DirectionalAnimation walk, DirectionalAnimation hit, DirectionalAnimation death, float scale,
            float colliderRadius, Vector2 colliderOffset) where T : Boss
        {
            DirectionalSpriteAnimator view = ArenaTestSceneBuilder.CreateCharacter(name, null, Vector2Int.zero, walk, Direction8.S);
            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("_hit").objectReferenceValue = hit;
            serializedView.FindProperty("_death").objectReferenceValue = death;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
            GameObject go = view.gameObject;
            go.transform.position = position;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            go.GetComponent<SpriteRenderer>().sharedMaterial = ArenaTestSceneBuilder.LoadOrCreateFlashMaterial();
            go.layer = ArenaTestSceneBuilder.RequireLayer(ArenaTestSceneBuilder.EnemiesLayer);

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.mass = definition.Mass;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = colliderRadius;
            collider.offset = colliderOffset;
            BoxCollider2D hurtbox = EnemyHurtboxBuilder.Add(go, walk);

            var boss = go.AddComponent<T>();
            var serialized = new SerializedObject(boss);
            serialized.FindProperty("_definition").objectReferenceValue = definition;
            serialized.FindProperty("_feel").objectReferenceValue =
                ArenaTestSceneBuilder.LoadOrCreateConfig<FeelConfig>(ArenaTestSceneBuilder.FeelConfigPath);
            serialized.FindProperty("_renderer").objectReferenceValue = go.GetComponent<SpriteRenderer>();
            serialized.FindProperty("_view").objectReferenceValue = view;
            serialized.FindProperty("_hurtbox").objectReferenceValue = hurtbox;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return boss;
        }

        private static Boss CreatePlaceholder(ArenaScene arena, BossDefinition definition, Vector2 position)
        {
            return CreateBody<Boss>(
                "Boss", arena, definition, position,
                AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BruteWalkPath),
                AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BruteHitPath),
                AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(DirectionalAnimationBuilder.BruteDeathPath),
                PlaceholderScale, PlaceholderColliderRadius, PlaceholderColliderOffset);
        }

        private static BossDefinition LoadOrCreateDefinition(LevelBoss spec)
        {
            var definition = AssetDatabase.LoadAssetAtPath<BossDefinition>(spec.DefinitionPath);
            if (definition != null)
            {
                return definition;
            }

            ArenaTestSceneBuilder.EnsureFolder(System.IO.Path.GetDirectoryName(spec.DefinitionPath).Replace('\\', '/'));
            definition = (BossDefinition)ScriptableObject.CreateInstance(spec.DefinitionType);
            spec.Configure?.Invoke(definition);
            AssetDatabase.CreateAsset(definition, spec.DefinitionPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<BossDefinition>(spec.DefinitionPath);
        }

        // The door uses the gate's bars, so it reads as the same gate closing again.
        private static ArenaDoor CreateDoor(ArenaScene arena, LevelGate gate)
        {
            Sprite[] frames = arena.Theme != null ? arena.Theme.GateFrames : null;
            Sprite sprite = frames != null && frames.Length > 0 ? frames[0] : AssetDatabase.LoadAssetAtPath<Sprite>(GateSpritePath);
            var go = new GameObject("ArenaDoor");
            go.transform.position = gate.Center;
            go.layer = ArenaTestSceneBuilder.RequireLayer(ArenaTestSceneBuilder.WallsLayer);
            var blocker = go.AddComponent<BoxCollider2D>();
            blocker.size = gate.Vertical ? new Vector2(1f, gate.Length) : new Vector2(gate.Length, 1f);
            var bars = new SpriteRenderer[gate.Length];
            for (int i = 0; i < bars.Length; i++)
            {
                var bar = new GameObject($"Bars_{i}");
                bar.transform.SetParent(go.transform, false);
                float offset = i - (gate.Length - 1) * 0.5f;
                bar.transform.localPosition = gate.Vertical ? new Vector3(0f, offset, 0f) : new Vector3(offset, 0f, 0f);
                bars[i] = bar.AddComponent<SpriteRenderer>();
                bars[i].sprite = sprite;
                bars[i].sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            }

            var door = go.AddComponent<ArenaDoor>();
            var serialized = new SerializedObject(door);
            serialized.FindProperty("_size").vector2IntValue = gate.Vertical ? new Vector2Int(1, gate.Length) : new Vector2Int(gate.Length, 1);
            serialized.FindProperty("_blocker").objectReferenceValue = blocker;
            SerializedProperty list = serialized.FindProperty("_bars");
            list.arraySize = bars.Length;
            for (int i = 0; i < bars.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = bars[i];
            }

            serialized.FindProperty("_navigator").objectReferenceValue = arena.Navigator;
            serialized.FindProperty("_sfx").objectReferenceValue = arena.Sfx;
            serialized.FindProperty("_closeClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("GateOpen.wav");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return door;
        }

        private static BossHealthBar CreateHealthBar(RectTransform hud)
        {
            Font font = PixelFontBuilder.Build();
            RectTransform holder = ArenaTestSceneBuilder.CreateUiRect("BossHealthBar", hud, new Vector2(0f, 1f), new Vector2(1f, 1f));
            holder.pivot = new Vector2(0.5f, 1f);
            holder.sizeDelta = Vector2.zero;
            holder.anchoredPosition = Vector2.zero;

            RectTransform root = ArenaTestSceneBuilder.CreateUiRect("Panel", holder, new Vector2(0f, 1f), new Vector2(1f, 1f));
            root.pivot = new Vector2(0.5f, 1f);
            root.offsetMin = new Vector2(BarSideMargin, -BarTop - BarHeight);
            root.offsetMax = new Vector2(-BarSideMargin, -BarTop);
            var back = root.gameObject.AddComponent<Image>();
            back.color = BarBack;
            back.raycastTarget = false;
            RectTransform fill = ArenaTestSceneBuilder.CreateUiRect("Fill", root, Vector2.zero, Vector2.one);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = BarFill;
            fillImage.raycastTarget = false;

            Text label = ArenaTestSceneBuilder.CreateText("Name", root, font, "Boss", ArenaTestSceneBuilder.HudPixelScale, Vector2.zero);
            var top = new Vector2(0.5f, 1f);
            label.rectTransform.anchorMin = top;
            label.rectTransform.anchorMax = top;
            label.rectTransform.pivot = new Vector2(0.5f, 0f);
            label.rectTransform.anchoredPosition = new Vector2(0f, 2f);
            label.alignment = TextAnchor.LowerCenter;
            label.color = ArenaTestSceneBuilder.PaletteWhite;

            var bar = holder.gameObject.AddComponent<BossHealthBar>();
            var serialized = new SerializedObject(bar);
            serialized.FindProperty("_root").objectReferenceValue = root.gameObject;
            serialized.FindProperty("_fill").objectReferenceValue = fill;
            serialized.FindProperty("_name").objectReferenceValue = label;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            root.gameObject.SetActive(false);
            return bar;
        }
    }
}
