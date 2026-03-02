using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Infrastructure
{
    public interface IServiceRegistry
    {
        T Get<T>() where T : class;
        void Add<T>(T instance) where T : class;
    }

    public sealed class ServiceRegistry : IServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Add<T>(T instance) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _services[typeof(T)] = instance;
        }

        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var instance))
                return (T) instance;

            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }
    }

}
