using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Playables;

namespace WeaverNet.Core.Infrastructure
{
    internal class ObjectRegistry
    {
        private readonly Dictionary<Type, object> _plugins = new Dictionary<Type, object>();
        public void Register<T>(T plugin) where T : class
        {
            if (plugin == null)
            {
                throw new ArgumentNullException();
            }
            if (_plugins.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"Plugin of type '{typeof(T).Name}' is already registered.");
            }
            _plugins.Add(typeof(T), plugin);
        }
        public T Get<T>() where T : class
        {
            if (_plugins.TryGetValue(typeof(T), out var plugin))
            {
                return (T)plugin;
            }

            throw new KeyNotFoundException($"Plugin of type '{typeof(T).Name}' is not registered.");
        }
    }
}
