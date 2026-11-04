using UnityEngine;

namespace FishHunter.EditorTools
{
    /// <summary>Frame-by-frame effects: enemy death burst, projectile bubble, muzzle splash.</summary>
    public static class EffectPainter
    {
        private static readonly Color32 _white = new(255, 255, 255, 255);
        private static readonly Color32 _bubbleRim = new(190, 240, 255, 255);
        private static readonly Color32 _bubbleFill = new(160, 225, 255, 90);
        private static readonly Color32 _bone = new(235, 235, 220, 255);
        private static readonly Color32 _boneShade = new(150, 160, 170, 255);

        private static void Bubble(PixelCanvas c, float x, float y, float r, byte alpha = 255)
        {
            Color32 rim = _bubbleRim; rim.a = alpha;
            Color32 fill = _bubbleFill; fill.a = (byte)(_bubbleFill.a * alpha / 255);
            if (r >= 1.5f) c.FillCircle(x, y, r - 1f, fill);
            c.Ring(x, y, r, 1f, rim);
            if (r >= 1.5f) c.Set(Mathf.RoundToInt(x - r * 0.4f), Mathf.RoundToInt(y + r * 0.4f), new Color32(255, 255, 255, alpha));
        }

        private static void FishBone(PixelCanvas c, int cx, int cy, byte alpha)
        {
            Color32 bone = _bone; bone.a = alpha;
            Color32 shade = _boneShade; shade.a = alpha;
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
            const int Size = 28;
            const float CenterX = 14f, CenterY = 14f;
            var frames = new PixelCanvas[6];
            Vector2[] dirs = new Vector2[7];
            for (int i = 0; i < dirs.Length; i++)
            {
                float a = (i / (float)dirs.Length) * Mathf.PI * 2f + 0.3f;
                dirs[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            }

            for (int f = 0; f < frames.Length; f++)
            {
                var c = new PixelCanvas(Size, Size);
                switch (f)
                {
                    case 0:
                        c.FillCircle(CenterX, CenterY, 5f, style.Highlight);
                        c.FillCircle(CenterX, CenterY, 3.5f, _white);
                        break;
                    case 1:
                        c.Ring(CenterX, CenterY, 8f, 2f, style.Highlight);
                        c.FillCircle(CenterX, CenterY, 2.5f, _white);
                        foreach (Vector2 d in dirs) c.FillRect(Mathf.RoundToInt(CenterX + d.x * 6f), Mathf.RoundToInt(CenterY + d.y * 6f), 2, 2, style.Mid);
                        break;
                    case 2:
                        c.Ring(CenterX, CenterY, 11f, 1f, style.Top);
                        foreach (Vector2 d in dirs) c.FillRect(Mathf.RoundToInt(CenterX + d.x * 9f), Mathf.RoundToInt(CenterY + d.y * 9f), 2, 2, style.Fin);
                        FishBone(c, 14, 14, 255);
                        Bubble(c, 8, 18, 2f);
                        break;
                    case 3:
                        foreach (Vector2 d in dirs) c.Set(Mathf.RoundToInt(CenterX + d.x * 11f), Mathf.RoundToInt(CenterY + d.y * 11f), style.Fin);
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
            Vector2Int[] glints = { new(3, 6), new(4, 6), new(3, 5) };
            for (int f = 0; f < frames.Length; f++)
            {
                var c = new PixelCanvas(10, 10);
                float r = f == 1 ? 3.6f : 3.3f;
                c.FillCircle(4.5f, 4.5f, r, new Color32(120, 215, 255, 255));
                c.FillCircle(4.5f, 4.5f, r - 1.2f, new Color32(205, 245, 255, 255));
                c.FillCircle(5f, 4f, 1f, new Color32(255, 255, 255, 255));
                c.Set(glints[f].x, glints[f].y, _white);
                c.Set(glints[f].x - 1, glints[f].y, _white);
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
                        c.FillCircle(8, 8, 2.5f, _white);
                        c.Ring(8, 8, 3.5f, 1f, _bubbleRim);
                        break;
                    case 1:
                        c.Ring(8, 8, 5f, 1f, _bubbleRim);
                        c.FillCircle(8, 8, 1.5f, _white);
                        c.Set(12, 11, _white); c.Set(12, 5, _white);
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
