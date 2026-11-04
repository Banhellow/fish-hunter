using UnityEngine;

namespace FishHunter.EditorTools
{
    /// <summary>Palette + proportions for one procedurally painted fish (facing +X).</summary>
    public struct FishStyle
    {
        public int CanvasWidth, CanvasHeight;
        public float CenterX, CenterY, RadiusX, RadiusY;
        public int TailLength;
        public float TailSpread;
        public float TailWag;
        public int DorsalHeight;
        public int VentralHeight;
        public int Stripes;
        public int NoseLength;
        public bool Spots;
        public Color32 Outline, Highlight, Top, Mid, Belly, Fin, FinDark, Stripe;
    }

    /// <summary>Paints side-view pixel fish with animated tail and fins.</summary>
    public static class FishPainter
    {
        private static readonly Color32 _eyeWhite = new(250, 250, 245, 255);
        private static readonly Color32 _pupil = new(15, 15, 25, 255);
        private static readonly Color32 _mouthInside = new(120, 20, 40, 255);

        /// <param name="phase">Swim cycle 0..1 (tail wag / fin flap).</param>
        /// <param name="mouthOpen">0 closed .. 1 wide open.</param>
        /// <param name="shiftX">Horizontal offset in pixels (shoot recoil).</param>
        public static PixelCanvas Paint(FishStyle s, float phase, float mouthOpen = 0f, int shiftX = 0)
        {
            var c = new PixelCanvas(s.CanvasWidth, s.CanvasHeight);
            float cx = s.CenterX + shiftX;
            float wave = Mathf.Sin(phase * Mathf.PI * 2f);
            float wave2 = Mathf.Sin(phase * Mathf.PI * 2f + 1.3f);

            PaintTail(c, s, cx, wave);
            PaintDorsal(c, s, cx, wave2);
            PaintBody(c, s, cx);
            PaintNose(c, s, cx);
            PaintPectoral(c, s, cx, wave2);
            PaintEye(c, s, cx);
            PaintMouth(c, s, cx, mouthOpen);
            c.Outline(s.Outline);
            return c;
        }

        private static float BodyHalfHeight(FishStyle s, float cx, float x)
        {
            float nx = (x - cx) / s.RadiusX;
            return nx * nx >= 1f ? 0f : s.RadiusY * Mathf.Sqrt(1f - nx * nx);
        }

        private static void PaintTail(PixelCanvas c, FishStyle s, float cx, float wave)
        {
            int baseX = Mathf.RoundToInt(cx - s.RadiusX + 2);
            for (int i = 0; i <= s.TailLength; i++)
            {
                int x = baseX - i;
                float t = i / (float)s.TailLength;
                float centerY = s.CenterY + wave * s.TailWag * t;
                float half = 0.8f + t * s.TailSpread;
                float notch = t > 0.7f ? (t - 0.7f) / 0.3f * half * 0.75f : -1f;

                for (int y = Mathf.FloorToInt(centerY - half); y <= Mathf.CeilToInt(centerY + half); y++)
                {
                    float dy = Mathf.Abs(y - centerY);
                    if (dy > half) continue;
                    if (dy < notch) continue; // forked tail
                    bool ray = (Mathf.RoundToInt(y - centerY) & 1) == 0 && t > 0.35f;
                    c.Set(x, y, ray ? s.FinDark : s.Fin);
                }
            }
        }

        private static void PaintDorsal(PixelCanvas c, FishStyle s, float cx, float wave)
        {
            float x0 = cx - s.RadiusX * 0.55f;
            float x1 = cx + s.RadiusX * 0.25f;
            float flutter = 1f + 0.18f * wave;

            for (int x = Mathf.FloorToInt(x0); x <= Mathf.CeilToInt(x1); x++)
            {
                float t = Mathf.InverseLerp(x0, x1, x);
                float profile = t < 0.3f ? t / 0.3f : (1f - t) / 0.7f;
                float bodyTop = s.CenterY + BodyHalfHeight(s, cx, x);

                float h = s.DorsalHeight * profile * flutter;
                for (int y = Mathf.FloorToInt(bodyTop - 1); y <= Mathf.RoundToInt(bodyTop + h); y++)
                    c.Set(x, y, (x & 1) == 0 ? s.Fin : s.FinDark);

                float bodyBottom = s.CenterY - BodyHalfHeight(s, cx, x);
                float hv = s.VentralHeight * profile * (2f - flutter);
                if (t > 0.25f)
                    for (int y = Mathf.RoundToInt(bodyBottom - hv); y <= Mathf.CeilToInt(bodyBottom + 1); y++)
                        c.Set(x, y, s.FinDark);
            }
        }

