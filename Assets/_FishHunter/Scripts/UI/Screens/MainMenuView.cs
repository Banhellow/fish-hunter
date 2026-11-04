using FishHunter.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FishHunter.UI.Screens
{
    public sealed class MainMenuView : ViewBase<MainMenuViewModel>
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private PixelText _bestScoreText;

        protected override void OnBind(MainMenuViewModel viewModel)
        {
            BindCommand(_startButton, viewModel.StartGame);
            BindCommand(_exitButton, viewModel.Exit);
            BindProperty(viewModel.BestScore, best => _bestScoreText.Text = best > 0 ? $"BEST: {best}" : string.Empty);
        }
    }
}
