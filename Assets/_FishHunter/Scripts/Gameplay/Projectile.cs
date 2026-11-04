using FishHunter.Core;
using FishHunter.Enemies;
using UnityEngine;

namespace FishHunter.Gameplay
{
    /// <summary>Bubble shot: flies in a straight line at a fixed speed and pops the first enemy it touches.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        private const float OffscreenMargin = 1.5f;

        [SerializeField] private Rigidbody2D _body;
        [SerializeField] private TrailRenderer _trail;

        private PoolService _pool;
        private CameraService _camera;
        private float _lifetime;
        private bool _active;

        private void Awake()
        {
            if (_body == null) _body = GetComponent<Rigidbody2D>();
        }

        public void Launch(Vector2 direction, float speed, float lifetime)
        {
            _pool ??= ServiceLocator.Get<PoolService>();
            _camera ??= ServiceLocator.Get<CameraService>();

            _lifetime = lifetime;
            _active = true;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            _body.position = transform.position;
            _body.linearVelocity = direction.normalized * speed;
            if (_trail != null) _trail.Clear();
        }

        private void Update()
        {
            if (!_active) return;
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f || _camera.IsOutside(transform.position, OffscreenMargin)) Despawn();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_active) return;
            if (!other.TryGetComponent(out Enemy enemy) || !enemy.IsAlive) return;

            enemy.Kill(true);
            Despawn();
        }

        private void Despawn()
        {
            _active = false;
            _pool.Despawn(this);
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            _active = false;
            _body.linearVelocity = Vector2.zero;
            if (_trail != null) _trail.Clear();
        }
    }
}