        private static void PaintBody(PixelCanvas c, FishStyle s, float cx)
        {
            for (int y = Mathf.FloorToInt(s.CenterY - s.RadiusY); y <= Mathf.CeilToInt(s.CenterY + s.RadiusY); y++)
                for (int x = Mathf.FloorToInt(cx - s.RadiusX); x <= Mathf.CeilToInt(cx + s.RadiusX); x++)
                {
                    float nx = (x - cx) / s.RadiusX;
                    float ny = (y - s.CenterY) / s.RadiusY;
                    if (nx * nx + ny * ny > 1f) continue;

                    Color32 col = ny > 0.45f ? s.Top : ny < -0.4f ? s.Belly : s.Mid;
                    if (ny > 0.55f && nx > -0.1f && nx < 0.55f) col = s.Highlight;

                    // Vertical stripes across the flank.
                    for (int k = 0; k < s.Stripes; k++)
                    {
                        float sx = cx - s.RadiusX * 0.5f + k * (s.RadiusX * 0.9f / Mathf.Max(1, s.Stripes - 1 + 0.001f));
                        if (s.Stripes == 1) sx = cx;
                        if (Mathf.Abs(x - sx) < 0.6f && ny > -0.55f) col = s.Stripe;
                    }

                    if (s.Spots && ((x * 7 + y * 13) % 11 == 0) && ny > -0.3f && nx < 0.5f) col = s.Stripe;

                    // Gill line.
                    if (Mathf.Abs(nx - 0.42f) < 0.06f && Mathf.Abs(ny) < 0.55f) col = s.Stripe;

                    c.Set(x, y, col);
                }
        }

        private static void PaintNose(PixelCanvas c, FishStyle s, float cx)
        {
            if (s.NoseLength <= 0) return;
            int x0 = Mathf.RoundToInt(cx + s.RadiusX - 1);
            int y = Mathf.RoundToInt(s.CenterY - s.RadiusY * 0.1f);
            for (int i = 0; i < s.NoseLength; i++)
            {
                c.Set(x0 + i, y, s.Top);
                if (i < s.NoseLength / 2) c.Set(x0 + i, y - 1, s.Mid);
            }
        }

        private static void PaintPectoral(PixelCanvas c, FishStyle s, float cx, float wave)
        {
            float px = cx + s.RadiusX * 0.05f;
            float py = s.CenterY - s.RadiusY * 0.2f;
            float len = Mathf.Max(2f, s.RadiusX * 0.3f);
            float lift = wave * Mathf.Max(1f, s.RadiusY * 0.2f);
            for (int i = 0; i <= Mathf.RoundToInt(len); i++)
            {
                float t = i / len;
                int x = Mathf.RoundToInt(px - i);
                int y = Mathf.RoundToInt(py - t * 1.5f + lift * t);
                c.Set(x, y, s.FinDark);
                if (t < 0.7f) c.Set(x, y + 1, s.Fin);
            }
        }

        private static void PaintEye(PixelCanvas c, FishStyle s, float cx)
        {
            int ex = Mathf.RoundToInt(cx + s.RadiusX * 0.58f);
            int ey = Mathf.RoundToInt(s.CenterY + s.RadiusY * 0.22f);
            if (s.RadiusY >= 5f)
            {
                c.FillRect(ex - 1, ey - 1, 3, 3, _eyeWhite);
                c.FillRect(ex, ey - 1, 2, 2, _pupil);
            }
            else
            {
                c.FillRect(ex - 1, ey, 2, 2, _eyeWhite);
                c.Set(ex, ey, _pupil);
            }
        }

