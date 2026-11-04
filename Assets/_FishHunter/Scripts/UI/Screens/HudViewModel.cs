using FishHunter.Core;
using FishHunter.Gameplay;
using FishHunter.UI.Core;

namespace FishHunter.UI.Screens
{
    public sealed class HudViewModel : ViewModelBase
    {
        public ObservableProperty<int> Score { get; } = new ObservableProperty<int>();
        public ObservableProperty<int> Lives { get; } = new ObservableProperty<int>();
        public int MaxLives { get; }

        public HudViewModel()
        {
            var score = ServiceLocator.Get<ScoreService>();
            var lives = ServiceLocator.Get<LivesService>();

            MaxLives = lives.MaxLives;
            Score.Value = score.Score;
            Lives.Value = lives.Lives;

            void OnScore(int value) => Score.Value = value;
            void OnLives(int value) => Lives.Value = value;

            score.ScoreChanged += OnScore;
            lives.LivesChanged += OnLives;
            AddCleanup(() => score.ScoreChanged -= OnScore);
            AddCleanup(() => lives.LivesChanged -= OnLives);
        }
    }
}
