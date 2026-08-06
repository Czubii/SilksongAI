using System.Collections.Generic;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Plugins
{
    public class DefaultBossfightSession : IBossfightSessionPluginFactory
    {
        public string Id => _id;
        private readonly string _id;
        public DefaultBossfightSession(string id)
        {
            _id = id;
        }
        public IReadOnlyList<IBossfightSessionPlugin> CreatePlugins(BossfightSessionConfiguration config)
        {
            return new List<IBossfightSessionPlugin>(0); // empty
        }
    }
}
