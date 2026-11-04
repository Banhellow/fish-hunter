using UnityEngine;

namespace FishHunter.Config
{
    /// <summary>Global tuning values. Edit the asset in Data/ to balance the game.</summary>
    [CreateAssetMenu(menuName = "Fish Hunter/Game Config", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Player")]
        public int startLives = 3;
        [Tooltip("Distance from the player's center to the nose, in world units.")]
        public float noseOffset = 1.0f;
        public Sprite[] playerIdleFrames;
        public float playerIdleFps = 8f;
        public Sprite[] playerShootFrames;
        public float playerShootFps = 18f;
        public float hitFlashDuration = 0.4f;

        [Header("Projectile")]
        public float projectileSpeed = 14f;
        public float projectileLifetime = 3f;
        public float fireCooldown = 0.18f;
        public Sprite[] muzzleFrames;
        public float muzzleFps = 20f;

        [Header("Difficulty (linear over rampDuration seconds)")]
        public float rampDuration = 180f;
        public float spawnIntervalStart = 1.6f;
        public float spawnIntervalEnd = 0.35f;
        public float speedMultiplierStart = 1f;
        public float speedMultiplierEnd = 1.8f;

        [Header("Spawning")]
        [Tooltip("Hard minimum time between two spawns.")]
        public float minSpawnGap = 0.25f;
        [Tooltip("Random +/- jitter applied to the spawn interval (fraction).")]
        [Range(0f, 0.5f)] public float spawnIntervalJitter = 0.2f;
        [Tooltip("How far beyond the screen edge enemies appear.")]
        public float spawnMargin = 1.5f;
        [Tooltip("Minimum distance between a new spawn and any living enemy.")]
        public float minSpawnSpacing = 1.8f;
        [Tooltip("A faster enemy is not spawned in the same approach lane (degrees) as a slower one ahead of it.")]
        public float laneAngle = 9f;
        public int spawnAttempts = 10;
    }
}
