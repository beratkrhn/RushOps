using System.IO;
using RushOps.Characters;
using UnityEditor;
using UnityEngine;

namespace RushOps.Editor.Scenes
{
    /// <summary>
    /// Gives enemies and bosses a hurtbox over their whole visible body: a trigger box on its own layer that covers the
    /// opaque pixels of all walk frames in all 8 directions. Projectiles hit it; it touches nothing else, so the feet
    /// collider alone keeps handling movement, pathing and contact (ADR 0006).
    /// </summary>
    internal static class EnemyHurtboxBuilder
    {
        /// <summary>Layer of the hurtboxes; it collides with no layer, only projectile raycasts see it.</summary>
        public const string Layer = "EnemyHurtbox";

        private const string ObjectName = "Hurtbox";

        /// <summary>Layer index, created and cut off from all collisions on first use.</summary>
        public static int EnsureLayer()
        {
            int layer = ArenaTestSceneBuilder.EnsureLayer(Layer);
            for (int i = 0; i < 32; i++)
            {
                Physics2D.IgnoreLayerCollision(layer, i, true);
            }

            return layer;
        }

        /// <summary>Adds the hurtbox to <paramref name="go"/>, sized from the frames of <paramref name="walk"/>.</summary>
        public static BoxCollider2D Add(GameObject go, DirectionalAnimation walk)
        {
            Rect bounds = BodyBounds(walk);
            var hurtboxGo = new GameObject(ObjectName) { layer = EnsureLayer() };
            hurtboxGo.transform.SetParent(go.transform, false);
            var box = hurtboxGo.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = bounds.size;
            box.offset = bounds.center;
            return box;
        }

        /// <summary>
        /// Union of the opaque pixels of all frames of <paramref name="animation"/>, in units relative to the sprite pivot.
        /// </summary>
        public static Rect BodyBounds(DirectionalAnimation animation)
        {
            bool any = false;
            Rect union = default;
            for (int d = 0; d < Direction8Math.Count; d++)
            {
                var direction = (Direction8)d;
                for (int f = 0; f < animation.FrameCount(direction); f++)
                {
                    Sprite sprite = animation.GetFrame(direction, f);
                    if (sprite == null || !TryOpaquePixels(sprite, out RectInt pixels))
                    {
                        continue;
                    }

                    Rect local = ToLocalUnits(pixels, sprite.pivot, sprite.pixelsPerUnit);
                    union = any ? Union(union, local) : local;
                    any = true;
                }
            }

            if (!any)
            {
                throw new System.InvalidOperationException($"{animation.name} has no opaque frames for a hurtbox");
            }

            return union;
        }

        /// <summary>
        /// Converts <paramref name="pixels"/> (texture coordinates, y up, max exclusive) to units relative to
        /// <paramref name="pivot"/> (in pixels).
        /// </summary>
        public static Rect ToLocalUnits(RectInt pixels, Vector2 pivot, float pixelsPerUnit)
        {
            return new Rect(
                (pixels.xMin - pivot.x) / pixelsPerUnit, (pixels.yMin - pivot.y) / pixelsPerUnit,
                pixels.width / pixelsPerUnit, pixels.height / pixelsPerUnit);
        }

        private static Rect Union(Rect a, Rect b)
        {
            return Rect.MinMaxRect(
                Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));
        }

        // Reads the PNG itself, since imported sprites are not readable.
        private static bool TryOpaquePixels(Sprite sprite, out RectInt pixels)
        {
            pixels = default;
            string path = AssetDatabase.GetAssetPath(sprite.texture);
            var texture = new Texture2D(2, 2);
            try
            {
                if (string.IsNullOrEmpty(path) || !texture.LoadImage(File.ReadAllBytes(path)))
                {
                    return false;
                }

                Color32[] colors = texture.GetPixels32();
                int width = texture.width;
                int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
                for (int i = 0; i < colors.Length; i++)
                {
                    if (colors[i].a == 0)
                    {
                        continue;
                    }

                    int x = i % width;
                    int y = i / width;
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }

                if (maxX < 0)
                {
                    return false;
                }

                pixels = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
                return true;
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
