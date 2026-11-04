using System;
using FishHunter.Gameplay;
using UnityEngine;

namespace FishHunter.Core
{
    /// <summary>
    /// Game state machine: main menu, playing, paused, game over. Owns session resets and time scale.
    /// </summary>
    public sealed class GameFlowService : IService
    {
        private readonly ScoreService _score;
        private readonly LivesService _lives;
        private readonly DifficultyService _difficulty;
        private readonly PoolService _pool;
        private readonly InputService _input;

        public GameState State { get; private set; } = GameState.MainMenu;

        public event Action<GameState> StateChanged;
        /// <summary>Raised after score/lives/difficulty are reset and pooled objects are despawned.</summary>
        public event Action SessionReset;

        public GameFlowService(ScoreService score, LivesService lives, DifficultyService difficulty, PoolService pool, InputService input)
        {
            _score = score;
            _lives = lives;
            _difficulty = difficulty;
            _pool = pool;
            _input = input;
            _lives.Depleted += GameOver;
        }

        public void Tick(float deltaTime)
        {
            if (_input.PausePressed) TogglePause();
            if (State == GameState.Playing) _difficulty.Tick(deltaTime);
        }

        public void StartGame()
        {
            ResetSession();
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        private void Pause()
        {
            if (State != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        private void TogglePause()
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        private void GameOver()
        {
            if (State != GameState.Playing) return;
            _score.CommitBest();
            Time.timeScale = 0f;
            SetState(GameState.GameOver);
        }

        public void ReturnToMenu()
        {
            ResetSession();
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ResetSession()
        {
            _pool.DespawnAll();
            _score.ResetScore();
            _lives.ResetLives();
            _difficulty.ResetDifficulty();
            SessionReset?.Invoke();
        }

        private void SetState(GameState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
