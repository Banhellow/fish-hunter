using FishHunter.Core;
using UnityEngine;

namespace FishHunter.Gameplay
{
    /// <summary>Rotates the player fish so its nose always points at the mouse cursor.</summary>
    [RequireComponent(typeof(PlayerFish))]
    public sealed class PlayerAim : MonoBehaviour
    {
        [SerializeField] private PlayerFish _player;

        private InputService _input;
        private CameraService _camera;
        private GameFlowService _flow;

        public Vector2 AimDirection { get; private set; } = Vector2.right;

        private void Start()
        {
            if (_player == null) _player = GetComponent<PlayerFish>();
            _input = ServiceLocator.Get<InputService>();
            _camera = ServiceLocator.Get<CameraService>();
            _flow = ServiceLocator.Get<GameFlowService>();
            _flow.SessionReset += ResetAim;
        }

        private void OnDestroy()
        {
            if (_flow != null) _flow.SessionReset -= ResetAim;
        }

        private void ResetAim() => AimDirection = Vector2.right;

        private void Update()
        {
            if (_flow.State != GameState.Playing) return;

            Vector2 pointer = _camera.ScreenToWorld(_input.PointerScreenPosition);
            Vector2 direction = pointer - _player.Position;
            if (direction.sqrMagnitude < 0.0001f) return;

            AimDirection = direction.normalized;
            _player.SetFacing(AimDirection);
        }
    }
}
