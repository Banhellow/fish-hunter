using FishHunter.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FishHunter.UI.Screens
{
    /// <summary>Top-left HUD: score counter and three hearts (lost hearts are dimmed).</summary>
    public sealed class HudView : ViewBase<HudViewModel>
    {
        [SerializeField] private PixelText _scoreText;
        [SerializeField] private Image[] _hearts;
        [SerializeField] private Sprite _heartFull;
        [SerializeField] private Sprite _heartEmpty;
        [SerializeField] private Color _dimmedColor = new(1f, 1f, 1f, 0.55f);

        protected override void OnBind(HudViewModel viewModel)
        {
            BindProperty(viewModel.Score, score => _scoreText.Text = $"SCORE: {score}");
            BindProperty(viewModel.Lives, UpdateHearts);
        }

        private void UpdateHearts(int lives)
        {
            for (int i = 0; i < _hearts.Length; i++)
            {
                bool alive = i < lives;
                _hearts[i].sprite = alive ? _heartFull : _heartEmpty;
                _hearts[i].color = alive ? Color.white : _dimmedColor;
            }
        }
    }
}
