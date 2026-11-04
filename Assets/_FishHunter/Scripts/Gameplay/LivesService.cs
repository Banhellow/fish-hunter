using System;
using FishHunter.Core;

namespace FishHunter.Gameplay
{
    public sealed class LivesService : IService
    {
        public int MaxLives { get; }
        public int Lives { get; private set; }

        public event Action<int> LivesChanged;
        public event Action Depleted;

        public LivesService(int maxLives)
        {
            MaxLives = maxLives;
            Lives = maxLives;
        }

        public void LoseLife()
        {
            if (Lives <= 0) return;
            Lives--;
            LivesChanged?.Invoke(Lives);
            if (Lives == 0) Depleted?.Invoke();
        }

        public void ResetLives()
        {
            Lives = MaxLives;
            LivesChanged?.Invoke(Lives);
        }
    }
}
