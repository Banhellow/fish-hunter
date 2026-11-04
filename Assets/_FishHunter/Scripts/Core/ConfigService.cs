using FishHunter.Config;

namespace FishHunter.Core
{
    /// <summary>Exposes the game's tuning data to every system through the service locator.</summary>
    public sealed class ConfigService : IService
    {
        public GameConfig Game { get; }

        public ConfigService(GameConfig game)
        {
            Game = game;
        }
    }
}
