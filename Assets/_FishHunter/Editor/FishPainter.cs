using UnityEngine;

namespace FishHunter.EditorTools
{
    /// <summary>Palette + proportions for one procedurally painted fish (facing +X).</summary>
    public struct FishStyle
    {
        public int canvasW, canvasH;
        public float cx, cy, rx, ry;
        public int tailLength;
        public float tailSpread;
        public float tailWag;
        public int dorsalHeight;
        public int ventralHeight;
        public int stripes;
        public int noseLength;
        public bool spots;
        public Color32 outline, highlight, top, mid, belly, fin, finDark, stripe;
    }

    /// <summary>Paints side-view pixel fish with animated tail and fins.</summary>
    public static class FishPainter
    {
        private static readonly Color32 EyeWhite = new Color32(250, 250, 245, 255);
        private static readonly Color32 Pupil = new Color32(15, 15, 25, 255);
        private static readonly Color32 MouthInside = new Color32(120, 20, 40, 255);

        /// <param name="phase">Swim cycle 0..1 (tail wag / fin flap).</param>
        /// <param name="mouthOpen">0 closed .. 1 wide open.</param>
        /// <param name="shiftX">Horizontal offset in pixels (shoot recoil).</param>
        public static PixelCanvas Paint(FishStyle s, float phase, float mouthOpen = 0f, int shiftX = 0)
        {
            var c = new PixelCanvas(s.canvasW, s.canvasH);
            float cx = s.cx + shiftX;
            float wave = Mathf.Sin(phase * Mathf.PI * 2f);
            float wave2 = Mathf.Sin(phase * Mathf.PI * 2f + 1.3f);

            PaintTail(c, s, cx, wave);
            PaintDorsal(c, s, cx, wave2);
            PaintBody(c, s, cx);
            PaintNose(c, s, cx);
            PaintPectoral(c, s, cx, wave2);
            PaintEye(c, s, cx);
            PaintMouth(c, s, cx, mouthOpen);
            c.Outline(s.outline);
            return c;
        }

        private static float BodyHalfHeight(FishStyle s, float cx, float x)
        {
            float nx = (x - cx) / s.rx;
            return nx * nx >= 1f ? 0f : s.ry * Mathf.Sqrt(1f - nx * nx);
        }

        private static void PaintTail(PixelCanvas c, FishStyle s, float cx, float wave)
        {
            int baseX = Mathf.RoundToInt(cx - s.rx + 2);
            for (int i = 0; i <= s.tailLength; i++)
            {
                int x = baseX - i;
                float t = i / (float)s.tailLength;
                float centerY = s.cy + wave * s.tailWag * t;
                float half = 0.8f + t * s.tailSpread;
                float notch = t > 0.7f ? (t - 0.7f) / 0.3f * half * 0.75f : -1f;

                for (int y = Mathf.FloorToInt(centerY - half); y <= Mathf.CeilToInt(centerY + half); y++)
                {
                    float dy = Mathf.Abs(y - centerY);
                    if (dy > half) continue;
                    if (dy < notch) continue; // forked tail
                    bool ray = (Mathf.RoundToInt(y - centerY) & 1) == 0 && t > 0.35f;
                    c.Set(x, y, ray ? s.finDark : s.fin);
                }
            }
        }

        private static void PaintDorsal(PixelCanvas c, FishStyle s, float cx, float wave)
        {
            float x0 = cx - s.rx * 0.55f;
            float x1 = cx + s.rx * 0.25f;
            float flutter = 1f + 0.18f * wave;

            for (int x = Mathf.FloorToInt(x0); x <= Mathf.CeilToInt(x1); x++)
            {
                float t = Mathf.InverseLerp(x0, x1, x);
                float profile = t < 0.3f ? t / 0.3f : (1f - t) / 0.7f;
                float bodyTop = s.cy + BodyHalfHeight(s, cx, x);

                float h = s.dorsalHeight * profile * flutter;
                for (int y = Mathf.FloorToInt(bodyTop - 1); y <= Mathf.RoundToInt(bodyTop + h); y++)
                    c.Set(x, y, (x & 1) == 0 ? s.fin : s.finDark);

                float bodyBottom = s.cy - BodyHalfHeight(s, cx, x);
                float hv = s.ventralHeight * profile * (2f - flutter);
                if (t > 0.25f)
                    for (int y = Mathf.RoundToInt(bodyBottom - hv); y <= Mathf.CeilToInt(bodyBottom + 1); y++)
                        c.Set(x, y, s.finDark);
            }
        }

