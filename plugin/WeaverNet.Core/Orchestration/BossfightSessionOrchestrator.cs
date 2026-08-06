using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionOrchestrator: IBossfightSessionOrchestrator
    {
        private readonly IBossfightSessionGameController _controller;
        private readonly IBossfightSessionStatusWriter _statusWriter;
        private Task _runningTask;
        private readonly object _startLock = new object();
        private CancellationTokenSource _cts;
        public BossfightSessionOrchestrator(
            IBossfightSessionGameController controller,
            IBossfightSessionStatusWriter statusWriter)
        {
            _statusWriter = statusWriter;
            _controller = controller;
        }
        public bool IsRunning => _runningTask != null && !_runningTask.IsCompleted;
        public bool CanStart => _runningTask == null || _runningTask.IsCompleted; // TODO add check for game state via the controller
        public Task StartAsync(BossfightSession session, CancellationToken ct)
        {
            lock (_startLock)
            {
                if (!CanStart)
                {
                    throw new InvalidOperationException("Bossfight session already running");
                }

                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _runningTask = ExecuteAsync(new BossfightSessionRuntime(session), _cts.Token);
            }

            return _runningTask;
        }
        public async Task StopAsync()
        {
            Task task;
            CancellationTokenSource cts;

            lock (_startLock)
            {
                task = _runningTask;
                cts = _cts;
            }

            if (task == null)
                return;

            cts.Cancel();

            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                cts.Dispose();
            }
        }

        private async Task ExecuteAsync(BossfightSessionRuntime runtime, CancellationToken ct)
        {

            PluginLog.Info($"Starting bossfight session for boss: {runtime.Session.Boss.DisplayName}");

            try
            {
                _statusWriter.Start(runtime.Session);
                using (GameStateScope scope = new GameStateScope())
                {
                    PluginLog.Info("Initializing session plugins...");
                    await NotifySessionStart(runtime.Session);
                    PluginLog.Info("Session plugins initialized.");

                    PluginLog.Info("Preparing session state...");
                    await PrepareSession(runtime.Session, scope.Modifier);
                    PluginLog.Info("Session preparation completed.");

                    PluginLog.Info("Beginning fight iterations.");
                    await ExecuteSession(runtime, scope.Modifier, ct);

                    if (ct.IsCancellationRequested)
                    {
                        PluginLog.Info("Bossfight session cancelled.");
                    }
                    else
                    {
                        PluginLog.Info("Bossfight session boundary reached.");
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Bossfight session failed:\n{ex}");
                throw;
            }
            finally
            {
                try
                {
                    //TODO: await "can stop".
                    //Now for example if game is paused and one stops the session the game freezes in a weird loading state state
                    _statusWriter.Stop();
                    await PostSessionCleanup(runtime);
                    await NotifySessionEnd(runtime.Session);
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Cleanup failed: {ex}");
                }
                PluginLog.Info("Bossfight session cleanup completed.");
            }
        }
        private async Task PrepareSession(BossfightSession session, TemporaryStateModifier modifier)
        {
            _controller.TryRemoveCocoon();
            _controller.SelectLoadout(session.Loadout, modifier);
            _controller.SelectRespawnPoint(session.RespawnPoint, modifier);
        }
        private async Task ExecuteSession(BossfightSessionRuntime runtime, TemporaryStateModifier modifier, CancellationToken ct)
        {
            var boundary = runtime.Session.Boundary;
            while (!boundary.ShouldTerminate(runtime) && !ct.IsCancellationRequested)
            {
                var fightContext = runtime.IterationStarted();

                PluginLog.Info($"Starting fight iteration {runtime.CurrentIteration}.");
                _statusWriter.UpdateProgress(runtime);

                await PrepareFight(runtime, modifier, ct);

                await NotifyFightStart(runtime.Session, fightContext);
                var result = await ExecuteFight(fightContext, ct);
                await NotifyFightEnd(runtime.Session, result);

                if (result == FightResult.Failure)
                {
                    await _controller.AwaitCocoonAndRemoveAsync();
                }

                runtime.IterationFinished(result);

                PluginLog.Info(
                    $"Finished fight iteration {runtime.CurrentIteration} with result: {runtime.PreviousIterationResult()}.");
            }
        }
        private async Task PrepareFight(BossfightSessionRuntime runtime, TemporaryStateModifier modifier, CancellationToken ct)
        {
            await _controller.AwaitCanTeleportAsync();
            var boss = runtime.Session.Boss;    
            if (runtime.FirstIteration)
            {
                _controller.RespawnBoss(boss, modifier);
                await _controller.TeleportToBossAsync(boss);
                PluginLog.Info($"Teleported player for the first iteration");
            }
            else if (runtime.PreviousIterationResult() == FightResult.Failure)
            {
                await _controller.AwaitPlayerRespawnedAsync();
                PluginLog.Info($"Player respawned.");
                if (!boss.CanRespawnOnArena)
                {
                    await _controller.TeleportToBossAsync(boss);
                    PluginLog.Info($"Teleported player to the arena because the respawn point was not on it");
                }
            }
            else
            {
                _controller.RespawnBoss(boss, modifier);
                await _controller.TeleportToBossAsync(boss);
                PluginLog.Info($"Teleported player");
            }
            _controller.ReplenishPlayerResources();
        }
        private async Task<FightResult> ExecuteFight(
            FightContext context, 
            CancellationToken ct)
        {
            var boss = context.Boss;
            using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                try
                {
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));
                    var bossId = await _controller.WaitForBossAsync(boss, timeoutCts.Token);
                    PluginLog.Info("Boss found");
                    var result = await _controller.AwaitFightFinishedAsync(bossId, ct);
                    return result;
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    PluginLog.Warning($"'{context.Boss.DisplayName}' Not found!");
                    return FightResult.BossMissing;
                }
            }
        }
        private async Task PostSessionCleanup(BossfightSessionRuntime runtime)
        {
            if(runtime.PreviousIterationResult() != FightResult.Failure)
            {
                await _controller.TeleportToBenchAsync();
            }
        }
        private Task NotifySessionStart(BossfightSession session)
        {
            return NotifyPlugins(
                session.Plugins,
                plugin => plugin.OnSessionStartAsync());
        }
        private Task NotifyFightStart(BossfightSession session, FightContext context)
        {
            return NotifyPlugins(
                session.Plugins,
                plugin => plugin.OnFightStartAsync(context));
        }
        private Task NotifyFightEnd(BossfightSession session, FightResult result)
        {
            return NotifyPlugins(
                session.Plugins,
                plugin => plugin.OnFightEndAsync(result));
        }
        private Task NotifySessionEnd(BossfightSession session)
        {

            return NotifyPlugins(
                session.Plugins,
                plugin => plugin.OnSessionEndAsync());
        }
        private async Task NotifyPlugins(IEnumerable<IBossfightSessionPlugin> plugins, Func<IBossfightSessionPlugin, Task> callback)
        {
            var tasks = plugins.Select(callback);

            try
            {
                await Task.WhenAll(tasks);
            }
            catch
            {
                throw;
            }
        }
    }
}
