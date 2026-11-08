using RushOps.Audio;
using RushOps.Characters;
using RushOps.Feel;
using RushOps.Perks;
using RushOps.Player;
using RushOps.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Adds the super weapons to the player of an arena (issue #254): the <see cref="PlayerSuperWeapon"/> that holsters
    /// the normal weapon, and the <see cref="Flamethrower"/> with the weapon in the hand, the flame on a pivot at the
    /// nozzle with a warm light, the looping flame sound through the effects group and a pool for the small fires on
    /// burning enemies. The flame reaches the hurtboxes of enemies and bosses and stops at walls.
    /// </summary>
    internal static class FlamethrowerBuilder
    {
        private const string ConfigPath = "Assets/_Project/Data/Weapons/FlamethrowerConfig.asset";
        private const string HandDir = "Assets/_Project/Art/Sprites/Flamethrower";
        private const string MuzzlesPath = HandDir + "/Flamethrower_Muzzles.json";
        private const string EffectsDir = "Assets/_Project/Art/Sprites/Effects";
        private const string BurnPrefabPath = "Assets/_Project/Prefabs/Effects/Burn.prefab";
        private const int FlameFrameCount = 4;
        private const int BurnFrameCount = 4;
        private static readonly Color FlameLightColor = new Color(1f, 0.6f, 0.25f);
        private const float FlameLightRadius = 3f;
        private const float FlameLightIntensity = 0.9f;

        public static Flamethrower Create(Transform player, LevelResults results, PauseController pause, SfxPlayer sfx)
        {
            FlamethrowerConfig config = ArenaTestSceneBuilder.LoadOrCreateConfig<FlamethrowerConfig>(ConfigPath);
            var health = player.GetComponent<PlayerHealth>();

            var superWeapon = player.gameObject.AddComponent<PlayerSuperWeapon>();
            var serializedSuper = new SerializedObject(superWeapon);
            serializedSuper.FindProperty("_perks").objectReferenceValue = player.GetComponent<PlayerPerks>();
            serializedSuper.FindProperty("_health").objectReferenceValue = health;
            serializedSuper.FindProperty("_results").objectReferenceValue = results;
            serializedSuper.FindProperty("_flamethrower").objectReferenceValue = config;
            serializedSuper.ApplyModifiedPropertiesWithoutUndo();

            var go = new GameObject("Flamethrower");
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

            // The pivot sits on the nozzle and turns; the centered sprite is shifted so its left edge is the nozzle.
            Sprite[] flameFrames = LoadFrames($"{EffectsDir}/Flame_", FlameFrameCount);
            float flameLength = flameFrames[0].rect.width / flameFrames[0].pixelsPerUnit;
            var pivotGo = new GameObject("Flame");
            pivotGo.transform.SetParent(go.transform, false);
            var flameGo = new GameObject("Sprite");
            flameGo.transform.SetParent(pivotGo.transform, false);
            flameGo.transform.localPosition = new Vector3(flameLength / 2f, 0f, 0f);
            var flame = flameGo.AddComponent<SpriteRenderer>();
            flame.sprite = flameFrames[0];
            flame.sortingOrder = ArenaTestSceneBuilder.EffectSortingOrder;
            Light2D light = ArenaTestSceneBuilder.AddPointLight(pivotGo.transform, FlameLightColor, FlameLightRadius);
            light.transform.localPosition = new Vector3(flameLength / 2f, 0f, 0f);
            light.intensity = FlameLightIntensity;
            pivotGo.SetActive(false);

            var audio = go.AddComponent<AudioSource>();
            audio.clip = ArenaTestSceneBuilder.LoadClip("FlameLoop.wav");
            audio.loop = true;
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;
            audio.outputAudioMixerGroup = sfx != null ? sfx.Output : null;

            SpriteEffectPool burns = ArenaTestSceneBuilder.CreateEffectPool(
                "BurnEffectPool",
                ArenaTestSceneBuilder.CreateOrUpdateEffectPrefab("Burn", BurnPrefabPath, Color.white, 0f, 0f), 16, 64);

            var weapon = player.GetComponent<PlayerWeapon>();
            var flamethrower = go.AddComponent<Flamethrower>();
            var serialized = new SerializedObject(flamethrower);
            serialized.FindProperty("_config").objectReferenceValue = config;
            serialized.FindProperty("_superWeapon").objectReferenceValue = superWeapon;
            serialized.FindProperty("_aim").objectReferenceValue = player.GetComponent<PlayerAim>();
            serialized.FindProperty("_assist").objectReferenceValue = player.GetComponent<AimAssist>();
            serialized.FindProperty("_weapon").objectReferenceValue = weapon;
            serialized.FindProperty("_view").objectReferenceValue = player.GetComponent<DirectionalSpriteAnimator>();
            serialized.FindProperty("_dash").objectReferenceValue = player.GetComponent<PlayerDash>();
            serialized.FindProperty("_health").objectReferenceValue = health;
            serialized.FindProperty("_results").objectReferenceValue = results;
            serialized.FindProperty("_pause").objectReferenceValue = pause;
            serialized.FindProperty("_targets").intValue = 1 << EnemyHurtboxBuilder.EnsureLayer();
            serialized.FindProperty("_walls").intValue = 1 << ArenaTestSceneBuilder.RequireLayer(ArenaTestSceneBuilder.WallsLayer);

            Vector2[] nozzles = ArenaTestSceneBuilder.LoadMuzzles(MuzzlesPath);
            SerializedProperty nozzleList = serialized.FindProperty("_nozzles");
            nozzleList.arraySize = nozzles.Length;
            for (int i = 0; i < nozzles.Length; i++)
            {
                nozzleList.GetArrayElementAtIndex(i).vector2Value = nozzles[i];
            }

            serialized.FindProperty("_hand").objectReferenceValue = hand;
            SetSprites(serialized.FindProperty("_handSprites"), handSprites);
            serialized.FindProperty("_flamePivot").objectReferenceValue = pivotGo.transform;
            serialized.FindProperty("_flame").objectReferenceValue = flame;
            SetSprites(serialized.FindProperty("_flameFrames"), flameFrames);
            serialized.FindProperty("_flameLength").floatValue = flameLength;
            serialized.FindProperty("_burnEffects").objectReferenceValue = burns;
            SetSprites(serialized.FindProperty("_burnFrames"), LoadFrames($"{EffectsDir}/Burn_", BurnFrameCount));
            serialized.FindProperty("_audio").objectReferenceValue = audio;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return flamethrower;
        }

        private static Sprite[] LoadFrames(string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                frames[i] = Load($"{prefix}{i}.png");
            }

            return frames;
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
