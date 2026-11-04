using System;
using System.Collections.Generic;

namespace FishHunter.Core
{
    /// <summary>
    /// Central registry for game services. Services are registered once by <see cref="GameBootstrap"/>
    /// and resolved by type everywhere else, so systems never reference each other through singletons.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _services = new();

        public static void Register<T>(T service) where T : class, IService
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
                throw new InvalidOperationException($"Service {type.Name} is already registered.");
            _services[type] = service ?? throw new ArgumentNullException(nameof(service));
        }

        public static T Get<T>() where T : class, IService
        {
            if (_services.TryGetValue(typeof(T), out IService service)) return (T)service;
            throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class, IService
        {
            if (_services.TryGetValue(typeof(T), out IService s))
            {
                service = (T)s;
                return true;
            }

            service = null;
            return false;
        }

        public static bool IsRegistered<T>() where T : class, IService => _services.ContainsKey(typeof(T));

        public static void Unregister<T>() where T : class, IService => _services.Remove(typeof(T));

        public static void Clear() => _services.Clear();
    }
}