        private static void PaintBody(PixelCanvas c, FishStyle s, float cx)
        {
            for (int y = Mathf.FloorToInt(s.cy - s.ry); y <= Mathf.CeilToInt(s.cy + s.ry); y++)
            for (int x = Mathf.FloorToInt(cx - s.rx); x <= Mathf.CeilToInt(cx + s.rx); x++)
            {
                float nx = (x - cx) / s.rx;
                float ny = (y - s.cy) / s.ry;
                if (nx * nx + ny * ny > 1f) continue;

                Color32 col = ny > 0.45f ? s.top : ny < -0.4f ? s.belly : s.mid;
                if (ny > 0.55f && nx > -0.1f && nx < 0.55f) col = s.highlight;

                // Vertical stripes across the flank.
                for (int k = 0; k < s.stripes; k++)
                {
                    float sx = cx - s.rx * 0.5f + k * (s.rx * 0.9f / Mathf.Max(1, s.stripes - 1 + 0.001f));
                    if (s.stripes == 1) sx = cx;
                    if (Mathf.Abs(x - sx) < 0.6f && ny > -0.55f) col = s.stripe;
                }

                if (s.spots && ((x * 7 + y * 13) % 11 == 0) && ny > -0.3f && nx < 0.5f) col = s.stripe;

                // Gill line.
                if (Mathf.Abs(nx - 0.42f) < 0.06f && Mathf.Abs(ny) < 0.55f) col = s.stripe;

                c.Set(x, y, col);
            }
        }

        private static void PaintNose(PixelCanvas c, FishStyle s, float cx)
        {
            if (s.noseLength <= 0) return;
            int x0 = Mathf.RoundToInt(cx + s.rx - 1);
            int y = Mathf.RoundToInt(s.cy - s.ry * 0.1f);
            for (int i = 0; i < s.noseLength; i++)
            {
                c.Set(x0 + i, y, s.top);
                if (i < s.noseLength / 2) c.Set(x0 + i, y - 1, s.mid);
            }
        }

        private static void PaintPectoral(PixelCanvas c, FishStyle s, float cx, float wave)
        {
            float px = cx + s.rx * 0.05f;
            float py = s.cy - s.ry * 0.2f;
            float len = Mathf.Max(2f, s.rx * 0.3f);
            float lift = wave * Mathf.Max(1f, s.ry * 0.2f);
            for (int i = 0; i <= Mathf.RoundToInt(len); i++)
            {
                float t = i / len;
                int x = Mathf.RoundToInt(px - i);
                int y = Mathf.RoundToInt(py - t * 1.5f + lift * t);
                c.Set(x, y, s.finDark);
                if (t < 0.7f) c.Set(x, y + 1, s.fin);
            }
        }

        private static void PaintEye(PixelCanvas c, FishStyle s, float cx)
        {
            int ex = Mathf.RoundToInt(cx + s.rx * 0.58f);
            int ey = Mathf.RoundToInt(s.cy + s.ry * 0.22f);
            if (s.ry >= 5f)
            {
                c.FillRect(ex - 1, ey - 1, 3, 3, EyeWhite);
                c.FillRect(ex, ey - 1, 2, 2, Pupil);
            }
            else
            {
                c.FillRect(ex - 1, ey, 2, 2, EyeWhite);
                c.Set(ex, ey, Pupil);
            }
        }

