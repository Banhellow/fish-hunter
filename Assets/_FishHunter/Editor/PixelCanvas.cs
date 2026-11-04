using UnityEngine;

namespace FishHunter.EditorTools
{
    /// <summary>Tiny software raster used to paint pixel art (origin bottom-left, like Texture2D).</summary>
    public sealed class PixelCanvas
    {
        public readonly int Width;
        public readonly int Height;
        private readonly Color32[] _pixels;

        public static readonly Color32 Clear = new(0, 0, 0, 0);

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new Color32[width * height];
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public Color32 Get(int x, int y) => InBounds(x, y) ? _pixels[y * Width + x] : Clear;

        public void Set(int x, int y, Color32 c)
        {
            if (InBounds(x, y)) _pixels[y * Width + x] = c;
        }

        /// <summary>Alpha-blends a color over the existing pixel.</summary>
        public void Blend(int x, int y, Color32 c)
        {
            if (!InBounds(x, y)) return;
            Color32 dst = _pixels[y * Width + x];
            float a = c.a / 255f;
            float outA = a + dst.a / 255f * (1f - a);
            if (outA <= 0f) return;
            byte Mix(byte s, byte d) => (byte)Mathf.Clamp(Mathf.RoundToInt((s * a + d * (dst.a / 255f) * (1f - a)) / outA), 0, 255);
            _pixels[y * Width + x] = new Color32(Mix(c.r, dst.r), Mix(c.g, dst.g), Mix(c.b, dst.b), (byte)Mathf.RoundToInt(outA * 255f));
        }

        public bool IsOpaque(int x, int y) => Get(x, y).a > 0;

        public void Fill(Color32 c)
        {
            for (int i = 0; i < _pixels.Length; i++) _pixels[i] = c;
        }

        public void FillRect(int x, int y, int w, int h, Color32 c)
        {
            for (int yy = y; yy < y + h; yy++)
                for (int xx = x; xx < x + w; xx++)
                    Set(xx, yy, c);
        }

        public void FillCircle(float cx, float cy, float r, Color32 c)
        {
            for (int y = Mathf.FloorToInt(cy - r); y <= Mathf.CeilToInt(cy + r); y++)
                for (int x = Mathf.FloorToInt(cx - r); x <= Mathf.CeilToInt(cx + r); x++)
                {
                    float dx = x - cx, dy = y - cy;
                    if (dx * dx + dy * dy <= r * r + 0.25f) Set(x, y, c);
                }
        }

        public void Ring(float cx, float cy, float r, float thickness, Color32 c)
        {
            float inner = Mathf.Max(0f, r - thickness);
            for (int y = Mathf.FloorToInt(cy - r - 1); y <= Mathf.CeilToInt(cy + r + 1); y++)
                for (int x = Mathf.FloorToInt(cx - r - 1); x <= Mathf.CeilToInt(cx + r + 1); x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d <= r + 0.5f && d > inner - 0.5f) Set(x, y, c);
                }
        }

        public void FillEllipse(float cx, float cy, float rx, float ry, Color32 c)
        {
            for (int y = Mathf.FloorToInt(cy - ry); y <= Mathf.CeilToInt(cy + ry); y++)
                for (int x = Mathf.FloorToInt(cx - rx); x <= Mathf.CeilToInt(cx + rx); x++)
                {
                    float nx = (x - cx) / rx, ny = (y - cy) / ry;
                    if (nx * nx + ny * ny <= 1.0f) Set(x, y, c);
                }
        }

        public void Line(int x0, int y0, int x1, int y1, Color32 c)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                Set(x0, y0, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        /// <summary>Adds a 1px outline around every opaque pixel (4-neighbourhood, optionally diagonals).</summary>
        public void Outline(Color32 c, bool diagonals = false)
        {
            var copy = (Color32[])_pixels.Clone();
            bool Opaque(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height && copy[y * Width + x].a > 0;

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    if (copy[y * Width + x].a > 0) continue;
                    bool edge = Opaque(x - 1, y) || Opaque(x + 1, y) || Opaque(x, y - 1) || Opaque(x, y + 1);
                    if (!edge && diagonals)
                        edge = Opaque(x - 1, y - 1) || Opaque(x + 1, y - 1) || Opaque(x - 1, y + 1) || Opaque(x + 1, y + 1);
                    if (edge) _pixels[y * Width + x] = c;
                }
        }

        public PixelCanvas FlipX()
        {
            var result = new PixelCanvas(Width, Height);
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    result.Set(Width - 1 - x, y, Get(x, y));
            return result;
        }

        /// <summary>Draws another canvas onto this one (alpha blended).</summary>
        public void Stamp(PixelCanvas other, int ox, int oy)
        {
            for (int y = 0; y < other.Height; y++)
                for (int x = 0; x < other.Width; x++)
                {
                    Color32 c = other.Get(x, y);
                    if (c.a > 0) Blend(ox + x, oy + y, c);
                }
        }

        public Texture2D ToTexture()
        {
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            texture.SetPixels32(_pixels);
            texture.Apply();
            return texture;
        }
    }
}
