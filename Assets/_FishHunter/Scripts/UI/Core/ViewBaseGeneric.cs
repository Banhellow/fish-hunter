using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FishHunter.UI.Core
{
    /// <summary>
    /// Typed View bound to a ViewModel. Subclasses implement <see cref="OnBind"/> and use the
    /// Bind helpers; every binding is undone automatically on rebind or destroy.
    /// </summary>
    public abstract class ViewBase<TViewModel> : ViewBase where TViewModel : ViewModelBase
    {
        private readonly List<IDisposable> _bindings = new();

        protected TViewModel ViewModel { get; private set; }

        public void Bind(TViewModel viewModel)
        {
            Unbind();
            ViewModel = viewModel;
            if (viewModel != null) OnBind(viewModel);
        }

        protected abstract void OnBind(TViewModel viewModel);

        protected override void OnShow()
        {
            ViewModel?.OnShown();
        }

        protected void BindProperty<T>(ObservableProperty<T> property, Action<T> handler)
        {
            _bindings.Add(property.Subscribe(handler));
        }

        protected void BindCommand(Button button, Action command)
        {
            // UnityEvent removes listeners by target + method, so both conversions of OnClick match.
            void OnClick() => command();
            button.onClick.AddListener(OnClick);
            _bindings.Add(new DisposableAction(() => button.onClick.RemoveListener(OnClick)));
        }

        private void Unbind()
        {
            foreach (IDisposable binding in _bindings) binding.Dispose();
            _bindings.Clear();
            ViewModel?.Dispose();
            ViewModel = null;
        }

        protected virtual void OnDestroy() => Unbind();
    }
}