        private static void PaintMouth(PixelCanvas c, FishStyle s, float cx, float mouthOpen)
        {
            int tipX = Mathf.RoundToInt(cx + s.rx);
            int my = Mathf.RoundToInt(s.cy - s.ry * 0.2f);

            if (mouthOpen <= 0.05f)
            {
                c.Set(tipX - 1, my, s.outline);
                c.Set(tipX - 2, my, s.outline);
                return;
            }

            int depth = Mathf.Max(1, Mathf.RoundToInt(mouthOpen * s.ry * 0.7f));
            for (int d = 0; d <= depth + 1; d++)
            {
                int x = tipX - d;
                int half = Mathf.RoundToInt((depth - d + 1) * 0.8f);
                for (int y = my - half; y <= my + half; y++)
                    c.Set(x, y, d == 0 || Mathf.Abs(y - my) == half ? PixelCanvas.Clear : MouthInside);
            }

            // Re-cut the very front so the open mouth reads as a notch.
            for (int y = my - depth; y <= my + depth; y++) c.Set(tipX + 1, y, PixelCanvas.Clear);
        }

        // ---------- Styles ----------

        public static FishStyle Player => new FishStyle
        {
            canvasW = 36, canvasH = 28, cx = 21, cy = 13, rx = 11, ry = 7.5f,
            tailLength = 8, tailSpread = 6f, tailWag = 3f, dorsalHeight = 5, ventralHeight = 3, stripes = 2,
            outline = new Color32(14, 28, 58, 255),
            highlight = new Color32(190, 245, 255, 255),
            top = new Color32(80, 200, 235, 255),
            mid = new Color32(40, 145, 205, 255),
            belly = new Color32(232, 244, 250, 255),
            fin = new Color32(255, 205, 70, 255),
            finDark = new Color32(225, 140, 35, 255),
            stripe = new Color32(28, 88, 150, 255)
        };

        public static FishStyle EnemyA => new FishStyle
        {
            canvasW = 26, canvasH = 22, cx = 15, cy = 10, rx = 8.5f, ry = 6.5f,
            tailLength = 6, tailSpread = 4f, tailWag = 2f, dorsalHeight = 3, ventralHeight = 2, stripes = 0, spots = true,
            outline = new Color32(30, 45, 22, 255),
            highlight = new Color32(220, 245, 150, 255),
            top = new Color32(170, 210, 80, 255),
            mid = new Color32(112, 170, 60, 255),
            belly = new Color32(225, 232, 165, 255),
            fin = new Color32(95, 145, 55, 255),
            finDark = new Color32(60, 100, 40, 255),
            stripe = new Color32(68, 108, 38, 255)
        };

        public static FishStyle EnemyB => new FishStyle
        {
            canvasW = 28, canvasH = 18, cx = 16, cy = 8, rx = 9.5f, ry = 4.5f,
            tailLength = 7, tailSpread = 4.5f, tailWag = 2f, dorsalHeight = 3, ventralHeight = 2, stripes = 2,
            outline = new Color32(62, 18, 24, 255),
            highlight = new Color32(255, 205, 150, 255),
            top = new Color32(255, 140, 65, 255),
            mid = new Color32(232, 78, 48, 255),
            belly = new Color32(255, 218, 175, 255),
            fin = new Color32(205, 45, 50, 255),
            finDark = new Color32(150, 28, 40, 255),
            stripe = new Color32(255, 245, 235, 255)
        };

        public static FishStyle EnemyC => new FishStyle
        {
            canvasW = 33, canvasH = 16, cx = 16, cy = 7, rx = 9.5f, ry = 3.4f,
            tailLength = 6, tailSpread = 3.5f, tailWag = 1.5f, dorsalHeight = 3, ventralHeight = 1, stripes = 3, noseLength = 6,
            outline = new Color32(30, 14, 52, 255),
            highlight = new Color32(225, 195, 255, 255),
            top = new Color32(178, 118, 240, 255),
            mid = new Color32(118, 68, 200, 255),
            belly = new Color32(205, 232, 255, 255),
            fin = new Color32(70, 225, 232, 255),
            finDark = new Color32(30, 150, 182, 255),
            stripe = new Color32(80, 38, 150, 255)
        };
    }
}
