using System;

namespace FishHunter.UI.Core
{
    /// <summary>Runs an action once when disposed (used to undo bindings).</summary>
    public sealed class DisposableAction : IDisposable
    {
        private Action _onDispose;

        public DisposableAction(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
            _onDispose = null;
        }
    }
}
