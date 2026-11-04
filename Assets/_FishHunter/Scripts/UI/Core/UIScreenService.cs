using System;
using System.Collections.Generic;
using FishHunter.Core;
using FishHunter.UI.Screens;
using UnityEngine;

namespace FishHunter.UI.Core
{
    /// <summary>
    /// Screen manager: creates ViewModels, binds them to their Views and shows the right
    /// screens for the current <see cref="GameState"/>.
    /// </summary>
    public sealed class UIScreenService : MonoBehaviour, IService, IInitializable
    {
        [SerializeField] private MainMenuView _mainMenuView;
        [SerializeField] private HudView _hudView;
        [SerializeField] private PauseView _pauseView;
        [SerializeField] private GameOverView _gameOverView;

        private readonly Dictionary<Type, ViewBase> _views = new();
        private GameFlowService _flow;

        public void Initialize()
        {
            _flow = ServiceLocator.Get<GameFlowService>();

            _mainMenuView.Bind(new MainMenuViewModel());
            _hudView.Bind(new HudViewModel());
            _pauseView.Bind(new PauseViewModel());
            _gameOverView.Bind(new GameOverViewModel());

            Register(_mainMenuView);
            Register(_hudView);
            Register(_pauseView);
            Register(_gameOverView);

            _flow.StateChanged += ApplyState;
            ApplyState(_flow.State);
        }

        private void OnDestroy()
        {
            if (_flow != null) _flow.StateChanged -= ApplyState;
        }

        private void Register(ViewBase view) => _views[view.GetType()] = view;

        private T Get<T>() where T : ViewBase => (T)_views[typeof(T)];

        private void Show<T>() where T : ViewBase => Get<T>().Show();

        private void HideAll()
        {
            foreach (ViewBase view in _views.Values) view.Hide();
        }

        private void ApplyState(GameState state)
        {
            HideAll();
            switch (state)
            {
                case GameState.MainMenu:
                    Show<MainMenuView>();
                    break;
                case GameState.Playing:
                    Show<HudView>();
                    break;
                case GameState.Paused:
                    Show<HudView>();
                    Show<PauseView>();
                    break;
                case GameState.GameOver:
                    Show<GameOverView>();
                    break;
            }
        }
    }
}
