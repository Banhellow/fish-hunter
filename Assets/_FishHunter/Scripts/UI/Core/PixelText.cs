using UnityEngine;
using UnityEngine.UI;

namespace FishHunter.UI.Core
{
    /// <summary>
    /// uGUI graphic that renders a string with a <see cref="PixelFont"/> at an integer pixel scale,
    /// with an optional hard drop shadow, so all UI text stays crisp pixel art.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PixelText : MaskableGraphic
    {
        public enum Align
        {
            Left,
            Center,
            Right
        }

        [SerializeField] private PixelFont font;
        [SerializeField, TextArea] private string text = "TEXT";
        [SerializeField, Min(1)] private int pixelScale = 4;
        [SerializeField] private Align alignment = Align.Left;
        [SerializeField] private bool shadow = true;
        [SerializeField] private Color shadowColor = new Color(0.04f, 0.08f, 0.18f, 0.9f);

        public override Texture mainTexture => font != null && font.texture != null ? font.texture : s_WhiteTexture;

        public PixelFont Font
        {
            get => font;
            set { font = value; SetAllDirty(); }
        }

        public string Text
        {
            get => text;
            set
            {
                if (text == value) return;
                text = value;
                SetVerticesDirty();
            }
        }

        public int PixelScale
        {
            get => pixelScale;
            set { pixelScale = Mathf.Max(1, value); SetVerticesDirty(); }
        }

        public Align Alignment
        {
            get => alignment;
            set { alignment = value; SetVerticesDirty(); }
        }

        public bool Shadow
        {
            get => shadow;
            set { shadow = value; SetVerticesDirty(); }
        }

        /// <summary>Size of the rendered string in canvas units.</summary>
        public Vector2 PreferredSize => font == null
            ? Vector2.zero
            : new Vector2(font.MeasureWidth(text) * pixelScale, font.glyphHeight * pixelScale);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (font == null || font.texture == null || string.IsNullOrEmpty(text)) return;

            Rect rect = rectTransform.rect;
            float width = font.MeasureWidth(text) * pixelScale;
            float height = font.glyphHeight * pixelScale;

            float x = alignment switch
            {
                Align.Center => rect.center.x - width * 0.5f,
                Align.Right => rect.xMax - width,
                _ => rect.xMin
            };
            float y = rect.center.y - height * 0.5f;
            x = Mathf.Round(x);
            y = Mathf.Round(y);

            if (shadow) AddString(vh, x + pixelScale, y - pixelScale, shadowColor * new Color(1f, 1f, 1f, color.a));
            AddString(vh, x, y, color);
        }

        private void AddString(VertexHelper vh, float startX, float startY, Color32 tint)
        {
            float advance = (font.glyphWidth + font.letterSpacing) * pixelScale;
            float glyphW = font.glyphWidth * pixelScale;
            float glyphH = font.glyphHeight * pixelScale;
            float x = startX;

            foreach (char c in text)
            {
                int index = font.IndexOf(c);
                if (index >= 0 && c != ' ')
                {
                    Rect uv = font.GetUv(index);
                    int vi = vh.currentVertCount;
                    vh.AddVert(new Vector3(x, startY), tint, new Vector2(uv.xMin, uv.yMin));
                    vh.AddVert(new Vector3(x, startY + glyphH), tint, new Vector2(uv.xMin, uv.yMax));
                    vh.AddVert(new Vector3(x + glyphW, startY + glyphH), tint, new Vector2(uv.xMax, uv.yMax));
                    vh.AddVert(new Vector3(x + glyphW, startY), tint, new Vector2(uv.xMax, uv.yMin));
                    vh.AddTriangle(vi, vi + 1, vi + 2);
                    vh.AddTriangle(vi + 2, vi + 3, vi);
                }

                x += advance;
            }
        }
    }
}
