namespace FishHunter.Core
{
    /// <summary>Scene services that need an explicit init step after every service is registered.</summary>
    public interface IInitializable
    {
        void Initialize();
    }
}
