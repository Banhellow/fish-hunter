using FishHunter.Core;
using FishHunter.UI.Core;

namespace FishHunter.UI.Screens
{
    public sealed class PauseViewModel : ViewModelBase
    {
        private readonly GameFlowService _flow;

        public PauseViewModel()
        {
            _flow = ServiceLocator.Get<GameFlowService>();
        }

        public void Resume() => _flow.Resume();

        public void ReturnToMenu() => _flow.ReturnToMenu();
    }
}
