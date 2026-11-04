using System;
using System.Collections.Generic;

namespace FishHunter.UI.Core
{
    /// <summary>
    /// Base ViewModel: exposes observable state and commands to a View and keeps track of
    /// its own subscriptions to services so they are released on <see cref="Dispose"/>.
    /// </summary>
    public abstract class ViewModelBase : IDisposable
    {
        private readonly List<Action> _cleanup = new();
        private bool _disposed;

        /// <summary>Register an unsubscribe action that runs when the ViewModel is disposed.</summary>
        protected void AddCleanup(Action cleanup) => _cleanup.Add(cleanup);

        /// <summary>Called by the screen manager when its view becomes visible.</summary>
        public virtual void OnShown()
        {
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (Action action in _cleanup) action();
            _cleanup.Clear();
            OnDispose();
        }

        protected virtual void OnDispose()
        {
        }
    }
}
