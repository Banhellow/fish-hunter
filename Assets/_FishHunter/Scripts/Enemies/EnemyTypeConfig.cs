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
        public string DisplayName = "Type A";
        public EnemyMovementKind Movement = EnemyMovementKind.Standard;

        [Header("Speed (world units / sec, rolled per individual)")]
        public float SpeedMin = 1.5f;
        public float SpeedMax = 2f;

        [Header("Reward")]
        public int Points = 10;

        [Header("Spawn weight (lerped by difficulty progress)")]
        public float SpawnWeightStart = 1f;
        public float SpawnWeightEnd = 1f;

        [Header("Body")]
        public float ColliderRadius = 0.5f;

        [Header("Animation")]
        public Sprite[] SwimFrames;
        public float SwimFps = 8f;
        public Sprite[] DeathFrames;
        public float DeathFps = 14f;

        [Header("Strategy parameters")]
        [Tooltip("Fast: extra speed fraction during a surge.")]
        public float SurgeAmplitude = 0.7f;
        [Tooltip("Fast: surges per second.")]
        public float SurgeFrequency = 0.8f;
        [Tooltip("Swift: lateral wiggle in world units.")]
        public float WiggleAmplitude = 0.35f;
        [Tooltip("Swift: wiggles per second.")]
        public float WiggleFrequency = 2.2f;
    }
}
