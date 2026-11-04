using UnityEngine;

namespace FishHunter.Enemies
{
    public enum EnemyMovementKind
    {
        Standard,
        Fast,
        Swift
    }

    /// <summary>Data for one enemy type (A/B/C). Balance values live in the assets under Data/.</summary>
    [CreateAssetMenu(menuName = "Fish Hunter/Enemy Type", fileName = "EnemyType")]
    public sealed class EnemyTypeConfig : ScriptableObject
    {
        public string displayName = "Type A";
        public EnemyMovementKind movement = EnemyMovementKind.Standard;

        [Header("Speed (world units / sec, rolled per individual)")]
        public float speedMin = 1.5f;
        public float speedMax = 2f;

        [Header("Reward")]
        public int points = 10;

        [Header("Spawn weight (lerped by difficulty progress)")]
        public float spawnWeightStart = 1f;
        public float spawnWeightEnd = 1f;

        [Header("Body")]
        public float colliderRadius = 0.5f;

        [Header("Animation")]
        public Sprite[] swimFrames;
        public float swimFps = 8f;
        public Sprite[] deathFrames;
        public float deathFps = 14f;

        [Header("Strategy parameters")]
        [Tooltip("Fast: extra speed fraction during a surge.")]
        public float surgeAmplitude = 0.7f;
        [Tooltip("Fast: surges per second.")]
        public float surgeFrequency = 0.8f;
        [Tooltip("Swift: lateral wiggle in world units.")]
        public float wiggleAmplitude = 0.35f;
        [Tooltip("Swift: wiggles per second.")]
        public float wiggleFrequency = 2.2f;
    }
}
