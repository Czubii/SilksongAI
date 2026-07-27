using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game;
using WeaverNet.Core.Orchestration.Boundaries;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    public interface IBossfightSessionStatus
    {
        bool IsRunning { get; }
        BossData CurrentBoss { get; }
        BossfightSessionProgress Progress { get; }

        event Action StatusChanged;
    }
    public interface IBossfightSessionStatusWriter
    {
        void Start(BossfightSession session);
        void UpdateProgress(BossfightSessionRuntime runtime);
        void Stop();
    }
}
