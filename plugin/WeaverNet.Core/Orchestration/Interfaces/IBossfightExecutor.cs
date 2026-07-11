using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    /// <summary>
    /// Implements methods for single bossfight execution
    /// </summary>
    public interface IBossfightExecutor
    {
        Task AwaitFightStartAsync(string bossID, CancellationToken ct);
        Task<BossfightResult> AwaitFightEndAsync(string bossID, CancellationToken ct);

    }
}
