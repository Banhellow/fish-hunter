namespace FishHunter.Gameplay
{
    /// <summary>Optional callbacks for components on pooled objects.</summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
