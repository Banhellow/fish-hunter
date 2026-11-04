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
        private static readonly Dictionary<Type, IService> Services = new Dictionary<Type, IService>();

        public static void Register<T>(T service) where T : class, IService
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            var type = typeof(T);
            if (Services.ContainsKey(type))
                throw new InvalidOperationException($"Service {type.Name} is already registered.");
            Services[type] = service;
        }

        public static T Get<T>() where T : class, IService
        {
            if (Services.TryGetValue(typeof(T), out var service)) return (T)service;
            throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class, IService
        {
            if (Services.TryGetValue(typeof(T), out var s))
            {
                service = (T)s;
                return true;
            }

            service = null;
            return false;
        }

        public static bool IsRegistered<T>() where T : class, IService => Services.ContainsKey(typeof(T));

        public static void Unregister<T>() where T : class, IService => Services.Remove(typeof(T));

        public static void Clear() => Services.Clear();
    }
}
