using UnityEngine;

namespace FishHunter.EditorTools
{
    /// <summary>Top-down seabed: rippled sand, rocks, kelp, coral, starfish and shells under a water tint.</summary>
    public static class BackgroundPainter
    {
        public const int Width = 640;
        public const int Height = 360;

        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 982451653;
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;
                return (h & 0x7fffffff) / (float)int.MaxValue;
            }
        }

        private static float ValueNoise(float x, float y, int seed)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            float a = Hash(x0, y0, seed), b = Hash(x0 + 1, y0, seed);
            float c = Hash(x0, y0 + 1, seed), d = Hash(x0 + 1, y0 + 1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        public static PixelCanvas Seabed()
        {
            var c = new PixelCanvas(Width, Height);
            var rng = new System.Random(1337);

            Color32 sandDark = new(168, 148, 100, 255);
            Color32 sand = new(196, 178, 126, 255);
            Color32 sandLight = new(218, 202, 150, 255);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    float n = ValueNoise(x / 38f, y / 38f, 1) * 0.65f + ValueNoise(x / 12f, y / 12f, 2) * 0.35f;
                    float ripple = Mathf.Sin(x * 0.16f + y * 0.05f + n * 3.2f);
                    Color32 col = n > 0.64f ? sandLight : n < 0.36f ? sandDark : sand;
                    if (ripple > 0.93f) col = sandDark;
                    else if (ripple > 0.72f && ripple <= 0.84f) col = sandLight;
                    if (Hash(x, y, 7) > 0.985f) col = Hash(x, y, 8) > 0.5f ? sandLight : sandDark;
                    c.Set(x, y, col);
                }

            Vector2 center = new(Width / 2f, Height / 2f);
            bool FarFromCenter(int x, int y, float min) => Vector2.Distance(new Vector2(x, y), center) > min;

            // Dark sand patches (depressions)
            for (int i = 0; i < 9; i++)
            {
                int x = rng.Next(Width), y = rng.Next(Height);
                float rx = rng.Next(18, 40), ry = rng.Next(10, 24);
                for (int yy = (int)(y - ry); yy <= y + ry; yy++)
                    for (int xx = (int)(x - rx); xx <= x + rx; xx++)
                    {
                        float nx = (xx - x) / rx, ny = (yy - y) / ry;
                        float d = nx * nx + ny * ny + (ValueNoise(xx / 6f, yy / 6f, 3) - 0.5f) * 0.5f;
                        if (d < 1f && ((xx + yy) % 2 == 0 || d < 0.7f)) c.Set(xx, yy, sandDark);
                    }
            }

            for (int i = 0; i < 16; i++)
            {
                int x = rng.Next(Width), y = rng.Next(Height);
                if (!FarFromCenter(x, y, 70)) continue;
                Rock(c, x, y, rng.Next(6, 16), rng.Next(5, 11), rng);
            }

            for (int i = 0; i < 14; i++)
            {
                int x = rng.Next(Width), y = rng.Next(Height);
                if (!FarFromCenter(x, y, 80)) continue;
                Kelp(c, x, y, rng);
            }

            for (int i = 0; i < 8; i++)
            {
                int x = rng.Next(Width), y = rng.Next(Height);
                if (!FarFromCenter(x, y, 80)) continue;
                Coral(c, x, y, rng);
            }

            for (int i = 0; i < 10; i++)
            {
                int x = rng.Next(10, Width - 10), y = rng.Next(10, Height - 10);
                if (!FarFromCenter(x, y, 60)) continue;
                Starfish(c, x, y, rng.Next(4, 7), rng.NextDouble() * Mathf.PI);
            }

            for (int i = 0; i < 26; i++)
            {
                int x = rng.Next(Width), y = rng.Next(Height);
                Shell(c, x, y, rng);
            }

            // Water tint + vignette
            Color32 water = new(20, 120, 150, 255);
            Color32 deep = new(10, 40, 80, 255);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    Color32 src = c.Get(x, y);
                    Color tinted = Color.Lerp(src, water, 0.38f);
                    float dx = (x - center.x) / center.x, dy = (y - center.y) / center.y;
                    float v = Mathf.Clamp01((dx * dx + dy * dy - 0.55f) * 0.5f);
                    // Dithered vignette: ordered 2x2 threshold keeps it pixel-art without visible bands.
                    float threshold = ((x & 1) * 2 + (y & 1)) / 4f * 0.12f;
                    v = Mathf.Floor((v + threshold) * 6f) / 6f;
                    tinted = Color.Lerp(tinted, deep, v);
                    c.Set(x, y, tinted);
                }

            return c;
        }

        private static void Rock(PixelCanvas c, int x, int y, int rx, int ry, System.Random rng)
        {
            Color32 outline = new(48, 50, 60, 255);
            Color32 dark = new(84, 86, 98, 255);
            Color32 mid = new(118, 120, 130, 255);
            Color32 light = new(156, 158, 166, 255);
            Color32 moss = new(78, 128, 70, 255);
            int seed = rng.Next(1000);

            var rock = new PixelCanvas(rx * 2 + 4, ry * 2 + 4);
            float cx = rx + 2, cy = ry + 2;
            for (int yy = 0; yy < rock.Height; yy++)
                for (int xx = 0; xx < rock.Width; xx++)
                {
                    float nx = (xx - cx) / rx, ny = (yy - cy) / ry;
                    float d = nx * nx + ny * ny + (ValueNoise(xx / 3f, yy / 3f, seed) - 0.5f) * 0.6f;
                    if (d > 1f) continue;
                    float light01 = -nx * 0.5f + ny * 0.7f;
                    Color32 col = light01 > 0.35f ? light : light01 < -0.3f ? dark : mid;
                    if (ValueNoise(xx / 2f, yy / 2f, seed + 1) > 0.78f && ny > 0f) col = moss;
                    rock.Set(xx, yy, col);
                }

            rock.Outline(outline);
            // Contact shadow on the sand (bottom-right).
            for (int yy = 0; yy < rock.Height; yy++)
                for (int xx = 0; xx < rock.Width; xx++)
                    if (rock.IsOpaque(xx, yy)) c.Blend(x - rx - 2 + xx + 2, y - ry - 2 + yy - 2, new Color32(40, 40, 30, 90));
            c.Stamp(rock, x - rx - 2, y - ry - 2);
        }

        private static void Kelp(PixelCanvas c, int x, int y, System.Random rng)
        {
            Color32 dark = new(28, 96, 52, 255);
            Color32 mid = new(46, 140, 70, 255);
            Color32 light = new(110, 190, 90, 255);
            int strands = rng.Next(4, 8);
            for (int s = 0; s < strands; s++)
            {
                double angle = rng.NextDouble() * Mathf.PI * 2f;
                int length = rng.Next(14, 30);
                float phase = (float)rng.NextDouble() * 6f;
                Vector2 dir = new(Mathf.Cos((float)angle), Mathf.Sin((float)angle));
                Vector2 side = new(-dir.y, dir.x);
                for (int i = 0; i < length; i++)
                {
                    float wob = Mathf.Sin(i * 0.35f + phase) * 2.2f;
                    Vector2 p = new Vector2(x, y) + dir * i + side * wob;
                    int px = Mathf.RoundToInt(p.x), py = Mathf.RoundToInt(p.y);
                    float width = Mathf.Lerp(1.6f, 0.6f, i / (float)length);
                    c.FillCircle(px, py, width, mid);
                    if (i % 4 == 0) c.Set(px, py, light);
                    if (i % 5 == 2) c.Set(px + 1, py - 1, dark);
                }
            }

            c.FillCircle(x, y, 2f, dark);
        }

        private static void Coral(PixelCanvas c, int x, int y, System.Random rng)
        {
            Color32[] palettes =
            {
                new(232, 96, 120, 255), new(250, 150, 90, 255), new(190, 110, 220, 255)
            };
            Color32 col = palettes[rng.Next(palettes.Length)];
            Color32 dark = new((byte)(col.r * 0.6f), (byte)(col.g * 0.6f), (byte)(col.b * 0.6f), 255);
            Color32 tip = new((byte)Mathf.Min(255, col.r + 40), (byte)Mathf.Min(255, col.g + 60), (byte)Mathf.Min(255, col.b + 50), 255);
            int blobs = rng.Next(5, 9);
            for (int i = 0; i < blobs; i++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                float r = rng.Next(2, 8);
                int bx = x + Mathf.RoundToInt(Mathf.Cos(a) * r), by = y + Mathf.RoundToInt(Mathf.Sin(a) * r);
                c.FillCircle(bx, by, 3f, dark);
                c.FillCircle(bx, by + 1, 2f, col);
                c.Set(bx - 1, by + 2, tip);
            }
        }

        private static void Starfish(PixelCanvas c, int x, int y, int r, double rot)
        {
            Color32 col = new(244, 124, 62, 255);
            Color32 dot = new(255, 214, 150, 255);
            Color32 outline = new(110, 44, 22, 255);
            var s = new PixelCanvas(r * 2 + 5, r * 2 + 5);
            int cx = r + 2, cy = r + 2;
            for (int arm = 0; arm < 5; arm++)
            {
                float a = (float)rot + arm * Mathf.PI * 2f / 5f;
                for (int i = 0; i <= r; i++)
                {
                    float w = Mathf.Lerp(1.4f, 0.4f, i / (float)r);
                    s.FillCircle(cx + Mathf.Cos(a) * i, cy + Mathf.Sin(a) * i, w, col);
                }
                s.Set(Mathf.RoundToInt(cx + Mathf.Cos(a) * r * 0.5f), Mathf.RoundToInt(cy + Mathf.Sin(a) * r * 0.5f), dot);
            }

            s.Set(cx, cy, dot);
            s.Outline(outline);
            c.Stamp(s, x - cx, y - cy);
        }

        private static void Shell(PixelCanvas c, int x, int y, System.Random rng)
        {
            Color32 light = rng.Next(2) == 0 ? new Color32(250, 226, 220, 255) : new Color32(236, 222, 196, 255);
            Color32 dark = new(176, 140, 130, 255);
            c.FillRect(x, y, 3, 2, light);
            c.Set(x + 1, y + 2, light);
            c.Set(x + 1, y, dark);
            c.Set(x - 1, y, dark); c.Set(x + 3, y, dark);
        }

        /// <summary>Frames in one caustics loop. More frames = smaller jumps between frames.</summary>
        public const int CausticFrameCount = 60;
        /// <summary>Playback rate; loop length = CausticFrameCount / CausticFps seconds (6 s).</summary>
        public const float CausticFps = 10f;
        /// <summary>Caustics are drawn in 2x2 art-pixel blocks, so they are stored at half resolution.</summary>
        public const int CausticBlock = 2;

        /// <summary>
        /// Looping caustic light network overlay (transparent), one texel per 2x2 block.
        /// Import at WorldPpu / CausticBlock so it covers the same area as the seabed.
        /// </summary>
        public static PixelCanvas Caustics(int frame, int frameCount)
        {
            var c = new PixelCanvas(Width / CausticBlock, Height / CausticBlock);
            float t = frame / (float)frameCount * Mathf.PI * 2f;
            for (int by = 0; by < c.Height; by++)
                for (int bx = 0; bx < c.Width; bx++)
                {
                    int x = bx * CausticBlock, y = by * CausticBlock;
                    float v1 = Mathf.Sin(x * 0.045f + 1.7f * Mathf.Sin(y * 0.031f + t));
                    float v2 = Mathf.Sin(y * 0.052f + 1.3f * Mathf.Sin(x * 0.027f - t));
                    float line = Mathf.Abs(v1 + v2);
                    Color32 col;
                    if (line < 0.06f) col = new Color32(210, 245, 255, 70);
                    else if (line < 0.14f) col = new Color32(180, 235, 255, 34);
                    else continue;
                    c.Set(bx, by, col);
                }

            return c;
        }
    }
}
