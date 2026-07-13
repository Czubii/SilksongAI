using System;

namespace WeaverNet.Core.Infrastructure
{
    public static class PluginRuntime
    {
        private static PluginState _instance;
        public static PluginState State
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("PluginRuntime not initialized");

                return _instance;
            }
        }
        public static void Initialize()
        {
            _instance = new PluginState();
        }
        public static void Dispose()
        {
            _instance = null;
        }
    }
}
