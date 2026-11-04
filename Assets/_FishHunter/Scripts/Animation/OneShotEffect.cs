using FishHunter.Core;
using FishHunter.Gameplay;
using UnityEngine;

namespace FishHunter.Animation
{
    /// <summary>Pooled sprite effect (enemy death burst, muzzle bubbles) that despawns when its frames end.</summary>
    [RequireComponent(typeof(SpriteFrameAnimator))]
    public sealed class OneShotEffect : MonoBehaviour
    {
        [SerializeField] private SpriteFrameAnimator _animator;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<SpriteFrameAnimator>();
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Play(Sprite[] frames, float fps, Color tint, bool flipY = false)
        {
            _spriteRenderer.color = tint;
            _spriteRenderer.flipY = flipY;
            _animator.Play(frames, fps, false, Despawn);
        }

        private void Despawn()
        {
            if (ServiceLocator.TryGet(out PoolService pool)) pool.Despawn(this);
            else gameObject.SetActive(false);
        }
    }
}
