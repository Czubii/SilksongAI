using System.Collections.Generic;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Plugins
{
    public class DefaultBossfightSession : IBossfightSessionType
    {
        public string Type => "default";
        public IReadOnlyList<IBossfightSessionPlugin> CreatePlugins(BossfightSessionConfiguration config)
        {
            return new List<IBossfightSessionPlugin>(0); // empty
        }
    }
}
