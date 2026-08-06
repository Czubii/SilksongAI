using System;

namespace WeaverNet.Core.Plugins.FrameCapture
{
    public interface IFixedUpdateFrameSource<TFrame> : IDisposable // collects the game data, from the main thread (Ideally that should be by implementing MonoBehavior)
    {
    }
}
