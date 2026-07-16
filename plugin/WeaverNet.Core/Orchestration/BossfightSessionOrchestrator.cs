using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionOrchestrator
    {
        private readonly IBossfightSessionGameController _controller;
        private Task _runningTask;
        private readonly object _startLock = new object();
        private CancellationTokenSource _cts;
        BossfightSessionOrchestrator(IBossfightSessionGameController controller)
        {
            _controller = controller;
        }
        public Task StartAsync(BossfightSession session, CancellationToken ct)
        {
            lock (_startLock)
            {
                if (_runningTask != null && !_runningTask.IsCompleted)
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
                if (_runningTask != null && !_runningTask.IsCompleted)
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
            try
            {
                using (GameStateScope scope = new GameStateScope())
                {
                    await NotifySessionStart(session); // let the plugins load necessary data for example an ONNX ai model
                    await PrepareSession(session, scope.Modifier); // set abilities etc

                    var boundary = runtime.Session.Boundary; // object defining the stop criterion
                    while (!boundary.ShouldTerminate(runtime) && !ct.IsCancellationRequested)
                    {
                        var fightContext = runtime.IterationStarted();
                        await PrepareFight(runtime);

                        runtime.IterationFinished(FightResult.Exception);
                    }
                }
            }
            finally
            {
                PluginRuntime.State.IsSessionActive = false;
            }
        }
        private async Task PrepareSession(BossfightSession session, TemporaryStateModifier modifier)
        {
            await _controller.SelectAbilities(session.Loadout.Abilities, modifier);
            await _controller.SelectTools(session.Loadout.Tools, modifier);
            await _controller.SetRespawnPoint(session.RespawnPoint, modifier);
        }
        private async Task PrepareFight(BossfightSessionRuntime runtime)
        {

            await Task.Delay(5000);
            //var boss = runtime.Session.Boss;
            //if (runtime.FirstIteration)
            //{
            //    _controller.TeleportToArena(boss.ArenaSceneName, boss.ArenaPosition);
            //}

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
