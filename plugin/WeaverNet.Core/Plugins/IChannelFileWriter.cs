using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WeaverNet.Core.Plugins
{
    public interface IChannelFileWriter<TData>
    {
        string FileExtension { get; }
        Task WriteAsync(ChannelReader<TData> reader, FileInfo file);
    }
}
