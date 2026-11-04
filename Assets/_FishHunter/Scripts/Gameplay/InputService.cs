using FishHunter.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FishHunter.Gameplay
{
    /// <summary>Reads mouse/keyboard through the Input System so gameplay code is device-agnostic.</summary>
    public sealed class InputService : IService
    {
        public Vector2 PointerScreenPosition => Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        /// <summary>Right mouse button held (fire, rate-limited by the shooter's cooldown).</summary>
        public bool FireHeld => Mouse.current != null && Mouse.current.rightButton.isPressed;

        public bool PausePressed => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }
}
