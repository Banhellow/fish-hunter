using UnityEngine;

namespace FishHunter.Config
{
    /// <summary>Global tuning values. Edit the asset in Data/ to balance the game.</summary>
    [CreateAssetMenu(menuName = "Fish Hunter/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Player")]
        public int StartLives = 3;
        [Tooltip("Distance from the player's center to the nose, in world units.")]
        public float NoseOffset = 1.0f;
        public Sprite[] PlayerIdleFrames;
        public float PlayerIdleFps = 8f;
        public Sprite[] PlayerShootFrames;
        public float PlayerShootFps = 18f;
        public float HitFlashDuration = 0.4f;

        [Header("Projectile")]
        public float ProjectileSpeed = 14f;
        public float ProjectileLifetime = 3f;
        public float FireCooldown = 0.18f;
        public Sprite[] MuzzleFrames;
        public float MuzzleFps = 20f;

        [Header("Difficulty (linear over rampDuration seconds)")]
        public float RampDuration = 180f;
        public float SpawnIntervalStart = 1.6f;
        public float SpawnIntervalEnd = 0.35f;
        public float SpeedMultiplierStart = 1f;
        public float SpeedMultiplierEnd = 1.8f;

        [Header("Spawning")]
        [Tooltip("Hard minimum time between two spawns.")]
        public float MinSpawnGap = 0.25f;
        [Tooltip("Random +/- jitter applied to the spawn interval (fraction).")]
        [Range(0f, 0.5f)] public float SpawnIntervalJitter = 0.2f;
        [Tooltip("How far beyond the screen edge enemies appear.")]
        public float SpawnMargin = 1.5f;
        [Tooltip("Minimum distance between a new spawn and any living enemy.")]
        public float MinSpawnSpacing = 1.8f;
        [Tooltip("A faster enemy is not spawned in the same approach lane (degrees) as a slower one ahead of it.")]
        public float LaneAngle = 9f;
        public int SpawnAttempts = 10;
    }
}
