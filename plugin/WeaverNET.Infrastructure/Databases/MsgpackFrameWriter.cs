using MessagePack;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using WeaverNet.Core.Plugins;

namespace WeaverNET.Infrastructure.Databases
{
    public class MsgpackFrameWriter : IChannelFileWriter<FrameData>
    {
        public string FileExtension => ".msgpack";

        /// <summary>
        /// To stop the exectuion you should close the stream
        /// </summary>
        public async Task WriteAsync(ChannelReader<FrameData> reader, FileInfo file)
        {
            file.Directory?.Create();

            using (var fileStream = File.Create(file.FullName))
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var frame))
                    {
                        byte[] msgPackBytes = MessagePackSerializer.Serialize(frame);
                        await fileStream.WriteAsync(msgPackBytes, 0, msgPackBytes.Length);
                    }
                }

                await fileStream.FlushAsync();
            }
        }
    }
}
