using UnityEngine;

namespace FishHunter.Enemies.Strategies
{
    /// <summary>
    /// Type C: high speed dart toward the player with a small lateral wiggle around the straight
    /// line. The wiggle fades out near the center so contact always happens head-on.
    /// </summary>
    public sealed class SwiftMovementStrategy : EnemyMovementStrategyBase
    {
        private const float FadeDistance = 4f;

        public override void Tick(Enemy enemy, float deltaTime)
        {
            EnemyTypeConfig type = enemy.Type;
            float remaining = AdvanceAlongPath(enemy, enemy.Speed, deltaTime);

            Vector2 forward = (enemy.Target - enemy.SpawnPosition).normalized;
            Vector2 side = new(-forward.y, forward.x);
            float fade = Mathf.Clamp01(remaining / FadeDistance);
            float offset = Mathf.Sin((enemy.Age * type.WiggleFrequency + enemy.Phase) * Mathf.PI * 2f) * type.WiggleAmplitude * fade;

            enemy.ApplyPosition(enemy.PathPosition + side * offset);
        }
    }
}
