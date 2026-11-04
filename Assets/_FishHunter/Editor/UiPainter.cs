using UnityEngine;

namespace FishHunter.EditorTools
{
    /// <summary>Pixel UI pieces: font atlas, hearts, 9-slice panel/buttons and the title logo.</summary>
    public static class UiPainter
    {
        public const int FontColumns = 16;
        public const int FontCellW = 6;
        public const int FontCellH = 8;

        public static PixelCanvas FontAtlas(string characters)
        {
            int rows = Mathf.CeilToInt(characters.Length / (float)FontColumns);
            int width = FontColumns * FontCellW;
            int height = rows * FontCellH;
            var c = new PixelCanvas(width, height);
            var ink = new Color32(255, 255, 255, 255);

            for (int i = 0; i < characters.Length; i++)
            {
                int col = i % FontColumns, row = i / FontColumns;
                for (int gy = 0; gy < PixelFontGlyphs.GlyphHeight; gy++)
                for (int gx = 0; gx < PixelFontGlyphs.GlyphWidth; gx++)
                {
                    if (!PixelFontGlyphs.Ink(characters[i], gx, gy)) continue;
                    int x = col * FontCellW + gx;
                    int y = height - 1 - (row * FontCellH + gy);
                    c.Set(x, y, ink);
                }
            }

            return c;
        }

        private static readonly string[] HeartShape =
        {
            ".##...##.",
            "####.####",
            "#########",
            "#########",
            ".#######.",
            "..#####..",
            "...###...",
            "....#....",
        };

        public static PixelCanvas Heart(bool full)
        {
            var c = new PixelCanvas(11, 10);
            Color32 fill = full ? new Color32(232, 52, 74, 255) : new Color32(72, 64, 88, 255);
            Color32 light = full ? new Color32(255, 160, 170, 255) : new Color32(105, 98, 120, 255);
            Color32 shade = full ? new Color32(168, 28, 52, 255) : new Color32(52, 46, 66, 255);
            Color32 outline = full ? new Color32(48, 10, 24, 255) : new Color32(26, 22, 36, 255);

            for (int row = 0; row < HeartShape.Length; row++)
            for (int x = 0; x < HeartShape[row].Length; x++)
            {
                if (HeartShape[row][x] != '#') continue;
                int px = x + 1, py = 8 - row;
                Color32 col = fill;
                if (row >= 4 && x >= 4) col = shade;
                c.Set(px, py, col);
            }

            c.Set(2, 7, light); c.Set(3, 7, light); c.Set(2, 6, light);
            c.Outline(outline);
            return c;
        }

        /// <summary>16x16 9-slice frame with 4px borders.</summary>
        public static PixelCanvas Frame(Color32 outline, Color32 rimLight, Color32 rimDark, Color32 fill, Color32 topShine)
        {
            const int s = 16;
            var c = new PixelCanvas(s, s);
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                bool corner = (x == 0 || x == s - 1) && (y == 0 || y == s - 1);
                if (corner) continue;
                bool edge = x == 0 || y == 0 || x == s - 1 || y == s - 1;
                bool innerCorner = (x == 1 || x == s - 2) && (y == 1 || y == s - 2);
                Color32 col;
                if (edge || innerCorner) col = outline;
                else if (y == s - 2 || x == 1) col = rimLight;
                else if (y == 1 || x == s - 2) col = rimDark;
                else if (y == s - 3) col = topShine;
                else col = fill;
                c.Set(x, y, col);
            }

            return c;
        }

        public static PixelCanvas Panel() => Frame(
            new Color32(10, 18, 42, 255), new Color32(96, 176, 214, 255), new Color32(30, 70, 120, 255),
            new Color32(18, 42, 82, 235), new Color32(26, 56, 102, 235));

        public static PixelCanvas Button(int state)
        {
            switch (state)
            {
                case 1: // hover
                    return Frame(new Color32(42, 20, 8, 255), new Color32(255, 240, 170, 255), new Color32(200, 120, 30, 255),
                        new Color32(255, 196, 70, 255), new Color32(255, 220, 120, 255));
                case 2: // pressed
                    return Frame(new Color32(42, 20, 8, 255), new Color32(170, 100, 20, 255), new Color32(255, 200, 100, 255),
                        new Color32(205, 135, 30, 255), new Color32(190, 120, 25, 255));
                default:
                    return Frame(new Color32(42, 20, 8, 255), new Color32(255, 222, 130, 255), new Color32(172, 98, 22, 255),
                        new Color32(236, 162, 42, 255), new Color32(248, 190, 80, 255));
            }
        }

        /// <summary>"FISH HUNTER" logo: gradient letters, outline, drop shadow, flanked by two fish.</summary>
        public static PixelCanvas Logo()
        {
            const string title = "FISH HUNTER";
            const int scale = 2;
            int textW = title.Length * (PixelFontGlyphs.GlyphWidth + 1) * scale - scale;
            int textH = PixelFontGlyphs.GlyphHeight * scale;

            var fishRight = FishPainter.Paint(FishPainter.Player, 0.15f);
            var fishLeft = fishRight.FlipX();
            int gap = 6;
            int width = fishRight.Width * 2 + gap * 2 + textW + 4;
            int height = Mathf.Max(fishRight.Height, textH + 8) + 2;

            var text = new PixelCanvas(width, height);
            int ox = fishRight.Width + gap + 2;
            int oy = (height - textH) / 2 + 1;
            Color32 topCol = new Color32(255, 236, 120, 255);
            Color32 bottomCol = new Color32(255, 128, 40, 255);

            for (int i = 0; i < title.Length; i++)
            for (int gy = 0; gy < PixelFontGlyphs.GlyphHeight; gy++)
            for (int gx = 0; gx < PixelFontGlyphs.GlyphWidth; gx++)
            {
                if (!PixelFontGlyphs.Ink(title[i], gx, gy)) continue;
                for (int sy = 0; sy < scale; sy++)
                for (int sx = 0; sx < scale; sx++)
                {
                    int x = ox + (i * (PixelFontGlyphs.GlyphWidth + 1) + gx) * scale + sx;
                    int yFromTop = gy * scale + sy;
                    int y = oy + textH - 1 - yFromTop;
                    float t = yFromTop / (float)(textH - 1);
                    Color32 col = Color32.Lerp(topCol, bottomCol, t);
                    if (yFromTop == 0) col = new Color32(255, 255, 220, 255);
                    text.Set(x, y, col);
                }
            }

            text.Outline(new Color32(52, 22, 8, 255), true);

            var logo = new PixelCanvas(width, height);
            // Drop shadow
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (text.IsOpaque(x, y)) logo.Set(x + 1, y - 2, new Color32(8, 24, 56, 200));
            logo.Stamp(text, 0, 0);
            logo.Stamp(fishRight, 0, (height - fishRight.Height) / 2);
            logo.Stamp(fishLeft, width - fishLeft.Width, (height - fishLeft.Height) / 2);
            return logo;
        }
    }
}
