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
        [SerializeField] private MainMenuView mainMenuView;
        [SerializeField] private HudView hudView;
        [SerializeField] private PauseView pauseView;
        [SerializeField] private GameOverView gameOverView;

        private readonly Dictionary<Type, ViewBase> _views = new Dictionary<Type, ViewBase>();
        private GameFlowService _flow;

        public void Initialize()
        {
            _flow = ServiceLocator.Get<GameFlowService>();

            mainMenuView.Bind(new MainMenuViewModel());
            hudView.Bind(new HudViewModel());
            pauseView.Bind(new PauseViewModel());
            gameOverView.Bind(new GameOverViewModel());

            Register(mainMenuView);
            Register(hudView);
            Register(pauseView);
            Register(gameOverView);

            _flow.StateChanged += ApplyState;
            ApplyState(_flow.State);
        }

        private void OnDestroy()
        {
            if (_flow != null) _flow.StateChanged -= ApplyState;
        }

        public void Register(ViewBase view) => _views[view.GetType()] = view;

        public T Get<T>() where T : ViewBase => (T)_views[typeof(T)];

        public void Show<T>() where T : ViewBase => Get<T>().Show();

        public void Hide<T>() where T : ViewBase => Get<T>().Hide();

        public void HideAll()
        {
            foreach (var view in _views.Values) view.Hide();
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
