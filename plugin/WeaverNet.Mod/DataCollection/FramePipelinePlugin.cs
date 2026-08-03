using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Plugins;

namespace WeaverNet.Mod.DataCollection
{
    public class FramePipelinePlugin : IFramePipelinePlugin
    {
        private readonly IReadOnlyCollection<IFrameSink> _listeners;
        private readonly IFixedUpdateDataSource<FrameData> _frameSource;
        private CancellationTokenSource _cts;
        private Task _runningTask;

        public FramePipelinePlugin(IFixedUpdateDataSource<FrameData> frameSource, IReadOnlyCollection<IFrameSink> listeners)
        {
            _listeners = listeners ?? throw new ArgumentNullException(nameof(listeners));
            _frameSource = frameSource ?? throw new ArgumentNullException(nameof(frameSource));
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
            _runningTask = DistributeFramesAsync(_cts.Token);
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

        private List<ChannelWriter<FrameData>> NotifyStreamStart()
        {
            var channels = new List<ChannelWriter<FrameData>>(_listeners.Count);
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

        private void PostFrame(FrameData frame, List<ChannelWriter<FrameData>> writers)
        {
            for (int i = 0; i < writers.Count; i++)
            {
                writers[i].TryWrite(frame);
            }
        }

        private async Task DistributeFramesAsync(CancellationToken ct)
        {
            var source = Channel.CreateBounded<FrameData>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = true
                });

            _frameSource.Attach(source.Writer);
            var sinks = NotifyStreamStart();

            try
            {
                while (await source.Reader.WaitToReadAsync(ct))
                {
                    while (source.Reader.TryRead(out var frame))
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
                _frameSource.Detach();
                for (int i = 0; i < sinks.Count; i++)
                {
                    sinks[i].TryComplete();
                }
            }
        }
    }
}