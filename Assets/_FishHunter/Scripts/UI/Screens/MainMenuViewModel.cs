using FishHunter.Core;
using FishHunter.Gameplay;
using FishHunter.UI.Core;

namespace FishHunter.UI.Screens
{
    public sealed class MainMenuViewModel : ViewModelBase
    {
        private readonly GameFlowService _flow;
        private readonly ScoreService _score;

        public ObservableProperty<int> BestScore { get; } = new ObservableProperty<int>();

        public MainMenuViewModel()
        {
            _flow = ServiceLocator.Get<GameFlowService>();
            _score = ServiceLocator.Get<ScoreService>();
            BestScore.Value = _score.Best;
            _score.BestChanged += OnBestChanged;
            AddCleanup(() => _score.BestChanged -= OnBestChanged);
        }

        private void OnBestChanged(int best) => BestScore.Value = best;

        public void StartGame() => _flow.StartGame();

        public void Exit() => _flow.Quit();
    }
}
