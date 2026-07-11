using System;
using WeaverNet.Core.Infrastructure.Interfaces;

namespace WeaverNet.Core.Infrastructure
{
    public static class PluginLog
    {
        private static IPluginLogger _logger;

        public static void Bind(IPluginLogger logger)
        {
            if (_logger != null)
                throw new InvalidOperationException("Logger already bound");

            _logger = logger;
        }

        public static void Info(string msg) => _logger?.Info(msg);
        public static void Warning(string msg) => _logger?.Warning(msg);
        public static void Error(string msg) => _logger?.Error(msg);
    }
}
