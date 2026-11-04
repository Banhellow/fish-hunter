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
        [SerializeField] private SpriteRenderer _target;
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _framesPerSecond = 8f;
        [SerializeField] private bool _loop = true;
        [SerializeField] private bool _playOnEnable = true;
        [SerializeField] private bool _randomStartFrame;

        private float _timer;
        private int _index;
        private Action _onComplete;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            if (_target == null) _target = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (_playOnEnable && _frames != null && _frames.Length > 0)
                Play(_frames, _framesPerSecond, _loop, null, _randomStartFrame);
        }

        public void Play(Sprite[] newFrames, float fps, bool shouldLoop, Action onComplete = null, bool randomStart = false)
        {
            _frames = newFrames;
            _framesPerSecond = Mathf.Max(0.01f, fps);
            _loop = shouldLoop;
            _onComplete = onComplete;
            _timer = 0f;
            _index = randomStart && _frames != null && _frames.Length > 0 ? UnityEngine.Random.Range(0, _frames.Length) : 0;
            IsPlaying = _frames != null && _frames.Length > 0;
            if (IsPlaying) _target.sprite = _frames[_index];
        }

        private void Update()
        {
            if (!IsPlaying) return;

            _timer += Time.deltaTime;
            float frameDuration = 1f / _framesPerSecond;
            bool changed = false;

            while (_timer >= frameDuration)
            {
                _timer -= frameDuration;
                _index++;
                changed = true;

                if (_index < _frames.Length) continue;

                if (_loop)
                {
                    _index = 0;
                    continue;
                }

                _index = _frames.Length - 1;
                IsPlaying = false;
                _target.sprite = _frames[_index];
                Action callback = _onComplete;
                _onComplete = null;
                callback?.Invoke();
                return;
            }

            if (changed) _target.sprite = _frames[_index];
        }
    }
}
