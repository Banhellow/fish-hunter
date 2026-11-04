namespace FishHunter.Enemies.Strategies
{
    /// <summary>Type A: steady, constant-speed swim straight at the player.</summary>
    public sealed class StandardMovementStrategy : EnemyMovementStrategyBase
    {
        public override void Tick(Enemy enemy, float deltaTime)
        {
            AdvanceAlongPath(enemy, enemy.Speed, deltaTime);
            enemy.ApplyPosition(enemy.PathPosition);
        }
    }
}
