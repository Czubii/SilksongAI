using System.Collections.Generic;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    public interface IBossfightSessionType
    {
        string Type { get; }
        IReadOnlyList<IBossfightSessionPlugin> CreatePlugins(BossfightSessionConfiguration config);
    }
}
