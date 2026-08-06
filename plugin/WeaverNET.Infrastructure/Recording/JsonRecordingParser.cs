using MessagePack;
using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using WeaverNet.Core.Plugins.FrameCapture;

namespace WeaverNET.Infrastructure.Databases
{
    public class JsonRecordingParser : IRecordingParser
    {
        MessagePackSerializerOptions _options;
        public string FileExtension => ".json";
        public JsonRecordingParser()
        {
            _options = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }
        /// <summary>
        /// Writes as long as the channel is not marked as completed
        /// </summary>
        public async Task WriteAsync<TFrame>(ChannelReader<TFrame> reader, FileInfo file)
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
                        // Serialize and write frame asynchronously
                        byte[] msgPackBytes = MessagePackSerializer.Serialize(frame, _options);
                        string jsonLine = MessagePackSerializer.ConvertToJson(msgPackBytes);

                        await streamWriter.WriteLineAsync(jsonLine);
                    }
                }
                // Flush remaining buffered data to disk before closing
                await streamWriter.FlushAsync();
            }
        }
        public async Task ReadAsync<TFrame>(ChannelWriter<TFrame> writer, FileInfo file)
        {
            try
            {
                using (var fileStream = File.OpenRead(file.FullName))
                using (var streamReader = new StreamReader(fileStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        string line = await streamReader.ReadLineAsync();

                        byte[] bytes =
                            MessagePackSerializer.ConvertFromJson(line);

                        TFrame frame =
                            MessagePackSerializer.Deserialize<TFrame>(
                                bytes,
                                _options);

                        await writer.WriteAsync(frame);
                    }
                }

                writer.Complete();
            }
            catch (Exception ex)
            {
                writer.Complete(ex);
                throw;
            }
        }
    }
}
