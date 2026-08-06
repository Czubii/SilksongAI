using System.Collections.Generic;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    public interface IBossfightSessionPluginFactory
    {
        string Id { get; }
        IReadOnlyList<IBossfightSessionPlugin> CreatePlugins(BossfightSessionConfiguration config);
    }
}
