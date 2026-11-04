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
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Transform poolRoot;
        [SerializeField] private PlayerFish player;
        [SerializeField] private EnemySpawner spawner;
        [SerializeField] private UIScreenService screens;

        private GameFlowService _flow;

        private void Awake()
        {
            ServiceLocator.Clear();
            Time.timeScale = 1f;

            var input = new InputService();
            var score = new ScoreService();
            var lives = new LivesService(gameConfig.startLives);
            var difficulty = new DifficultyService(gameConfig);
            var pool = new PoolService(poolRoot != null ? poolRoot : transform);
            _flow = new GameFlowService(score, lives, difficulty, pool, input);

            ServiceLocator.Register(new ConfigService(gameConfig));
            ServiceLocator.Register(new CameraService(gameCamera != null ? gameCamera : Camera.main));
            ServiceLocator.Register(input);
            ServiceLocator.Register(score);
            ServiceLocator.Register(lives);
            ServiceLocator.Register(difficulty);
            ServiceLocator.Register(pool);
            ServiceLocator.Register(_flow);
            ServiceLocator.Register(player);
            ServiceLocator.Register(spawner);
            ServiceLocator.Register(screens);

            player.Initialize();
            spawner.Initialize();
            screens.Initialize();

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
