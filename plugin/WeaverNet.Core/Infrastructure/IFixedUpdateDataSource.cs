using System.Threading.Channels;

namespace WeaverNet.Core.Infrastructure
{
    public interface IFixedUpdateDataSource<TData> // collects the game data, from the main thread (Ideally that should be by implementing MonoBehavior)
    {
        bool IsRunning { get; }
        void Attach(ChannelWriter<TData> output);
        void Detach();
    }
}
