using FishHunter.Core;
using FishHunter.Gameplay;
using FishHunter.UI.Core;

namespace FishHunter.UI.Screens
{
    public sealed class GameOverViewModel : ViewModelBase
    {
        private readonly GameFlowService _flow;
        private readonly ScoreService _score;

        public ObservableProperty<int> FinalScore { get; } = new ObservableProperty<int>();
        public ObservableProperty<int> BestScore { get; } = new ObservableProperty<int>();
        public ObservableProperty<bool> IsNewBest { get; } = new ObservableProperty<bool>();

        public GameOverViewModel()
        {
            _flow = ServiceLocator.Get<GameFlowService>();
            _score = ServiceLocator.Get<ScoreService>();
        }

        public override void OnShown()
        {
            FinalScore.Value = _score.Score;
            BestScore.Value = _score.Best;
            IsNewBest.Value = _score.IsNewBest;
        }

        public void PlayAgain() => _flow.StartGame();

        public void ReturnToMenu() => _flow.ReturnToMenu();
    }
}
