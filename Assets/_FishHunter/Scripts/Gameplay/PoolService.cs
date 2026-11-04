using System.Collections.Generic;
using FishHunter.Core;
using UnityEngine;

namespace FishHunter.Gameplay
{
    /// <summary>Owns one <see cref="ObjectPool{T}"/> per prefab; used for enemies, projectiles and effects.</summary>
    public sealed class PoolService : IService
    {
        private readonly Transform _root;
        private readonly Dictionary<Component, IObjectPool> _poolsByPrefab = new Dictionary<Component, IObjectPool>();
        private readonly Dictionary<Component, IObjectPool> _poolsByInstance = new Dictionary<Component, IObjectPool>();

        public PoolService(Transform root)
        {
            _root = root;
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            if (!_poolsByPrefab.TryGetValue(prefab, out var pool))
            {
                pool = new ObjectPool<T>(prefab, _root);
                _poolsByPrefab[prefab] = pool;
            }

            T instance = ((ObjectPool<T>)pool).Get(position, rotation);
            _poolsByInstance[instance] = pool;
            return instance;
        }

        public void Despawn(Component instance)
        {
            if (instance == null) return;
            if (_poolsByInstance.TryGetValue(instance, out var pool)) pool.Release(instance);
            else Object.Destroy(instance.gameObject);
        }

        public void DespawnAll()
        {
            foreach (var pool in _poolsByPrefab.Values) pool.ReleaseAll();
        }
    }
}
