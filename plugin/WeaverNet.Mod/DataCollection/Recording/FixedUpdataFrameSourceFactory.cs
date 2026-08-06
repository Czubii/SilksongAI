using System.Threading.Channels;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Plugins.FrameCapture;
using WeaverNet.Mod.DataCollection;

public class FixedUpdateFrameSourceFactory<TFrame> : IFixedUpdataFrameSourceFactory<TFrame>
{
    private readonly IFrameGenerator<TFrame> _generator;
    private readonly IFixedUpdateEventSource _updateSource;

    public FixedUpdateFrameSourceFactory(
        IFrameGenerator<TFrame> generator,
        IFixedUpdateEventSource updateSource)
    {
        _generator = generator;
        _updateSource = updateSource;
    }

    public IFixedUpdateFrameSource<TFrame> Create(ChannelWriter<TFrame> output, FightContext context)
    {
        return new FixedUpdateFrameSource<TFrame>(context, _generator, output, _updateSource);
    }
}