using System.Threading.Channels;
using WeaverNet.Core.Orchestration;

namespace WeaverNet.Core.Plugins.FrameCapture
{
    public interface IFixedUpdataFrameSourceFactory<TFrame>
    {
        IFixedUpdateFrameSource<TFrame> Create(ChannelWriter<TFrame> output, FightContext context);
    }
}
