using FishHunter.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FishHunter.UI.Screens
{
    public sealed class GameOverView : ViewBase<GameOverViewModel>
    {
        [SerializeField] private PixelText _finalScoreText;
        [SerializeField] private PixelText _bestScoreText;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _menuButton;

        protected override void OnBind(GameOverViewModel viewModel)
        {
            BindCommand(_playAgainButton, viewModel.PlayAgain);
            BindCommand(_menuButton, viewModel.ReturnToMenu);
            BindProperty(viewModel.FinalScore, score => _finalScoreText.Text = $"SCORE: {score}");
            BindProperty(viewModel.BestScore, _ => RefreshBest());
            BindProperty(viewModel.IsNewBest, _ => RefreshBest());
        }

        private void RefreshBest()
        {
            _bestScoreText.Text = ViewModel.IsNewBest.Value ? "NEW BEST!" : $"BEST: {ViewModel.BestScore.Value}";
        }
    }
}
