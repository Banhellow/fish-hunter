using FishHunter.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FishHunter.UI.Screens
{
    public sealed class PauseView : ViewBase<PauseViewModel>
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _menuButton;

        protected override void OnBind(PauseViewModel viewModel)
        {
            BindCommand(_resumeButton, viewModel.Resume);
            BindCommand(_menuButton, viewModel.ReturnToMenu);
        }
    }
}
