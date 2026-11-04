using UnityEngine;

namespace FishHunter.EditorTools
{
    /// <summary>Frame-by-frame effects: enemy death burst, projectile bubble, muzzle splash.</summary>
    public static class EffectPainter
    {
        private static readonly Color32 White = new Color32(255, 255, 255, 255);
        private static readonly Color32 BubbleRim = new Color32(190, 240, 255, 255);
        private static readonly Color32 BubbleFill = new Color32(160, 225, 255, 90);
        private static readonly Color32 Bone = new Color32(235, 235, 220, 255);
        private static readonly Color32 BoneShade = new Color32(150, 160, 170, 255);

        private static void Bubble(PixelCanvas c, float x, float y, float r, byte alpha = 255)
        {
            var rim = BubbleRim; rim.a = alpha;
            var fill = BubbleFill; fill.a = (byte)(BubbleFill.a * alpha / 255);
            if (r >= 1.5f) c.FillCircle(x, y, r - 1f, fill);
            c.Ring(x, y, r, 1f, rim);
            if (r >= 1.5f) c.Set(Mathf.RoundToInt(x - r * 0.4f), Mathf.RoundToInt(y + r * 0.4f), new Color32(255, 255, 255, alpha));
        }

        private static void FishBone(PixelCanvas c, int cx, int cy, byte alpha)
        {
            var bone = Bone; bone.a = alpha;
            var shade = BoneShade; shade.a = alpha;
            for (int x = -5; x <= 3; x++) c.Set(cx + x, cy, bone);
            for (int i = 0; i < 3; i++)
            {
                int rx = cx - 3 + i * 2;
                c.Set(rx, cy + 1, bone); c.Set(rx, cy + 2, shade);
                c.Set(rx, cy - 1, bone); c.Set(rx, cy - 2, shade);
            }
            // Skull
            c.FillRect(cx + 3, cy - 2, 3, 5, bone);
            c.Set(cx + 4, cy + 1, new Color32(20, 20, 30, alpha));
            // Tail
            c.Set(cx - 6, cy + 1, bone); c.Set(cx - 6, cy - 1, bone);
            c.Set(cx - 7, cy + 2, shade); c.Set(cx - 7, cy - 2, shade);
        }

        /// <summary>6-frame pop: flash, ring of scales, fish bone, bubbles drifting up.</summary>
        public static PixelCanvas[] Death(FishStyle style)
        {
            const int size = 28;
            const float cx = 14f, cy = 14f;
            var frames = new PixelCanvas[6];
            Vector2[] dirs = new Vector2[7];
            for (int i = 0; i < dirs.Length; i++)
            {
                float a = (i / (float)dirs.Length) * Mathf.PI * 2f + 0.3f;
                dirs[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            }

            for (int f = 0; f < frames.Length; f++)
            {
                var c = new PixelCanvas(size, size);
                switch (f)
                {
                    case 0:
                        c.FillCircle(cx, cy, 5f, style.highlight);
                        c.FillCircle(cx, cy, 3.5f, White);
                        break;
                    case 1:
                        c.Ring(cx, cy, 8f, 2f, style.highlight);
                        c.FillCircle(cx, cy, 2.5f, White);
                        foreach (var d in dirs) c.FillRect(Mathf.RoundToInt(cx + d.x * 6f), Mathf.RoundToInt(cy + d.y * 6f), 2, 2, style.mid);
                        break;
                    case 2:
                        c.Ring(cx, cy, 11f, 1f, style.top);
                        foreach (var d in dirs) c.FillRect(Mathf.RoundToInt(cx + d.x * 9f), Mathf.RoundToInt(cy + d.y * 9f), 2, 2, style.fin);
                        FishBone(c, 14, 14, 255);
                        Bubble(c, 8, 18, 2f);
                        break;
                    case 3:
                        foreach (var d in dirs) c.Set(Mathf.RoundToInt(cx + d.x * 11f), Mathf.RoundToInt(cy + d.y * 11f), style.fin);
                        FishBone(c, 14, 13, 230);
                        Bubble(c, 7, 21, 2.5f);
                        Bubble(c, 19, 19, 1.5f);
                        break;
                    case 4:
                        FishBone(c, 14, 12, 150);
                        Bubble(c, 7, 24, 2.5f, 200);
                        Bubble(c, 19, 22, 2f, 200);
                        Bubble(c, 13, 20, 1f, 200);
                        break;
                    default:
                        FishBone(c, 14, 11, 70);
                        Bubble(c, 18, 25, 2f, 120);
                        Bubble(c, 12, 23, 1f, 120);
                        break;
                }

                frames[f] = c;
            }

            return frames;
        }

        /// <summary>3-frame shimmering bubble shot.</summary>
        public static PixelCanvas[] Projectile()
        {
            var frames = new PixelCanvas[3];
            Vector2Int[] glints = { new Vector2Int(3, 6), new Vector2Int(4, 6), new Vector2Int(3, 5) };
            for (int f = 0; f < frames.Length; f++)
            {
                var c = new PixelCanvas(10, 10);
                float r = f == 1 ? 3.6f : 3.3f;
                c.FillCircle(4.5f, 4.5f, r, new Color32(120, 215, 255, 255));
                c.FillCircle(4.5f, 4.5f, r - 1.2f, new Color32(205, 245, 255, 255));
                c.FillCircle(5f, 4f, 1f, new Color32(255, 255, 255, 255));
                c.Set(glints[f].x, glints[f].y, White);
                c.Set(glints[f].x - 1, glints[f].y, White);
                c.Outline(new Color32(20, 70, 120, 255));
                frames[f] = c;
            }

            return frames;
        }

        /// <summary>4-frame water splash at the fish's mouth when firing.</summary>
        public static PixelCanvas[] Muzzle()
        {
            var frames = new PixelCanvas[4];
            for (int f = 0; f < frames.Length; f++)
            {
                var c = new PixelCanvas(16, 16);
                switch (f)
                {
                    case 0:
                        c.FillCircle(8, 8, 2.5f, White);
                        c.Ring(8, 8, 3.5f, 1f, BubbleRim);
                        break;
                    case 1:
                        c.Ring(8, 8, 5f, 1f, BubbleRim);
                        c.FillCircle(8, 8, 1.5f, White);
                        c.Set(12, 11, White); c.Set(12, 5, White);
                        break;
                    case 2:
                        c.Ring(8, 8, 6.5f, 1f, new Color32(190, 240, 255, 180));
                        Bubble(c, 12, 11, 1.5f);
                        Bubble(c, 11, 4, 1f);
                        break;
                    default:
                        Bubble(c, 13, 12, 1.5f, 150);
                        Bubble(c, 12, 3, 1f, 150);
                        break;
                }

                frames[f] = c;
            }

            return frames;
        }
    }
}
