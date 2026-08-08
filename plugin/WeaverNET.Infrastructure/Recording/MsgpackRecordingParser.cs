using MessagePack;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Plugins.FrameCapture;

namespace WeaverNET.Infrastructure.Databases
{
    public class MsgpackRecordingParser : IRecordingParser
    {
        public string FileExtension => ".msgpack";

        /// <summary>
        /// To stop the exectuion you should close the stream
        /// </summary>
        public async Task WriteAsync<TFrame>(ChannelReader<TFrame> reader, FileInfo file)
        {
            file.Directory?.Create();

            using (var fileStream = File.Create(file.FullName))
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var frame))
                    {
                        var options = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance); // todo FIND A WAY TO DEAL WITH THIS

                        byte[] msgPackBytes = MessagePackSerializer.Serialize(frame, options);
                        await fileStream.WriteAsync(msgPackBytes, 0, msgPackBytes.Length);
                    }
                }

                await fileStream.FlushAsync();
            }
        }

        public Task ReadAsync<TFrame>(ChannelWriter<TFrame> writer, FileInfo file)
        {
            throw new System.NotImplementedException();
        }

    }
}
