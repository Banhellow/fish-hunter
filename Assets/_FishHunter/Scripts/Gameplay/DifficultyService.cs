using FishHunter.Config;
using FishHunter.Core;
using FishHunter.Enemies;
using UnityEngine;

namespace FishHunter.Gameplay
{
    /// <summary>
    /// Linear difficulty ramp over play time: spawn rate, base enemy speed and the share of
    /// faster enemy types all grow linearly until <see cref="GameConfig.RampDuration"/>.
    /// </summary>
    public sealed class DifficultyService : IService
    {
        private readonly GameConfig _config;

        public float ElapsedTime { get; private set; }

        /// <summary>0 at session start, 1 when the ramp is complete.</summary>
        public float Progress => _config.RampDuration <= 0f ? 1f : Mathf.Clamp01(ElapsedTime / _config.RampDuration);

        public float SpawnInterval => Mathf.Lerp(_config.SpawnIntervalStart, _config.SpawnIntervalEnd, Progress);

        public float SpeedMultiplier => Mathf.Lerp(_config.SpeedMultiplierStart, _config.SpeedMultiplierEnd, Progress);

        public DifficultyService(GameConfig config)
        {
            _config = config;
        }

        public float GetSpawnWeight(EnemyTypeConfig type) => Mathf.Max(0f, Mathf.Lerp(type.SpawnWeightStart, type.SpawnWeightEnd, Progress));

        public void Tick(float deltaTime) => ElapsedTime += deltaTime;

        public void ResetDifficulty() => ElapsedTime = 0f;
    }
}
