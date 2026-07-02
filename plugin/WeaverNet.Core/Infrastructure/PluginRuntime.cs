using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Infrastructure
{
    public static class PluginRuntime
    {
        private static readonly object _lock = new object();
        private static PluginState _instance;

        public static PluginState State
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null) throw new InvalidOperationException("PluginRuntime not initialized");
                    return _instance;
                }
            }
        }

        public static void Initialize()
        {
            lock (_lock)
            {
                _instance = new PluginState();
            }
        }

        public static void Dispose()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }
    }
}
