using System;
using FishHunter.Animation;
using FishHunter.Enemies.Strategies;
using FishHunter.Gameplay;
using UnityEngine;

namespace FishHunter.Enemies
{
    /// <summary>
    /// Enemy fish actor. Movement is delegated to an <see cref="IEnemyMovementStrategy"/>;
    /// the enemy only holds per-individual state and reports when it is removed.
    /// </summary>
    public sealed class Enemy : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteFrameAnimator _animator;
        [SerializeField] private CircleCollider2D _bodyCollider;

        private IEnemyMovementStrategy _strategy;
        private Action<Enemy, bool> _onRemoved;

        public EnemyTypeConfig Type { get; private set; }
        public float Speed { get; private set; }
        public float Age { get; private set; }
        public float Phase { get; private set; }
        public Vector2 SpawnPosition { get; private set; }
        public Vector2 Target { get; private set; }
        /// <summary>Point on the straight spawn-to-target line; strategies may offset the visible position from it.</summary>
        public Vector2 PathPosition { get; set; }
        public bool IsAlive { get; private set; }
        public Vector2 Position => transform.position;

        public void Init(EnemyTypeConfig type, float speed, IEnemyMovementStrategy strategy, Vector2 target, Action<Enemy, bool> onRemoved)
        {
            Type = type;
            Speed = speed;
            _strategy = strategy;
            _onRemoved = onRemoved;
            Target = target;
            SpawnPosition = transform.position;
            PathPosition = SpawnPosition;
            Age = 0f;
            Phase = UnityEngine.Random.value;
            IsAlive = true;

            _bodyCollider.radius = type.ColliderRadius;
            _spriteRenderer.color = Color.white;
            Face(target - SpawnPosition);
            _animator.Play(type.SwimFrames, type.SwimFps * Mathf.Lerp(0.9f, 1.3f, Mathf.InverseLerp(type.SpeedMin, type.SpeedMax * 2f, speed)), true, null, true);
        }

        private void Update()
        {
            if (!IsAlive) return;
            Age += Time.deltaTime;
            _strategy.Tick(this, Time.deltaTime);
        }

        /// <summary>Called by strategies with the new visible position; also orients the sprite.</summary>
        public void ApplyPosition(Vector2 position)
        {
            Vector2 delta = position - (Vector2)transform.position;
            transform.position = position;
            if (delta.sqrMagnitude > 1e-8f) Face(delta);
        }

        /// <param name="killedByPlayer">True when destroyed by a projectile (awards points).</param>
        public void Kill(bool killedByPlayer)
        {
            if (!IsAlive) return;
            IsAlive = false;
            Action<Enemy, bool> callback = _onRemoved;
            _onRemoved = null;
            callback?.Invoke(this, killedByPlayer);
        }

        private void Face(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            _spriteRenderer.flipY = direction.x < 0f;
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            IsAlive = false;
            _onRemoved = null;
            _strategy = null;
        }
    }
}
