using FishHunter.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FishHunter.UI.Screens
{
    public sealed class PauseView : ViewBase<PauseViewModel>
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button menuButton;

        protected override void OnBind(PauseViewModel viewModel)
        {
            BindCommand(resumeButton, viewModel.Resume);
            BindCommand(menuButton, viewModel.ReturnToMenu);
        }
    }
}
