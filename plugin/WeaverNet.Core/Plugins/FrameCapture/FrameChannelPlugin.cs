using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Core.Plugins;
using WeaverNet.Core.Plugins.FrameCapture;

namespace WeaverNet.Mod.DataCollection
{
    public class FrameChannelPlugin<TFrame> : IBossfightSessionPlugin
    {
        private readonly IReadOnlyCollection<IFrameChannelPluginSink<TFrame>> _listeners;
        private readonly IFixedUpdataFrameSourceFactory<TFrame> _frameSourceFactory;
        private CancellationTokenSource _cts;
        private Task _runningTask;

        public FrameChannelPlugin(IFixedUpdataFrameSourceFactory<TFrame> frameSourceFactory, IReadOnlyCollection<IFrameChannelPluginSink<TFrame>> listeners)
        {
            _listeners = listeners ?? throw new ArgumentNullException(nameof(listeners));
            _frameSourceFactory = frameSourceFactory ?? throw new ArgumentNullException(nameof(frameSourceFactory));
        }

        public async Task OnSessionStartAsync()
        {
            foreach (var listener in _listeners)
            {
                if (listener != null) await listener.OnSessionStartAsync();
            }
        }

        public async Task OnFightStartAsync(FightContext context)
        {
            if (_runningTask != null)
            {
                await Stop();
            }

            foreach (var listener in _listeners)
            {
                if (listener != null) await listener.OnFightStartAsync(context);
            }

            _cts = new CancellationTokenSource();
            _runningTask = DistributeFramesAsync(context, _cts.Token);
        }

        public async Task OnFightEndAsync(FightResult result)
        {
            await Stop();

            foreach (var listener in _listeners)
            {
                if (listener != null) await listener.OnFightEndAsync(result);
            }
        }

        public async Task OnSessionEndAsync()
        {
            foreach (var listener in _listeners)
            {
                if (listener != null) await listener.OnSessionEndAsync();
            }
        }

        private async Task Stop()
        {
            var task = _runningTask;
            if (task == null) return;

            _cts?.Cancel();

            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // Expected on cancellation
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _runningTask = null;
            }
        }

        private List<ChannelWriter<TFrame>> NotifyStreamStart()
        {
            var channels = new List<ChannelWriter<TFrame>>(_listeners.Count);
            foreach (var listener in _listeners)
            {
                if (listener == null) continue;

                var writer = listener.BeginStream();
                if (writer != null)
                {
                    channels.Add(writer);
                }
            }
            return channels;
        }

        private void PostFrame(TFrame frame, List<ChannelWriter<TFrame>> writers)
        {
            for (int i = 0; i < writers.Count; i++)
            {
                writers[i].TryWrite(frame);
            }
        }

        private async Task DistributeFramesAsync(FightContext context, CancellationToken ct)
        {
            var sourceChannel = Channel.CreateBounded<TFrame>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true
                });

            using (IFixedUpdateFrameSource<TFrame> FrameSource = _frameSourceFactory.Create(sourceChannel.Writer, context))
            {
                var sinks = NotifyStreamStart();
                try
                {
                    while (await sourceChannel.Reader.WaitToReadAsync(ct))
                    {
                        while (sourceChannel.Reader.TryRead(out var frame))
                        {
                            PostFrame(frame, sinks);
                        }
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // Normal shutdown
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Error in frame distribution loop: {ex}");
                }
                finally
                {
                    for (int i = 0; i < sinks.Count; i++)
                    {
                        sinks[i].TryComplete();
                    }
                }
            }
            
        }
    }
}