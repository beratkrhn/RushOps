using System.IO;
using RushOps.Characters;
using RushOps.Editor.Art;
using RushOps.Editor.Scenes;
using RushOps.Skins;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Skins
{
    /// <summary>
    /// Creates or updates the skin assets and the <see cref="SkinCatalog"/> (issue #128): the standard skin, one skin
    /// per level (F-SKN-02) and three milestone skins (F-SKN-03). Names and thresholds of existing assets are kept, so
    /// tuned values survive; the animations are taken from <c>Data/Animations/Skins/&lt;Id&gt;_&lt;Anim&gt;.asset</c>
    /// (issue #129, built from the sprites of <c>Tools/gen_skins.py</c>) and fall back to the standard player
    /// animations.
    /// Batchmode: <c>-executeMethod RushOps.Editor.Skins.SkinCatalogBuilder.Build</c>
    /// </summary>
    public static class SkinCatalogBuilder
    {
        /// <summary>Asset path of the skin catalog.</summary>
        public const string CatalogPath = SkinsDir + "/SkinCatalog.asset";

        /// <summary>Id of the standard skin.</summary>
        public const string DefaultSkinId = "Standard";

        private const string SkinsDir = "Assets/_Project/Data/Skins";
        private const string SkinAnimationsDir = "Assets/_Project/Data/Animations/Skins";
        private const string SkinSpritesDir = "Assets/_Project/Art/Sprites/Player/Skins";

        private readonly struct Spec
        {
            public Spec(string id, string name, SkinUnlockKind kind, string level = "", SkinMilestone milestone = default, int threshold = 0)
            {
                Id = id;
                Name = name;
                Kind = kind;
                Level = level;
                Milestone = milestone;
                Threshold = threshold;
            }

            public string Id { get; }
            public string Name { get; }
            public SkinUnlockKind Kind { get; }
            public string Level { get; }
            public SkinMilestone Milestone { get; }
            public int Threshold { get; }
        }

        // Display order of the skin selection: standard, the level skins by island, then the milestones.
        private static readonly Spec[] Specs =
        {
            new Spec(DefaultSkinId, "Standard", SkinUnlockKind.Default),
            new Spec("Rettungsschwimmer", "Rettungsschwimmer", SkinUnlockKind.LevelCompleted, "1-1"),
            new Spec("Surfer", "Surfer", SkinUnlockKind.LevelCompleted, "1-2"),
            new Spec("Sandsturm", "Sandsturm", SkinUnlockKind.LevelCompleted, "1-3"),
            new Spec("Nachtwache", "Nachtwache", SkinUnlockKind.LevelCompleted, "2-1"),
            new Spec("Neon", "Neon", SkinUnlockKind.LevelCompleted, "2-2"),
            new Spec("Rost", "Rost", SkinUnlockKind.LevelCompleted, "2-3"),
            new Spec("Glut", "Glut", SkinUnlockKind.LevelCompleted, "3-1"),
            new Spec("Asche", "Asche", SkinUnlockKind.LevelCompleted, "3-2"),
            new Spec("Obsidian", "Obsidian", SkinUnlockKind.LevelCompleted, "3-3"),
            new Spec("Veteran", "Veteran", SkinUnlockKind.Milestone, "", SkinMilestone.TotalKills, 100),
            new Spec("Zombiejaeger", "Zombiejäger", SkinUnlockKind.Milestone, "", SkinMilestone.TotalKills, 1000),
            new Spec("Goldstern", "Goldstern", SkinUnlockKind.Milestone, "", SkinMilestone.TotalStars, 27),
        };

        /// <summary>Ids of all skins in display order.</summary>
        public static string[] SkinIds
        {
            get
            {
                var ids = new string[Specs.Length];
                for (int i = 0; i < Specs.Length; i++)
                {
                    ids[i] = Specs[i].Id;
                }

                return ids;
            }
        }

        /// <summary>Asset path of a skin's animation, e.g. <c>Surfer_Walk</c> (issue #129).</summary>
        public static string SkinAnimationPath(string skinId, string animation)
        {
            return $"{SkinAnimationsDir}/{skinId}_{animation}.asset";
        }

        [MenuItem("RushOps/Skins/Skin-Katalog aktualisieren")]
        public static SkinCatalog Build()
        {
            ArenaTestSceneBuilder.EnsureFolder(SkinsDir);
            BuildAnimations();
            var skins = new SkinDefinition[Specs.Length];
            for (int i = 0; i < Specs.Length; i++)
            {
                skins[i] = CreateOrUpdate(Specs[i]);
            }

            SkinCatalog catalog = ArenaTestSceneBuilder.LoadOrCreateConfig<SkinCatalog>(CatalogPath);
            var serialized = new SerializedObject(catalog);
            SerializedProperty list = serialized.FindProperty("_skins");
            list.arraySize = skins.Length;
            for (int i = 0; i < skins.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = skins[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        // Animations of the recoloured sprites from Tools/gen_skins.py (issue #129), with the standard player's frame
        // rates and looping, so every skin moves the same.
        private static void BuildAnimations()
        {
            if (!AssetDatabase.IsValidFolder(SkinSpritesDir))
            {
                return;
            }

            ArenaTestSceneBuilder.EnsureFolder(SkinAnimationsDir);
            foreach (Spec spec in Specs)
            {
                if (spec.Kind == SkinUnlockKind.Default || !File.Exists($"{SkinSpritesDir}/{spec.Id}_Idle_S.png"))
                {
                    continue;
                }

                BuildAnimation(spec.Id, "Idle", DirectionalAnimationBuilder.PlayerIdlePath);
                BuildAnimation(spec.Id, "Walk", DirectionalAnimationBuilder.PlayerWalkPath);
                BuildAnimation(spec.Id, "Hit", DirectionalAnimationBuilder.PlayerHitPath);
                BuildAnimation(spec.Id, "Death", DirectionalAnimationBuilder.PlayerDeathPath);
                BuildAnimation(spec.Id, "Dash", DirectionalAnimationBuilder.PlayerDashPath);
            }
        }

        private static void BuildAnimation(string skinId, string animation, string standardPath)
        {
            var standard = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(standardPath);
            DirectionalAnimation built = DirectionalAnimationBuilder.CreateOrUpdate(
                SkinAnimationPath(skinId, animation), SkinSpritesDir, $"{skinId}_{animation}",
                standard == null || standard.Loops, standard != null ? standard.FramesPerSecond : -1f);
            if (standard != null && !Mathf.Approximately(built.FramesPerSecond, standard.FramesPerSecond))
            {
                // Tempo changes of the standard player carry over to every skin.
                var serialized = new SerializedObject(built);
                serialized.FindProperty("_framesPerSecond").floatValue = standard.FramesPerSecond;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static SkinDefinition CreateOrUpdate(Spec spec)
        {
            string path = $"{SkinsDir}/Skin_{spec.Id}.asset";
            var skin = AssetDatabase.LoadAssetAtPath<SkinDefinition>(path);
            bool created = skin == null;
            if (created)
            {
                skin = ScriptableObject.CreateInstance<SkinDefinition>();
                AssetDatabase.CreateAsset(skin, path);
            }

            var serialized = new SerializedObject(skin);
            serialized.FindProperty("_id").stringValue = spec.Id;
            serialized.FindProperty("_unlockKind").enumValueIndex = (int)spec.Kind;
            serialized.FindProperty("_levelId").stringValue = spec.Kind == SkinUnlockKind.LevelCompleted ? "Level_" + spec.Level : "";
            serialized.FindProperty("_levelName").stringValue = spec.Kind == SkinUnlockKind.LevelCompleted ? "Level " + spec.Level : "";
            serialized.FindProperty("_milestone").enumValueIndex = (int)spec.Milestone;
            if (created)
            {
                serialized.FindProperty("_displayName").stringValue = spec.Name;
                serialized.FindProperty("_threshold").intValue = spec.Threshold;
            }

            SetAnimation(serialized, "_idle", spec.Id, "Idle", DirectionalAnimationBuilder.PlayerIdlePath);
            SetAnimation(serialized, "_walk", spec.Id, "Walk", DirectionalAnimationBuilder.PlayerWalkPath);
            SetAnimation(serialized, "_hit", spec.Id, "Hit", DirectionalAnimationBuilder.PlayerHitPath);
            SetAnimation(serialized, "_death", spec.Id, "Death", DirectionalAnimationBuilder.PlayerDeathPath);
            SetAnimation(serialized, "_dash", spec.Id, "Dash", DirectionalAnimationBuilder.PlayerDashPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skin);
            return skin;
        }

        // The skin's own animation (issue #129) if it exists, else the standard player's.
        private static void SetAnimation(SerializedObject serialized, string field, string skinId, string animation, string fallbackPath)
        {
            string own = SkinAnimationPath(skinId, animation);
            var asset = File.Exists(own) ? AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(own) : null;
            if (asset == null)
            {
                asset = AssetDatabase.LoadAssetAtPath<DirectionalAnimation>(fallbackPath);
            }

            serialized.FindProperty(field).objectReferenceValue = asset;
        }
    }
}
