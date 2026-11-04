using UnityEngine;

namespace FishHunter.Enemies.Strategies
{
    /// <summary>Type B: medium speed with periodic tail-flick surges, still heading straight at the player.</summary>
    public sealed class FastMovementStrategy : EnemyMovementStrategyBase
    {
        public override void Tick(Enemy enemy, float deltaTime)
        {
            var type = enemy.Type;
            float wave = Mathf.Sin((enemy.Age * type.surgeFrequency + enemy.Phase) * Mathf.PI * 2f);
            float surge = Mathf.Pow(Mathf.Max(0f, wave), 3f);
            float speed = enemy.Speed * (1f + type.surgeAmplitude * surge);

            AdvanceAlongPath(enemy, speed, deltaTime);
            enemy.ApplyPosition(enemy.PathPosition);
        }
    }
}
