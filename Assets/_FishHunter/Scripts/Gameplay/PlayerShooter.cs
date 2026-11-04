using FishHunter.Animation;
using FishHunter.Config;
using FishHunter.Core;
using UnityEngine;

namespace FishHunter.Gameplay
{
    /// <summary>Right mouse button fires projectiles from the fish's nose toward the cursor.</summary>
    [RequireComponent(typeof(PlayerAim))]
    public sealed class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private PlayerFish _player;
        [SerializeField] private PlayerAim _aim;
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private OneShotEffect _muzzleEffectPrefab;

        private GameConfig _config;
        private InputService _input;
        private GameFlowService _flow;
        private PoolService _pool;
        private float _cooldown;

        private void Start()
        {
            if (_player == null) _player = GetComponent<PlayerFish>();
            if (_aim == null) _aim = GetComponent<PlayerAim>();
            _config = ServiceLocator.Get<ConfigService>().Game;
            _input = ServiceLocator.Get<InputService>();
            _flow = ServiceLocator.Get<GameFlowService>();
            _pool = ServiceLocator.Get<PoolService>();
        }

        private void Update()
        {
            if (_cooldown > 0f) _cooldown -= Time.deltaTime;
            if (_flow.State != GameState.Playing) return;

            if (_input.FireHeld && _cooldown <= 0f)
            {
                Fire();
                _cooldown = _config.FireCooldown;
            }
        }

        public void Fire()
        {
            Vector2 direction = _aim.AimDirection;
            Vector2 origin = _player.NosePosition;

            Projectile projectile = _pool.Spawn(_projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, _config.ProjectileSpeed, _config.ProjectileLifetime);

            if (_muzzleEffectPrefab != null && _config.MuzzleFrames != null && _config.MuzzleFrames.Length > 0)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                OneShotEffect muzzle = _pool.Spawn(_muzzleEffectPrefab, origin, Quaternion.Euler(0f, 0f, angle));
                muzzle.Play(_config.MuzzleFrames, _config.MuzzleFps, Color.white);
            }

            _player.PlayShoot();
        }
    }
}
