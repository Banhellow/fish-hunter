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
        [SerializeField] private PlayerFish player;
        [SerializeField] private PlayerAim aim;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private OneShotEffect muzzleEffectPrefab;

        private GameConfig _config;
        private InputService _input;
        private GameFlowService _flow;
        private PoolService _pool;
        private float _cooldown;

        private void Start()
        {
            if (player == null) player = GetComponent<PlayerFish>();
            if (aim == null) aim = GetComponent<PlayerAim>();
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
                _cooldown = _config.fireCooldown;
            }
        }

        public void Fire()
        {
            Vector2 direction = aim.AimDirection;
            Vector2 origin = player.NosePosition;

            Projectile projectile = _pool.Spawn(projectilePrefab, origin, Quaternion.identity);
            projectile.Launch(direction, _config.projectileSpeed, _config.projectileLifetime);

            if (muzzleEffectPrefab != null && _config.muzzleFrames != null && _config.muzzleFrames.Length > 0)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                OneShotEffect muzzle = _pool.Spawn(muzzleEffectPrefab, origin, Quaternion.Euler(0f, 0f, angle));
                muzzle.Play(_config.muzzleFrames, _config.muzzleFps, Color.white);
            }

            player.PlayShoot();
        }
    }
}
