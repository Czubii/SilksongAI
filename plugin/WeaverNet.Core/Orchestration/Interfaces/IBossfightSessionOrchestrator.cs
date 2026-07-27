using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    public interface IBossfightSessionOrchestrator
    {
        bool CanStart { get; }
        bool IsRunning { get; }
        Task StartAsync(BossfightSession session, CancellationToken ct);
        Task StopAsync();
    }
}