        private static void PaintMouth(PixelCanvas c, FishStyle s, float cx, float mouthOpen)
        {
            int tipX = Mathf.RoundToInt(cx + s.RadiusX);
            int my = Mathf.RoundToInt(s.CenterY - s.RadiusY * 0.2f);

            if (mouthOpen <= 0.05f)
            {
                c.Set(tipX - 1, my, s.Outline);
                c.Set(tipX - 2, my, s.Outline);
                return;
            }

            int depth = Mathf.Max(1, Mathf.RoundToInt(mouthOpen * s.RadiusY * 0.7f));
            for (int d = 0; d <= depth + 1; d++)
            {
                int x = tipX - d;
                int half = Mathf.RoundToInt((depth - d + 1) * 0.8f);
                for (int y = my - half; y <= my + half; y++)
                    c.Set(x, y, d == 0 || Mathf.Abs(y - my) == half ? PixelCanvas.Clear : _mouthInside);
            }

            // Re-cut the very front so the open mouth reads as a notch.
            for (int y = my - depth; y <= my + depth; y++) c.Set(tipX + 1, y, PixelCanvas.Clear);
        }

        // ---------- Styles ----------

        public static FishStyle Player => new()
        {
            CanvasWidth = 36,
            CanvasHeight = 28,
            CenterX = 21,
            CenterY = 13,
            RadiusX = 11,
            RadiusY = 7.5f,
            TailLength = 8,
            TailSpread = 6f,
            TailWag = 3f,
            DorsalHeight = 5,
            VentralHeight = 3,
            Stripes = 2,
            Outline = new Color32(14, 28, 58, 255),
            Highlight = new Color32(190, 245, 255, 255),
            Top = new Color32(80, 200, 235, 255),
            Mid = new Color32(40, 145, 205, 255),
            Belly = new Color32(232, 244, 250, 255),
            Fin = new Color32(255, 205, 70, 255),
            FinDark = new Color32(225, 140, 35, 255),
            Stripe = new Color32(28, 88, 150, 255)
        };

        public static FishStyle EnemyA => new()
        {
            CanvasWidth = 26,
            CanvasHeight = 22,
            CenterX = 15,
            CenterY = 10,
            RadiusX = 8.5f,
            RadiusY = 6.5f,
            TailLength = 6,
            TailSpread = 4f,
            TailWag = 2f,
            DorsalHeight = 3,
            VentralHeight = 2,
            Stripes = 0,
            Spots = true,
            Outline = new Color32(30, 45, 22, 255),
            Highlight = new Color32(220, 245, 150, 255),
            Top = new Color32(170, 210, 80, 255),
            Mid = new Color32(112, 170, 60, 255),
            Belly = new Color32(225, 232, 165, 255),
            Fin = new Color32(95, 145, 55, 255),
            FinDark = new Color32(60, 100, 40, 255),
            Stripe = new Color32(68, 108, 38, 255)
        };

        public static FishStyle EnemyB => new()
        {
            CanvasWidth = 28,
            CanvasHeight = 18,
            CenterX = 16,
            CenterY = 8,
            RadiusX = 9.5f,
            RadiusY = 4.5f,
            TailLength = 7,
            TailSpread = 4.5f,
            TailWag = 2f,
            DorsalHeight = 3,
            VentralHeight = 2,
            Stripes = 2,
            Outline = new Color32(62, 18, 24, 255),
            Highlight = new Color32(255, 205, 150, 255),
            Top = new Color32(255, 140, 65, 255),
            Mid = new Color32(232, 78, 48, 255),
            Belly = new Color32(255, 218, 175, 255),
            Fin = new Color32(205, 45, 50, 255),
            FinDark = new Color32(150, 28, 40, 255),
            Stripe = new Color32(255, 245, 235, 255)
        };

        public static FishStyle EnemyC => new()
        {
            CanvasWidth = 33,
            CanvasHeight = 16,
            CenterX = 16,
            CenterY = 7,
            RadiusX = 9.5f,
            RadiusY = 3.4f,
            TailLength = 6,
            TailSpread = 3.5f,
            TailWag = 1.5f,
            DorsalHeight = 3,
            VentralHeight = 1,
            Stripes = 3,
            NoseLength = 6,
            Outline = new Color32(30, 14, 52, 255),
            Highlight = new Color32(225, 195, 255, 255),
            Top = new Color32(178, 118, 240, 255),
            Mid = new Color32(118, 68, 200, 255),
            Belly = new Color32(205, 232, 255, 255),
            Fin = new Color32(70, 225, 232, 255),
            FinDark = new Color32(30, 150, 182, 255),
            Stripe = new Color32(80, 38, 150, 255)
        };
    }
}
