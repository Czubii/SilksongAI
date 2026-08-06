using WeaverNet.Core.Orchestration;

namespace WeaverNet.Core.Plugins.FrameCapture
{
    public interface IFrameGenerator<TFrame>
    {
        bool TryGenerate(FightContext context, out TFrame frame);
    }
}
