using RushOps.Audio;
using RushOps.Characters;
using RushOps.Combat;
using RushOps.Feel;
using RushOps.Perks;
using RushOps.Player;
using RushOps.UI;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Adds the rocket launcher to the player of an arena (issue #255), next to the flamethrower: its config on the
    /// <see cref="PlayerSuperWeapon"/>, the <see cref="RocketLauncher"/> with the weapon in the hand, rockets from the
    /// player's projectile pool, a pool for the blasts with an orange accent light, and the launch and blast sounds
    /// through the effects group. Rockets stop at walls and enemies; the blast reaches only enemy hurtboxes.
    /// Run after <see cref="FlamethrowerBuilder"/>, which adds the <see cref="PlayerSuperWeapon"/>.
    /// </summary>
    internal static class RocketLauncherBuilder
    {
        private const string ConfigPath = "Assets/_Project/Data/Weapons/RocketLauncherConfig.asset";
        private const string HandDir = "Assets/_Project/Art/Sprites/RocketLauncher";
        private const string MuzzlesPath = HandDir + "/RocketLauncher_Muzzles.json";
        private const string RocketSpritePath = "Assets/_Project/Art/Sprites/Projectiles/Rocket.png";
        private const string BlastFramesPrefix = "Assets/_Project/Art/Sprites/Effects/RocketBlast_";
        private const string BlastPrefabPath = "Assets/_Project/Prefabs/Effects/RocketBlast.prefab";
        private const int BlastFrameCount = 6;
        private const float BlastLightRadius = 2.5f;

        public static RocketLauncher Create(Transform player, LevelResults results, SfxPlayer sfx)
        {
            RocketLauncherConfig config = ArenaTestSceneBuilder.LoadOrCreateConfig<RocketLauncherConfig>(ConfigPath);
            var superWeapon = player.GetComponent<PlayerSuperWeapon>();
            if (superWeapon == null)
            {
                throw new System.InvalidOperationException("Player has no PlayerSuperWeapon; run FlamethrowerBuilder first");
            }

            var serializedSuper = new SerializedObject(superWeapon);
            serializedSuper.FindProperty("_rocketLauncher").objectReferenceValue = config;
            serializedSuper.ApplyModifiedPropertiesWithoutUndo();

            var go = new GameObject("RocketLauncher");
            go.transform.SetParent(player, false);

            // Laid over the player sprite, which has the same canvas and pivot.
            var handGo = new GameObject("Hand");
            handGo.transform.SetParent(go.transform, false);
            var hand = handGo.AddComponent<SpriteRenderer>();
            hand.sortingOrder = ArenaTestSceneBuilder.ActorSortingOrder + 1;
            hand.spriteSortPoint = SpriteSortPoint.Pivot;
            hand.enabled = false;
            var handSprites = new Sprite[Direction8Math.Count];
            for (int d = 0; d < Direction8Math.Count; d++)
            {
                handSprites[d] = Load($"{HandDir}/Hand_{(Direction8)d}.png");
            }

            hand.sprite = handSprites[(int)Direction8.S];

            SpriteEffectPool blasts = ArenaTestSceneBuilder.CreateEffectPool(
                "RocketBlastEffectPool",
                ArenaTestSceneBuilder.CreateOrUpdateEffectPrefab("RocketBlast", BlastPrefabPath,
                    ArenaTestSceneBuilder.ExplosionLightColor, BlastLightRadius, ArenaTestSceneBuilder.ExplosionLightIntensity),
                4, 16);

            var weapon = player.GetComponent<PlayerWeapon>();
            var serializedWeapon = new SerializedObject(weapon);
            var projectiles = (ProjectilePool)serializedWeapon.FindProperty("_pool").objectReferenceValue;
            int obstacles = serializedWeapon.FindProperty("_obstacles").intValue;

            var launcher = go.AddComponent<RocketLauncher>();
            var serialized = new SerializedObject(launcher);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_superWeapon").objectReferenceValue = superWeapon;
            serialized.FindProperty("_aim").objectReferenceValue = player.GetComponent<PlayerAim>();
            serialized.FindProperty("_assist").objectReferenceValue = player.GetComponent<AimAssist>();
            serialized.FindProperty("_weapon").objectReferenceValue = weapon;
            serialized.FindProperty("_view").objectReferenceValue = player.GetComponent<DirectionalSpriteAnimator>();
            serialized.FindProperty("_dash").objectReferenceValue = player.GetComponent<PlayerDash>();
            serialized.FindProperty("_health").objectReferenceValue = player.GetComponent<PlayerHealth>();
            serialized.FindProperty("_results").objectReferenceValue = results;
            serialized.FindProperty("_rockets").objectReferenceValue = projectiles;
            // Same as the normal weapon: walls, enemy bodies and hurtboxes.
            serialized.FindProperty("_obstacles").intValue = obstacles;
            serialized.FindProperty("_targets").intValue = 1 << EnemyHurtboxBuilder.EnsureLayer();

            Vector2[] muzzles = ArenaTestSceneBuilder.LoadMuzzles(MuzzlesPath);
            SerializedProperty muzzleList = serialized.FindProperty("_muzzles");
            muzzleList.arraySize = muzzles.Length;
            for (int i = 0; i < muzzles.Length; i++)
            {
                muzzleList.GetArrayElementAtIndex(i).vector2Value = muzzles[i];
            }

            serialized.FindProperty("_hand").objectReferenceValue = hand;
            SetSprites(serialized.FindProperty("_handSprites"), handSprites);
            serialized.FindProperty("_rocketSprite").objectReferenceValue = Load(RocketSpritePath);
            serialized.FindProperty("_explosionEffects").objectReferenceValue = blasts;
            var frames = new Sprite[BlastFrameCount];
            for (int i = 0; i < BlastFrameCount; i++)
            {
                frames[i] = Load($"{BlastFramesPrefix}{i}.png");
            }

            SetSprites(serialized.FindProperty("_explosionFrames"), frames);
            serialized.FindProperty("_sfx").objectReferenceValue = sfx;
            serialized.FindProperty("_launchClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("RocketLaunch.wav");
            serialized.FindProperty("_explosionClip").objectReferenceValue = ArenaTestSceneBuilder.LoadClip("RocketBlast.wav");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return launcher;
        }

        private static Sprite Load(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path)
                ?? throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
        }

        private static void SetSprites(SerializedProperty list, Sprite[] sprites)
        {
            list.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }
    }
}
