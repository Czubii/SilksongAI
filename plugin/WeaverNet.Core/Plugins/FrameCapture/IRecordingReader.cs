using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WeaverNet.Core.Plugins.FrameCapture
{
    public interface IRecordingReader: IRecordingFileExtension
    {
        Task ReadAsync<TFrame>(ChannelWriter<TFrame> writer, FileInfo file);
    }
}
