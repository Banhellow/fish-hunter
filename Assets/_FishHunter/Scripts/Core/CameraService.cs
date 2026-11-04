using UnityEngine;

namespace FishHunter.Core
{
    /// <summary>Wraps the gameplay camera: screen-to-world conversion and visible world bounds.</summary>
    public sealed class CameraService : IService
    {
        public Camera Camera { get; }

        public CameraService(Camera camera)
        {
            Camera = camera;
        }

        /// <summary>Half extents of the visible area in world units (camera is centered on the origin).</summary>
        public Vector2 HalfExtents
        {
            get
            {
                float halfHeight = Camera.orthographicSize;
                return new Vector2(halfHeight * Camera.aspect, halfHeight);
            }
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            Vector3 world = Camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.transform.position.z));
            return world;
        }

        public bool IsOutside(Vector2 position, float margin)
        {
            Vector2 half = HalfExtents;
            Vector2 center = Camera.transform.position;
            return Mathf.Abs(position.x - center.x) > half.x + margin || Mathf.Abs(position.y - center.y) > half.y + margin;
        }
    }
}
