using System.Collections.Generic;
using FishHunter.Animation;
using FishHunter.Config;
using FishHunter.Core;
using FishHunter.Gameplay;
using UnityEngine;

namespace FishHunter.Enemies
{
    /// <summary>
    /// Spawns enemies just outside the screen on all four sides, with timing from
    /// <see cref="DifficultyService"/>. Spawns are spaced in time and space so fish never start overlapping,
    /// and a faster fish is never placed in the same lane behind a slower one (it would overtake through it).
    /// </summary>
    public sealed class EnemySpawner : MonoBehaviour, IService, IInitializable
    {
        [SerializeField] private Enemy _enemyPrefab;
        [SerializeField] private OneShotEffect _deathEffectPrefab;
        [SerializeField] private EnemyTypeConfig[] _enemyTypes;

        private readonly List<Enemy> _active = new();
        private GameConfig _config;
        private DifficultyService _difficulty;
        private GameFlowService _flow;
        private ScoreService _score;
        private CameraService _camera;
        private PoolService _pool;
        private EnemyFactory _factory;
        private float _spawnTimer;

        public IReadOnlyList<Enemy> ActiveEnemies => _active;
        public IReadOnlyList<EnemyTypeConfig> EnemyTypes => _enemyTypes;

        public void Initialize()
        {
            _config = ServiceLocator.Get<ConfigService>().Game;
            _difficulty = ServiceLocator.Get<DifficultyService>();
            _flow = ServiceLocator.Get<GameFlowService>();
            _score = ServiceLocator.Get<ScoreService>();
            _camera = ServiceLocator.Get<CameraService>();
            _pool = ServiceLocator.Get<PoolService>();
            _factory = new EnemyFactory(_enemyPrefab, _pool);
            _flow.SessionReset += OnSessionReset;
            OnSessionReset();
        }

        private void OnDestroy()
        {
            if (_flow != null) _flow.SessionReset -= OnSessionReset;
        }

        private void OnSessionReset()
        {
            _active.Clear();
            _spawnTimer = 0.6f;
        }

        private void Update()
        {
            if (_flow == null || _flow.State != GameState.Playing) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;

            if (TrySpawn())
            {
                float jitter = 1f + Random.Range(-_config.SpawnIntervalJitter, _config.SpawnIntervalJitter);
                _spawnTimer = Mathf.Max(_config.MinSpawnGap, _difficulty.SpawnInterval * jitter);
            }
            else
            {
                // No free spot this frame: retry shortly instead of stacking fish.
                _spawnTimer = 0.1f;
            }
        }

        private bool TrySpawn()
        {
            EnemyTypeConfig type = PickType();
            if (type == null) return false;

            float speed = _factory.RollSpeed(type, _difficulty.SpeedMultiplier);
            if (!TryFindSpawnPoint(speed, type.ColliderRadius, out Vector2 position)) return false;

            Enemy enemy = _factory.Create(type, position, speed, Vector2.zero, OnEnemyRemoved);
            _active.Add(enemy);
            return true;
        }

        private EnemyTypeConfig PickType()
        {
            float total = 0f;
            foreach (EnemyTypeConfig type in _enemyTypes) total += _difficulty.GetSpawnWeight(type);
            if (total <= 0f) return null;

            float roll = Random.value * total;
            foreach (EnemyTypeConfig type in _enemyTypes)
            {
                roll -= _difficulty.GetSpawnWeight(type);
                if (roll <= 0f) return type;
            }

            return _enemyTypes[_enemyTypes.Length - 1];
        }

        private bool TryFindSpawnPoint(float speed, float radius, out Vector2 position)
        {
            Vector2 half = _camera.HalfExtents;
            float offset = _config.SpawnMargin + radius;

            for (int attempt = 0; attempt < _config.SpawnAttempts; attempt++)
            {
                int side = Random.Range(0, 4);
                position = side switch
                {
                    0 => new Vector2(Random.Range(-half.x, half.x), half.y + offset),
                    1 => new Vector2(Random.Range(-half.x, half.x), -half.y - offset),
                    2 => new Vector2(-half.x - offset, Random.Range(-half.y, half.y)),
                    _ => new Vector2(half.x + offset, Random.Range(-half.y, half.y)),
                };
                if (IsFree(position, speed)) return true;
            }

            position = default;
            return false;
        }

        private bool IsFree(Vector2 candidate, float speed)
        {
            float minSpacingSqr = _config.MinSpawnSpacing * _config.MinSpawnSpacing;
            float candidateAngle = Mathf.Atan2(candidate.y, candidate.x) * Mathf.Rad2Deg;

            foreach (Enemy enemy in _active)
            {
                if (!enemy.IsAlive) continue;
                Vector2 other = enemy.Position;
                if ((other - candidate).sqrMagnitude < minSpacingSqr) return false;

                // Same approach lane: whichever fish is further out must not be faster than the one ahead,
                // otherwise it would catch up and swim through it.
                float otherAngle = Mathf.Atan2(other.y, other.x) * Mathf.Rad2Deg;
                if (Mathf.Abs(Mathf.DeltaAngle(candidateAngle, otherAngle)) >= _config.LaneAngle) continue;

                bool candidateIsOuter = candidate.sqrMagnitude > other.sqrMagnitude;
                float outerSpeed = candidateIsOuter ? speed : enemy.Speed;
                float innerSpeed = candidateIsOuter ? enemy.Speed : speed;
                if (outerSpeed > innerSpeed) return false;
            }

            return true;
        }

        private void OnEnemyRemoved(Enemy enemy, bool killedByPlayer)
        {
            _active.Remove(enemy);
            if (killedByPlayer) _score.Add(enemy.Type.Points);

            if (_deathEffectPrefab != null && enemy.Type.DeathFrames != null && enemy.Type.DeathFrames.Length > 0)
            {
                OneShotEffect effect = _pool.Spawn(_deathEffectPrefab, enemy.Position, Quaternion.identity);
                effect.Play(enemy.Type.DeathFrames, enemy.Type.DeathFps, Color.white);
            }

            _pool.Despawn(enemy);
        }
    }
}
