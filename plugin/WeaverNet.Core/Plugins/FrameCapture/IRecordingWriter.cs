using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using WeaverNet.Core.Plugins;

namespace WeaverNet.Core.Infrastructure
{
    public interface IRecordingWriter: IRecordingFileExtension
    {
        Task WriteAsync<TData>(ChannelReader<TData> reader, FileInfo file);
    }
}
