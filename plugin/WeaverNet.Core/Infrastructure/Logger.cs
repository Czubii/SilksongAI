using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Infrastructure
{
    public interface IPluginLogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }
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
        public static void Warn(string msg) => _logger?.Warning(msg);
        public static void Error(string msg) => _logger?.Error(msg);
    }
}
