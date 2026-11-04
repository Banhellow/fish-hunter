using FishHunter.Core;
using FishHunter.Gameplay;
using UnityEngine;

namespace FishHunter.Animation
{
    /// <summary>Pooled sprite effect (enemy death burst, muzzle bubbles) that despawns when its frames end.</summary>
    [RequireComponent(typeof(SpriteFrameAnimator))]
    public sealed class OneShotEffect : MonoBehaviour
    {
        [SerializeField] private SpriteFrameAnimator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<SpriteFrameAnimator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Play(Sprite[] frames, float fps, Color tint, bool flipY = false)
        {
            spriteRenderer.color = tint;
            spriteRenderer.flipY = flipY;
            animator.Play(frames, fps, false, Despawn);
        }

        private void Despawn()
        {
            if (ServiceLocator.TryGet(out PoolService pool)) pool.Despawn(this);
            else gameObject.SetActive(false);
        }
    }
}
