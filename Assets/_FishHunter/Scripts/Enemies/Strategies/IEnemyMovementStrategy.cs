namespace FishHunter.Enemies.Strategies
{
    /// <summary>
    /// Strategy for how an enemy swims toward the player. Strategies are stateless and shared
    /// between all enemies of a kind; per-fish state lives on <see cref="Enemy"/>.
    /// </summary>
    public interface IEnemyMovementStrategy
    {
        void Tick(Enemy enemy, float deltaTime);
    }
}
