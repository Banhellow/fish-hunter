using System;
using System.Collections.Generic;
using FishHunter.Enemies.Strategies;
using FishHunter.Gameplay;
using UnityEngine;

namespace FishHunter.Enemies
{
    /// <summary>Creates pooled enemies and resolves the movement strategy for each type.</summary>
    public sealed class EnemyFactory
    {
        private readonly Enemy _prefab;
        private readonly PoolService _pool;
        private readonly Dictionary<EnemyMovementKind, IEnemyMovementStrategy> _strategies;

        public EnemyFactory(Enemy prefab, PoolService pool)
        {
            _prefab = prefab;
            _pool = pool;
            _strategies = new Dictionary<EnemyMovementKind, IEnemyMovementStrategy>
            {
                { EnemyMovementKind.Standard, new StandardMovementStrategy() },
                { EnemyMovementKind.Fast, new FastMovementStrategy() },
                { EnemyMovementKind.Swift, new SwiftMovementStrategy() }
            };
        }

        public float RollSpeed(EnemyTypeConfig type, float speedMultiplier) =>
            UnityEngine.Random.Range(type.speedMin, type.speedMax) * speedMultiplier;

        public IEnemyMovementStrategy GetStrategy(EnemyMovementKind kind) => _strategies[kind];

        public Enemy Create(EnemyTypeConfig type, Vector2 position, float speed, Vector2 target, Action<Enemy, bool> onRemoved)
        {
            Enemy enemy = _pool.Spawn(_prefab, position, Quaternion.identity);
            enemy.Init(type, speed, GetStrategy(type.movement), target, onRemoved);
            return enemy;
        }
    }
}
