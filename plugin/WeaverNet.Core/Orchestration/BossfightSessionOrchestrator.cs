using System;
using System.Collections.Generic;
using System.Linq;
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
                PluginRuntime.State.IsSessionActive = true;

                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _runningTask = ExecuteAsync(session, _cts.Token);
            }

            return _runningTask;
        }
        public async Task StopAsync()
        {
            CancellationTokenSource cts = null;
            Task task = null;

            lock (_startLock)
            {
                if (IsRunning)
                {
                    cts = _cts;
                    task = _runningTask;
                }
            }
            try
            {
                _cts?.Cancel();
                await _runningTask;
            }
            catch (OperationCanceledException){ }
            finally
            {
                _cts?.Dispose();
            }
        }

        private async Task ExecuteAsync(BossfightSession session, CancellationToken ct)
        {
            var runtime = new BossfightSessionRuntime(session);

            PluginLog.Info($"Starting bossfight session for boss: {session.Boss.DisplayName}");

            try
            {
                _statusWriter.Start(session);
                using (GameStateScope scope = new GameStateScope())
                {
                    PluginLog.Info("Initializing session plugins...");
                    await NotifySessionStart(session);
                    PluginLog.Info("Session plugins initialized.");

                    PluginLog.Info("Preparing session state...");
                    await PrepareSession(session, scope.Modifier);
                    PluginLog.Info("Session preparation completed.");

                    var boundary = runtime.Session.Boundary;

                    PluginLog.Info("Beginning fight iterations.");

                    while (!boundary.ShouldTerminate(runtime) && !ct.IsCancellationRequested)
                    {
                        var fightContext = runtime.IterationStarted();
                        PluginLog.Info($"Starting fight iteration {runtime.CurrentIteration}.");

                        _statusWriter.UpdateProgress(runtime);

                        var result = await ExecuteFight(fightContext, scope.Modifier, ct);

                        runtime.IterationFinished(result);

                        PluginLog.Info(
                            $"Finished fight iteration {runtime.CurrentIteration} with result: {runtime.PreviousIterationResult()}.");
                    }

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
                    _statusWriter.Stop();
                    await PostSessionCleanup();
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Cleanup failed: {ex}");
                }
                PluginRuntime.State.IsSessionActive = false;
                PluginLog.Info("Bossfight session cleanup completed.");
            }
        }
        private async Task PrepareSession(BossfightSession session, TemporaryStateModifier modifier)
        {
            _controller.SelectLoadout(session.Loadout, modifier);
            _controller.SelectRespawnPoint(session.RespawnPoint, modifier);
        }
        private async Task<FightResult> ExecuteFight(
            FightContext context, 
            TemporaryStateModifier modifier, 
            CancellationToken ct)
        {
            var boss = context.Boss;
            _controller.RespawnBoss(boss, modifier);
            await _controller.TeleportToBossAsync(boss);

            var bossId = await _controller.WaitForBossAsync(context.Boss, ct); 
            var result = await _controller.AwaitFightFinishedAsync(bossId, ct);

            return result;
        }
        private async Task PostSessionCleanup()
        {
            await _controller.TeleportToBenchAsync();
        }
        private Task NotifySessionStart(BossfightSession session)
        {
            return NotifyPlugins(
                session.Plugins,
                plugin => plugin.OnSessionStart());
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
