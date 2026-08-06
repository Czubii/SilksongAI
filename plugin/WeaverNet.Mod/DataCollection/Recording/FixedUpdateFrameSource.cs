using System;
using System.Threading.Channels;
using UnityEngine;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Plugins.FrameCapture;

namespace WeaverNet.Mod.DataCollection
{
    public class FixedUpdateFrameSource<TFrame> : IFixedUpdateListener, IFixedUpdateFrameSource<TFrame>
    {
        private readonly IFrameGenerator<TFrame> _frameGenerator;
        private readonly IFixedUpdateEventSource _fixedUpdateSource;
        private readonly ChannelWriter<TFrame> _output;
        private readonly FightContext _fightContext;
        public FixedUpdateFrameSource(
            FightContext context, 
            IFrameGenerator<TFrame> generator, 
            ChannelWriter<TFrame> output, 
            IFixedUpdateEventSource fixedUpdateSource)
        {
            fixedUpdateSource.Register(this);
            _frameGenerator = generator;
            _output = output;
            _fixedUpdateSource = fixedUpdateSource;
            _fightContext = context;
        }
        public void Dispose()
        {
            _fixedUpdateSource.Unregister(this);
        }
        public void OnFixedUpdate()
        {
            if (_frameGenerator.TryGenerate(_fightContext, out var frame))
            {
                _output.TryWrite(frame);
            }
        }
    }
}
