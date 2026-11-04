using FishHunter.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FishHunter.UI.Screens
{
    public sealed class MainMenuView : ViewBase<MainMenuViewModel>
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private PixelText bestScoreText;

        protected override void OnBind(MainMenuViewModel viewModel)
        {
            BindCommand(startButton, viewModel.StartGame);
            BindCommand(exitButton, viewModel.Exit);
            BindProperty(viewModel.BestScore, best => bestScoreText.Text = best > 0 ? $"BEST: {best}" : string.Empty);
        }
    }
}
