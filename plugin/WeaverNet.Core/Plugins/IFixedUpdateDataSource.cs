using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WeaverNet.Core.Plugins
{
    public interface IFixedUpdateDataSource<TData> // collects the game data, from the main thread (Ideally that should be by implementing MonoBehavior)
    {
        bool IsRunning { get; }
        void Attach(ChannelWriter<TData> output);
        void Detach();
    }
}
