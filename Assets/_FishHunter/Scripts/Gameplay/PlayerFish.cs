using FishHunter.Animation;
using FishHunter.Config;
using FishHunter.Core;
using FishHunter.Enemies;
using UnityEngine;

namespace FishHunter.Gameplay
{
    /// <summary>
    /// The player's fish, fixed at the center. Owns its visuals (idle/shoot frame animations, hit flash)
    /// and loses a life whenever an enemy fish touches it.
    /// </summary>
    public sealed class PlayerFish : MonoBehaviour, IService, IInitializable
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private SpriteFrameAnimator _animator;
        [SerializeField] private Color _hitColor = new(1f, 0.35f, 0.35f, 1f);

        private GameConfig _config;
        private GameFlowService _flow;
        private LivesService _lives;
        private float _flashTimer;

        public Vector2 Position => transform.position;
        public Vector2 Facing => _visual.right;
        public Vector2 NosePosition => (Vector2)transform.position + Facing * _config.NoseOffset;

        public void Initialize()
        {
            _config = ServiceLocator.Get<ConfigService>().Game;
            _flow = ServiceLocator.Get<GameFlowService>();
            _lives = ServiceLocator.Get<LivesService>();
            _flow.SessionReset += ResetFish;
            ResetFish();
        }

        private void OnDestroy()
        {
            if (_flow != null) _flow.SessionReset -= ResetFish;
        }

        public void SetFacing(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _visual.rotation = Quaternion.Euler(0f, 0f, angle);
            // Keep the fish upright when it faces left.
            _spriteRenderer.flipY = direction.x < 0f;
        }

        public void PlayShoot()
        {
            _animator.Play(_config.PlayerShootFrames, _config.PlayerShootFps, false, PlayIdle);
        }

        private void PlayIdle()
        {
            _animator.Play(_config.PlayerIdleFrames, _config.PlayerIdleFps, true);
        }

        private void ResetFish()
        {
            _flashTimer = 0f;
            _spriteRenderer.color = Color.white;
            SetFacing(Vector2.right);
            PlayIdle();
        }

        private void Update()
        {
            if (_flashTimer <= 0f) return;
            _flashTimer -= Time.deltaTime;
            bool flashOn = _flashTimer > 0f && Mathf.FloorToInt(_flashTimer * 16f) % 2 == 0;
            _spriteRenderer.color = flashOn ? _hitColor : Color.white;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_flow == null || _flow.State != GameState.Playing) return;
            if (!other.TryGetComponent(out Enemy enemy) || !enemy.IsAlive) return;

            enemy.Kill(false);
            _flashTimer = _config.HitFlashDuration;
            _lives.LoseLife();
        }
    }
}
