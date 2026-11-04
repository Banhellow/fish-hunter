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

        [SerializeField] private PixelFont _font;
        [SerializeField, TextArea] private string _text = "TEXT";
        [SerializeField, Min(1)] private int _pixelScale = 4;
        [SerializeField] private Align _alignment = Align.Left;
        [SerializeField] private bool _shadow = true;
        [SerializeField] private Color _shadowColor = new(0.04f, 0.08f, 0.18f, 0.9f);

        public override Texture mainTexture => _font != null && _font.Texture != null ? _font.Texture : s_WhiteTexture;

        public PixelFont Font
        {
            get => _font;
            set { _font = value; SetAllDirty(); }
        }

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                SetVerticesDirty();
            }
        }

        public int PixelScale
        {
            get => _pixelScale;
            set { _pixelScale = Mathf.Max(1, value); SetVerticesDirty(); }
        }

        public Align Alignment
        {
            get => _alignment;
            set { _alignment = value; SetVerticesDirty(); }
        }

        public bool Shadow
        {
            get => _shadow;
            set { _shadow = value; SetVerticesDirty(); }
        }

        /// <summary>Size of the rendered string in canvas units.</summary>
        public Vector2 PreferredSize => _font == null
            ? Vector2.zero
            : new Vector2(_font.MeasureWidth(_text) * _pixelScale, _font.GlyphHeight * _pixelScale);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_font == null || _font.Texture == null || string.IsNullOrEmpty(_text)) return;

            Rect rect = rectTransform.rect;
            float width = _font.MeasureWidth(_text) * _pixelScale;
            float height = _font.GlyphHeight * _pixelScale;

            float x = _alignment switch
            {
                Align.Center => rect.center.x - width * 0.5f,
                Align.Right => rect.xMax - width,
                _ => rect.xMin
            };
            float y = rect.center.y - height * 0.5f;
            x = Mathf.Round(x);
            y = Mathf.Round(y);

            if (_shadow) AddString(vh, x + _pixelScale, y - _pixelScale, _shadowColor * new Color(1f, 1f, 1f, color.a));
            AddString(vh, x, y, color);
        }

        private void AddString(VertexHelper vh, float startX, float startY, Color32 tint)
        {
            float advance = (_font.GlyphWidth + _font.LetterSpacing) * _pixelScale;
            float glyphW = _font.GlyphWidth * _pixelScale;
            float glyphH = _font.GlyphHeight * _pixelScale;
            float x = startX;

            foreach (char c in _text)
            {
                int index = _font.IndexOf(c);
                if (index >= 0 && c != ' ')
                {
                    Rect uv = _font.GetUv(index);
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
