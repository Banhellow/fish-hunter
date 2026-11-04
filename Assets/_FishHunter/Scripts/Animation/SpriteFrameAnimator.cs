using System;
using UnityEngine;

namespace FishHunter.Animation
{
    /// <summary>
    /// Lightweight frame-by-frame sprite animation (pool friendly, no Animator overhead).
    /// Runs on scaled time, so it freezes while the game is paused.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 8f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool randomStartFrame;

        private float _timer;
        private int _index;
        private bool _playing;
        private Action _onComplete;

        public bool IsPlaying => _playing;
        public Sprite[] Frames => frames;

        private void Awake()
        {
            if (target == null) target = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (playOnEnable && frames != null && frames.Length > 0)
                Play(frames, framesPerSecond, loop, null, randomStartFrame);
        }

        public void Play(Sprite[] newFrames, float fps, bool shouldLoop, Action onComplete = null, bool randomStart = false)
        {
            frames = newFrames;
            framesPerSecond = Mathf.Max(0.01f, fps);
            loop = shouldLoop;
            _onComplete = onComplete;
            _timer = 0f;
            _index = randomStart && frames != null && frames.Length > 0 ? UnityEngine.Random.Range(0, frames.Length) : 0;
            _playing = frames != null && frames.Length > 0;
            if (_playing) target.sprite = frames[_index];
        }

        public void Stop()
        {
            _playing = false;
            _onComplete = null;
        }

        private void Update()
        {
            if (!_playing) return;

            _timer += Time.deltaTime;
            float frameDuration = 1f / framesPerSecond;
            bool changed = false;

            while (_timer >= frameDuration)
            {
                _timer -= frameDuration;
                _index++;
                changed = true;

                if (_index < frames.Length) continue;

                if (loop)
                {
                    _index = 0;
                    continue;
                }

                _index = frames.Length - 1;
                _playing = false;
                target.sprite = frames[_index];
                var callback = _onComplete;
                _onComplete = null;
                callback?.Invoke();
                return;
            }

            if (changed) target.sprite = frames[_index];
        }
    }
}
