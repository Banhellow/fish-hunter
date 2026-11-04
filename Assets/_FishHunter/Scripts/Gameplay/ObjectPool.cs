using System.Collections.Generic;
using UnityEngine;

namespace FishHunter.Gameplay
{
    public interface IObjectPool
    {
        void Release(Component instance);
        void ReleaseAll();
    }

    /// <summary>Simple component pool: reuses deactivated instances of a prefab.</summary>
    public sealed class ObjectPool<T> : IObjectPool where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactive = new();
        private readonly HashSet<T> _active = new();
        private readonly List<T> _releaseBuffer = new();
        private readonly List<IPoolable> _callbackBuffer = new();

        public int ActiveCount => _active.Count;

        public ObjectPool(T prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance = _inactive.Count > 0 ? _inactive.Pop() : Object.Instantiate(_prefab, _parent);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            _active.Add(instance);

            instance.GetComponents(_callbackBuffer);
            foreach (IPoolable poolable in _callbackBuffer) poolable.OnSpawned();
            return instance;
        }

        public void Release(Component instance)
        {
            var typed = (T)instance;
            if (!_active.Remove(typed)) return;

            typed.GetComponents(_callbackBuffer);
            foreach (IPoolable poolable in _callbackBuffer) poolable.OnDespawned();

            typed.gameObject.SetActive(false);
            _inactive.Push(typed);
        }

        public void ReleaseAll()
        {
            _releaseBuffer.Clear();
            _releaseBuffer.AddRange(_active);
            foreach (T instance in _releaseBuffer) Release(instance);
        }
    }
}
