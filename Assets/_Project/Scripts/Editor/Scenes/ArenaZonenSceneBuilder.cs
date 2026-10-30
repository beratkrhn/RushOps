using RushOps.Levels;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Builds the <c>Arena_Zonen</c> test level (issue #17): the arena of <see cref="ArenaTestSceneBuilder"/> in a
    /// tall layout with three zones stacked from south to north, separated by two gates. Both gates open by a
    /// <see cref="DebugGateCondition"/> a few seconds after the zone in front of them became active.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Scenes.ArenaZonenSceneBuilder.Build</c>
    /// </summary>
    public static class ArenaZonenSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Arena_Zonen.unity";
        private const string GateConfigPath = "Assets/_Project/Data/Levels/GateConfig.asset";
        private const string GateSpritePath = "Assets/_Project/Art/Sprites/Level/Gate_Bars.png";
        private const int Width = 24;
        private const int Height = 84;
        // Rows of the walls between the zones; each has a gate in the middle.
        private static readonly int[] DividerRows = { 27, 55 };
        private const int GateX = 10;
        private const int GateWidth = 4;
        // Debug gates open this many seconds after the zone in front of them became active.
        private const float DebugGateDelay = 5f;

        private static readonly ArenaLayout Layout = new ArenaLayout(
            Width, Height, new Vector2Int(12, 4),
            // Spawn points only in the first zone; waves per zone follow in issue #18.
            new[]
            {
                new Vector2Int(2, 25), new Vector2Int(21, 25), new Vector2Int(12, 24),
                new Vector2Int(2, 19), new Vector2Int(21, 19),
            },
            new[]
            {
                // Zone 1
                new RectInt(5, 8, 3, 2),
                new RectInt(16, 12, 3, 3),
                new RectInt(9, 19, 6, 1),
                // Divider with gate 1
                new RectInt(1, DividerRows[0], GateX - 1, 1),
                new RectInt(GateX + GateWidth, DividerRows[0], Width - 1 - GateX - GateWidth, 1),
                // Zone 2
                new RectInt(4, 34, 2, 6),
                new RectInt(18, 33, 2, 6),
                new RectInt(10, 44, 4, 2),
                // Divider with gate 2
                new RectInt(1, DividerRows[1], GateX - 1, 1),
                new RectInt(GateX + GateWidth, DividerRows[1], Width - 1 - GateX - GateWidth, 1),
                // Zone 3
                new RectInt(6, 62, 3, 3),
                new RectInt(15, 62, 3, 3),
                new RectInt(8, 72, 8, 2),
            });

        [MenuItem("RushOps/Szenen/Arena_Zonen erzeugen")]
        public static void Build()
        {
            ArenaTestSceneBuilder.BuildScene(ScenePath, Layout, false, AddZones);
        }

        private static void AddZones(ArenaScene arena)
        {
            var config = ArenaTestSceneBuilder.LoadOrCreateConfig<GateConfig>(GateConfigPath);
            var bars = AssetDatabase.LoadAssetAtPath<Sprite>(GateSpritePath);
            if (bars == null)
            {
                throw new System.InvalidOperationException($"Gate sprite not found at {GateSpritePath}; run Tools/gen_pixelart.py");
            }

            AudioClip openClip = ArenaTestSceneBuilder.LoadClip("GateOpen.wav");
            var gatesRoot = new GameObject("Gates").transform;
            var gates = new Gate[DividerRows.Length];
            for (int i = 0; i < gates.Length; i++)
            {
                gates[i] = CreateGate($"Gate_{i + 1}", gatesRoot, DividerRows[i], config, bars, openClip, arena);
            }

            // Zones span the inner columns, from the row above the previous divider up to their own divider.
            var zonesRoot = new GameObject("Zones").transform;
            int bottom = 1;
            for (int i = 0; i <= gates.Length; i++)
            {
                int top = i < gates.Length ? DividerRows[i] + 1 : Height - 1;
                Gate exit = i < gates.Length ? gates[i] : null;
                Zone zone = CreateZone($"Zone_{i + 1}", zonesRoot, bottom, top, exit, arena.Player);
                if (exit != null)
                {
                    var condition = new SerializedObject(exit.GetComponent<DebugGateCondition>());
                    condition.FindProperty("_zone").objectReferenceValue = zone;
                    condition.FindProperty("_delay").floatValue = DebugGateDelay;
                    condition.ApplyModifiedPropertiesWithoutUndo();
                }

                bottom = top;
            }
        }

        // Zone from row bottom (inclusive) to row top (exclusive) over the inner columns.
        private static Zone CreateZone(string name, Transform parent, int bottom, int top, Gate exit, Transform player)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(Width * 0.5f, (bottom + top) * 0.5f, 0f);
            var zone = go.AddComponent<Zone>();
            var serialized = new SerializedObject(zone);
            serialized.FindProperty("_size").vector2Value = new Vector2(Width - 2, top - bottom);
            serialized.FindProperty("_player").objectReferenceValue = player;
            serialized.FindProperty("_exitGate").objectReferenceValue = exit;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return zone;
        }

        private static Gate CreateGate(
            string name, Transform parent, int row, GateConfig config, Sprite barSprite, AudioClip openClip, ArenaScene arena)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(GateX + GateWidth * 0.5f, row + 0.5f, 0f);
            // On the walls layer, so the player, enemies and shots treat a closed gate like a wall.
            go.layer = ArenaTestSceneBuilder.RequireLayer(ArenaTestSceneBuilder.WallsLayer);
            var blocker = go.AddComponent<BoxCollider2D>();
            blocker.size = new Vector2(GateWidth, 1f);

            var bars = new SpriteRenderer[GateWidth];
            for (int i = 0; i < GateWidth; i++)
            {
                var bar = new GameObject($"Bars_{i}");
                bar.transform.SetParent(go.transform, false);
                bar.transform.localPosition = new Vector3(i - (GateWidth - 1) * 0.5f, 0f, 0f);
                bars[i] = bar.AddComponent<SpriteRenderer>();
                bars[i].sprite = barSprite;
                bars[i].sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder;
            }

            go.AddComponent<DebugGateCondition>();
            var gate = go.AddComponent<Gate>();
            var serialized = new SerializedObject(gate);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_condition").objectReferenceValue = go.GetComponent<DebugGateCondition>();
            serialized.FindProperty("_size").vector2IntValue = new Vector2Int(GateWidth, 1);
            serialized.FindProperty("_blocker").objectReferenceValue = blocker;
            serialized.FindProperty("_navigator").objectReferenceValue = arena.Navigator;
            serialized.FindProperty("_sfx").objectReferenceValue = arena.Sfx;
            serialized.FindProperty("_openClip").objectReferenceValue = openClip;
            SerializedProperty barsProperty = serialized.FindProperty("_bars");
            barsProperty.arraySize = bars.Length;
            for (int i = 0; i < bars.Length; i++)
            {
                barsProperty.GetArrayElementAtIndex(i).objectReferenceValue = bars[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return gate;
        }
    }
}
