using System.Collections.Generic;
using System.IO;
using FishHunter.UI.Core;
using UnityEditor;
using UnityEngine;

namespace FishHunter.EditorTools
{
    /// <summary>
    /// Generates every sprite in the game procedurally and imports them as point-filtered pixel art.
    /// Menu: Fish Hunter/Generate Art.
    /// </summary>
    public static class PixelArtGenerator
    {
        public const string Root = "Assets/_FishHunter/Art/Generated";
        public const string FontAssetPath = "Assets/_FishHunter/Data/PixelFont.asset";
        public const int WorldPpu = 16;
        /// <summary>UI sprites at PPU 20 render 5x on a canvas with referencePixelsPerUnit 100.</summary>
        public const int UiPpu = 20;

        private struct ImportJob
        {
            public string path;
            public int ppu;
            public Vector4 border;
            public bool isFont;
        }

        private static readonly List<ImportJob> Jobs = new List<ImportJob>();

        [MenuItem("Fish Hunter/Generate Art", priority = 0)]
        public static void Generate()
        {
            Jobs.Clear();

            // Player
            var player = FishPainter.Player;
            for (int f = 0; f < 4; f++)
                Save(FishPainter.Paint(player, f / 4f), "Player", $"Player_Idle_{f}", WorldPpu);
            float[] mouth = { 0.6f, 1f, 0.7f, 0.25f };
            int[] recoil = { -1, -2, -1, 0 };
            for (int f = 0; f < 4; f++)
                Save(FishPainter.Paint(player, f / 4f, mouth[f], recoil[f]), "Player", $"Player_Shoot_{f}", WorldPpu);

            // Enemies
            var styles = new Dictionary<string, FishStyle>
            {
                { "EnemyA", FishPainter.EnemyA },
                { "EnemyB", FishPainter.EnemyB },
                { "EnemyC", FishPainter.EnemyC }
            };
            foreach (var pair in styles)
            {
                for (int f = 0; f < 4; f++)
                    Save(FishPainter.Paint(pair.Value, f / 4f), "Enemies", $"{pair.Key}_Swim_{f}", WorldPpu);
                var death = EffectPainter.Death(pair.Value);
                for (int f = 0; f < death.Length; f++)
                    Save(death[f], "Effects", $"{pair.Key}_Death_{f}", WorldPpu);
            }

            // Projectile + muzzle
            var projectile = EffectPainter.Projectile();
            for (int f = 0; f < projectile.Length; f++) Save(projectile[f], "Effects", $"Projectile_{f}", WorldPpu);
            var muzzle = EffectPainter.Muzzle();
            for (int f = 0; f < muzzle.Length; f++) Save(muzzle[f], "Effects", $"Muzzle_{f}", WorldPpu);

            // Background
            Save(BackgroundPainter.Seabed(), "Background", "Seabed", WorldPpu);
            const int causticFrames = 6;
            for (int f = 0; f < causticFrames; f++)
                Save(BackgroundPainter.Caustics(f, causticFrames), "Background", $"Caustics_{f}", WorldPpu);

            // UI
            Save(UiPainter.Heart(true), "UI", "Heart_Full", UiPpu);
            Save(UiPainter.Heart(false), "UI", "Heart_Empty", UiPpu);
            var border = new Vector4(4, 4, 4, 4);
            Save(UiPainter.Panel(), "UI", "Panel", UiPpu, border);
            Save(UiPainter.Button(0), "UI", "Button_Normal", UiPpu, border);
            Save(UiPainter.Button(1), "UI", "Button_Hover", UiPpu, border);
            Save(UiPainter.Button(2), "UI", "Button_Pressed", UiPpu, border);
            Save(UiPainter.Logo(), "UI", "Logo", UiPpu);

            // Font
            string characters = PixelFontGlyphs.Characters;
            Save(UiPainter.FontAtlas(characters), "Font", "PixelFont", UiPpu, default, true);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var job in Jobs) ConfigureImporter(job);

            CreateFontAsset(characters);
            AssetDatabase.SaveAssets();
            Debug.Log($"[FishHunter] Generated {Jobs.Count} pixel-art textures.");
        }

        private static void Save(PixelCanvas canvas, string folder, string name, int ppu, Vector4 border = default, bool isFont = false)
        {
            string dir = $"{Root}/{folder}";
            Directory.CreateDirectory(dir);
            string path = $"{dir}/{name}.png";
            Texture2D texture = canvas.ToTexture();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            Jobs.Add(new ImportJob { path = path, ppu = ppu, border = border, isFont = isFont });
        }

        private static void ConfigureImporter(ImportJob job)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(job.path);
            if (importer == null)
            {
                Debug.LogError($"[FishHunter] Missing importer for {job.path}");
                return;
            }

            importer.textureType = job.isFont ? TextureImporterType.Default : TextureImporterType.Sprite;
            if (!job.isFont)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = job.ppu;
                importer.spriteBorder = job.border;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteAlignment = (int)SpriteAlignment.Center;
                importer.SetTextureSettings(settings);
            }

            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static void CreateFontAsset(string characters)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{Root}/Font/PixelFont.png");
            Directory.CreateDirectory(Path.GetDirectoryName(FontAssetPath));
            var font = AssetDatabase.LoadAssetAtPath<PixelFont>(FontAssetPath);
            if (font == null)
            {
                font = ScriptableObject.CreateInstance<PixelFont>();
                AssetDatabase.CreateAsset(font, FontAssetPath);
            }

            font.texture = texture;
            font.characters = characters;
            font.columns = UiPainter.FontColumns;
            font.cellWidth = UiPainter.FontCellW;
            font.cellHeight = UiPainter.FontCellH;
            font.glyphWidth = PixelFontGlyphs.GlyphWidth;
            font.glyphHeight = PixelFontGlyphs.GlyphHeight;
            font.letterSpacing = 1;
            EditorUtility.SetDirty(font);
        }

        // ---------- Loading helpers for the scene builder ----------

        public static Sprite LoadSprite(string folder, string name)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Root}/{folder}/{name}.png");
            if (sprite == null) throw new FileNotFoundException($"Sprite {folder}/{name} not found. Run Fish Hunter/Generate Art first.");
            return sprite;
        }

        public static Sprite[] LoadFrames(string folder, string prefix, int count)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++) frames[i] = LoadSprite(folder, $"{prefix}_{i}");
            return frames;
        }
    }
}
