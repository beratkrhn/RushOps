using RushOps.Combat;
using RushOps.Editor.Scenes;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Combat
{
    /// <summary>
    /// Creates the weapon assets under Data/Weapons (issue #174, decision W2): the pistol and the three new weapons,
    /// each with its icon, bullet and shot sound. Balancing values are only set on new assets; tuned values stay.
    /// All weapons deal about the damage per second of the pistol (W7, ADR 0012: 6 shots of 10 damage).
    /// Batchmode: <c>-executeMethod RushOps.Editor.Combat.WeaponCatalogBuilder.Build</c>
    /// </summary>
    public static class WeaponCatalogBuilder
    {
        public const string PistolPath = WeaponsDir + "/Pistol.asset";
        public const string AssaultRiflePath = WeaponsDir + "/AssaultRifle.asset";
        public const string ShotgunPath = WeaponsDir + "/Shotgun.asset";
        public const string DualPistolsPath = WeaponsDir + "/DualPistols.asset";

        private const string WeaponsDir = "Assets/_Project/Data/Weapons";
        private const string UiDir = "Assets/_Project/Art/Sprites/UI";
        private const string ProjectilesDir = "Assets/_Project/Art/Sprites/Projectiles";

        private readonly struct Spec
        {
            public Spec(string path, string name, float fireRate, int damage, int projectiles, float fan, float spread,
                float range, float speed, bool alternate, string icon, string bullet, string sound)
            {
                Path = path;
                Name = name;
                FireRate = fireRate;
                Damage = damage;
                Projectiles = projectiles;
                Fan = fan;
                Spread = spread;
                Range = range;
                Speed = speed;
                Alternate = alternate;
                Icon = icon;
                Bullet = bullet;
                Sound = sound;
            }

            public string Path { get; }
            public string Name { get; }
            public float FireRate { get; }
            public int Damage { get; }
            public int Projectiles { get; }
            public float Fan { get; }
            public float Spread { get; }
            public float Range { get; }
            public float Speed { get; }
            public bool Alternate { get; }
            public string Icon { get; }
            public string Bullet { get; }
            public string Sound { get; }
        }

        // Start values (W2, W7): about 60 damage per second each.
        private static readonly Spec[] Specs =
        {
            new Spec(PistolPath, "Pistole", 6f, 10, 1, 0f, 0f, 10f, 14f, false, "Weapon_Pistol.png", null, "Shot.wav"),
            // Faster than the pistol, a little spread, a bit further.
            new Spec(AssaultRiflePath, "Sturmgewehr", 8.5f, 7, 1, 0f, 4f, 11f, 16f, false,
                "Weapon_AssaultRifle.png", "Bullet_Rifle.png", "ShotRifle.wav"),
            // Slow, five pellets in a fan, short range.
            new Spec(ShotgunPath, "Schrotflinte", 1.2f, 10, 5, 30f, 2f, 5f, 13f, false,
                "Weapon_Shotgun.png", "Bullet_Pellet.png", "ShotShotgun.wav"),
            // Very fast, little damage, left and right in turn.
            new Spec(DualPistolsPath, "Dual-Pistolen", 10f, 6, 1, 0f, 1f, 9f, 15f, true,
                "Weapon_DualPistols.png", "Bullet_Dual.png", "ShotDual.wav"),
        };

        [MenuItem("RushOps/Daten/Waffen erzeugen")]
        public static void Build()
        {
            All();
        }

        /// <summary>Creates or updates all weapons; returns them in the order pistol, rifle, shotgun, dual pistols.</summary>
        public static WeaponDefinition[] All()
        {
            var weapons = new WeaponDefinition[Specs.Length];
            for (int i = 0; i < Specs.Length; i++)
            {
                weapons[i] = CreateOrUpdate(Specs[i]);
            }

            AssetDatabase.SaveAssets();
            return weapons;
        }

        private static WeaponDefinition CreateOrUpdate(Spec spec)
        {
            bool exists = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(spec.Path) != null;
            WeaponDefinition weapon = ArenaTestSceneBuilder.LoadOrCreateConfig<WeaponDefinition>(spec.Path);
            var serialized = new SerializedObject(weapon);
            if (!exists)
            {
                serialized.FindProperty("_displayName").stringValue = spec.Name;
                serialized.FindProperty("_fireRate").floatValue = spec.FireRate;
                serialized.FindProperty("_damage").intValue = spec.Damage;
                serialized.FindProperty("_projectilesPerShot").intValue = spec.Projectiles;
                serialized.FindProperty("_fanAngle").floatValue = spec.Fan;
                serialized.FindProperty("_spread").floatValue = spec.Spread;
                serialized.FindProperty("_range").floatValue = spec.Range;
                serialized.FindProperty("_projectileSpeed").floatValue = spec.Speed;
                serialized.FindProperty("_alternateMuzzles").boolValue = spec.Alternate;
            }

            serialized.FindProperty("_icon").objectReferenceValue = LoadSprite($"{UiDir}/{spec.Icon}");
            serialized.FindProperty("_projectileSprite").objectReferenceValue =
                spec.Bullet != null ? LoadSprite($"{ProjectilesDir}/{spec.Bullet}") : null;
            serialized.FindProperty("_shotSound").objectReferenceValue = ArenaTestSceneBuilder.LoadClip(spec.Sound);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(weapon);
            return weapon;
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException($"Sprite not found at {path}; run Tools/gen_pixelart.py");
            }

            return sprite;
        }
    }
}
