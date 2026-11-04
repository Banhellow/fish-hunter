using FishHunter.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FishHunter.UI.Screens
{
    /// <summary>Top-left HUD: score counter and three hearts (lost hearts are dimmed).</summary>
    public sealed class HudView : ViewBase<HudViewModel>
    {
        [SerializeField] private PixelText scoreText;
        [SerializeField] private Image[] hearts;
        [SerializeField] private Sprite heartFull;
        [SerializeField] private Sprite heartEmpty;
        [SerializeField] private Color dimmedColor = new Color(1f, 1f, 1f, 0.55f);

        protected override void OnBind(HudViewModel viewModel)
        {
            BindProperty(viewModel.Score, score => scoreText.Text = $"SCORE: {score}");
            BindProperty(viewModel.Lives, UpdateHearts);
        }

        private void UpdateHearts(int lives)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                bool alive = i < lives;
                hearts[i].sprite = alive ? heartFull : heartEmpty;
                hearts[i].color = alive ? Color.white : dimmedColor;
            }
        }
    }
}
