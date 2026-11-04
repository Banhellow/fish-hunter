using FishHunter.Config;
using FishHunter.Enemies;
using FishHunter.Gameplay;
using FishHunter.UI.Core;
using UnityEngine;

namespace FishHunter.Core
{
    /// <summary>
    /// Composition root: registers every service in the <see cref="ServiceLocator"/>, initializes
    /// scene services in a known order and drives the game-flow tick.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private Camera _gameCamera;
        [SerializeField] private Transform _poolRoot;
        [SerializeField] private PlayerFish _player;
        [SerializeField] private EnemySpawner _spawner;
        [SerializeField] private UIScreenService _screens;

        private GameFlowService _flow;

        private void Awake()
        {
            ServiceLocator.Clear();
            Time.timeScale = 1f;

            var input = new InputService();
            var score = new ScoreService();
            var lives = new LivesService(_gameConfig.StartLives);
            var difficulty = new DifficultyService(_gameConfig);
            var pool = new PoolService(_poolRoot != null ? _poolRoot : transform);
            _flow = new GameFlowService(score, lives, difficulty, pool, input);

            ServiceLocator.Register(new ConfigService(_gameConfig));
            ServiceLocator.Register(new CameraService(_gameCamera != null ? _gameCamera : Camera.main));
            ServiceLocator.Register(input);
            ServiceLocator.Register(score);
            ServiceLocator.Register(lives);
            ServiceLocator.Register(difficulty);
            ServiceLocator.Register(pool);
            ServiceLocator.Register(_flow);
            ServiceLocator.Register(_player);
            ServiceLocator.Register(_spawner);
            ServiceLocator.Register(_screens);

            _player.Initialize();
            _spawner.Initialize();
            _screens.Initialize();

            _flow.ReturnToMenu();
        }

        private void Update()
        {
            _flow.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            ServiceLocator.Clear();
            Time.timeScale = 1f;
        }
    }
}
