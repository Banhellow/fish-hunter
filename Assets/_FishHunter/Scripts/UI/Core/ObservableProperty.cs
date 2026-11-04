using System;
using System.Collections.Generic;

namespace FishHunter.UI.Core
{
    /// <summary>Bindable value for ViewModels. Views subscribe and get notified on change.</summary>
    public sealed class ObservableProperty<T>
    {
        private T _value;

        public event Action<T> Changed;

        public ObservableProperty(T initialValue = default)
        {
            _value = initialValue;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;
                _value = value;
                Changed?.Invoke(value);
            }
        }

        /// <summary>Subscribes and (by default) immediately pushes the current value.</summary>
        public IDisposable Subscribe(Action<T> handler, bool invokeImmediately = true)
        {
            Changed += handler;
            if (invokeImmediately) handler(_value);
            return new DisposableAction(() => Changed -= handler);
        }
    }
}
