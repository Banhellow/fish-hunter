using System;
using FishHunter.Core;
using UnityEngine;

namespace FishHunter.Gameplay
{
    public sealed class ScoreService : IService
    {
        private const string BestKey = "FishHunter.BestScore";

        public int Score { get; private set; }
        public int Best { get; private set; }
        public bool IsNewBest { get; private set; }

        public event Action<int> ScoreChanged;
        public event Action<int> BestChanged;

        public ScoreService()
        {
            Best = PlayerPrefs.GetInt(BestKey, 0);
        }

        public void Add(int points)
        {
            if (points <= 0) return;
            Score += points;
            ScoreChanged?.Invoke(Score);
        }

        public void ResetScore()
        {
            Score = 0;
            IsNewBest = false;
            ScoreChanged?.Invoke(Score);
        }

        public void CommitBest()
        {
            if (Score <= Best) return;
            Best = Score;
            IsNewBest = true;
            PlayerPrefs.SetInt(BestKey, Best);
            PlayerPrefs.Save();
            BestChanged?.Invoke(Best);
        }
    }
}
