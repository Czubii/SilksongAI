using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    /// <summary>
    /// Executes a single boss fight.
    /// Assumes the player is already in the arena and ready to engage.
    /// Waits for the boss to become active, monitors the fight until it concludes,
    /// and returns the fight result.
    /// </summary>
    public class BossFightOrchestrator 
    {
        private readonly IBossfightExecutor _executor;
        private readonly List<IBossfightPlugin> _plugins;
        public BossFightOrchestrator(IBossfightExecutor executor, List<IBossfightPlugin> plugins)
        {
            _executor = executor;
            _plugins = plugins;
        }
        public async Task<BossfightResult> RunAsync(FightContext ctx,CancellationToken ct)
        {
            try
            {
                using (var startTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    startTimeoutCts.CancelAfter(TimeSpan.FromSeconds(20));

                    await _executor.AwaitFightStartAsync(
                        ctx.Boss.ID,
                        startTimeoutCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested)
                    throw;

                return BossfightResult.BossNeverAppeared;
            }

            try
            {
                using (var fightTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    fightTimeoutCts.CancelAfter(TimeSpan.FromSeconds(400));

                    return await _executor.AwaitFightEndAsync(
                        ctx.Boss.ID,
                        fightTimeoutCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested)
                    throw;

                return BossfightResult.FightTimeout;
            }
        }

    }
}
