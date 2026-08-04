using MessagePack;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Plugins;

namespace WeaverNET.Infrastructure.Databases
{
    public class JsonRecordingFrameWriter : IChannelFileWriter<RecordingFrame>
    {
        public string FileExtension => ".json";

        /// <summary>
        /// To stop the exectuion you should close the stream
        /// </summary>
        public async Task WriteAsync(ChannelReader<RecordingFrame> reader, FileInfo file)
        {
            file.Directory?.Create();

            using (var fileStream = File.Create(file.FullName))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                // The file remains open while reading from the channel
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var frame))
                    {
                        var options = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
                        // Serialize and write frame asynchronously
                        byte[] msgPackBytes = MessagePackSerializer.Serialize(frame, options);
                        string jsonLine = MessagePackSerializer.ConvertToJson(msgPackBytes);

                        await streamWriter.WriteLineAsync(jsonLine);
                    }
                }
                // Flush remaining buffered data to disk before closing
                await streamWriter.FlushAsync();
            }
        }
    }
}
