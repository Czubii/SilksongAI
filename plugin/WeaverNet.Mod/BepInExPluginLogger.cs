using BepInEx.Logging;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod
{
    public class BepInExPluginLogger : IPluginLogger
    {
        private readonly ManualLogSource _logger;
        // Dedicated lock object to synchronize threads
        private readonly object _lock = new object();
        public BepInExPluginLogger(ManualLogSource logger)
        {
            _logger = logger;
        }
        public void Info(string message)
        {
            lock (_lock)
            {
                _logger.LogInfo(message);
            }
        }
        public void Warning(string message)
        {
            lock (_lock)
            {
                _logger.LogWarning(message);
            }
        }
        public void Error(string message)
        {
            lock (_lock)
            {
                _logger.LogError(message);
            }
        }
    }
}