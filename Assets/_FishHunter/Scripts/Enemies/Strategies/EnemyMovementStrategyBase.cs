using UnityEngine;

namespace FishHunter.Enemies.Strategies
{
    /// <summary>Shared helper: advance the enemy's path point straight toward its target.</summary>
    public abstract class EnemyMovementStrategyBase : IEnemyMovementStrategy
    {
        public abstract void Tick(Enemy enemy, float deltaTime);

        /// <returns>Remaining distance to the target after moving.</returns>
        protected static float AdvanceAlongPath(Enemy enemy, float speed, float deltaTime)
        {
            Vector2 toTarget = enemy.Target - enemy.PathPosition;
            float distance = toTarget.magnitude;
            float step = speed * deltaTime;

            if (distance <= step)
            {
                enemy.PathPosition = enemy.Target;
                return 0f;
            }

            enemy.PathPosition += toTarget / distance * step;
            return distance - step;
        }
    }
}
