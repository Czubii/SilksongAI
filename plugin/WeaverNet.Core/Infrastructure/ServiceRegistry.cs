using System;
using System.Collections.Concurrent;

namespace WeaverNET.Infrastructure
{
    public class ServiceRegistry
    {
        private readonly ConcurrentDictionary<Type, object> _services = new ConcurrentDictionary<Type, object>();

        public void Register<TService>(TService implementation)
            where TService : class
        {
            _services[typeof(TService)] = implementation;
        }

        public TService Get<TService>()
            where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out var service))
            {
                return (TService)service;
            }

            throw new InvalidOperationException(
                $"No service registered for {typeof(TService).Name}"
            );
        }

        public bool Has<TService>()
            where TService : class
        {
            return _services.ContainsKey(typeof(TService));
        }
    }
}
