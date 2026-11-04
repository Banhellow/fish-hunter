using FishHunter.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FishHunter.UI.Screens
{
    public sealed class GameOverView : ViewBase<GameOverViewModel>
    {
        [SerializeField] private PixelText finalScoreText;
        [SerializeField] private PixelText bestScoreText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button menuButton;

        protected override void OnBind(GameOverViewModel viewModel)
        {
            BindCommand(playAgainButton, viewModel.PlayAgain);
            BindCommand(menuButton, viewModel.ReturnToMenu);
            BindProperty(viewModel.FinalScore, score => finalScoreText.Text = $"SCORE: {score}");
            BindProperty(viewModel.BestScore, _ => RefreshBest());
            BindProperty(viewModel.IsNewBest, _ => RefreshBest());
        }

        private void RefreshBest()
        {
            bestScoreText.Text = ViewModel.IsNewBest.Value ? "NEW BEST!" : $"BEST: {ViewModel.BestScore.Value}";
        }
    }
}
