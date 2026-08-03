using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Plugins;

namespace WeaverNET.Infrastructure.Databases
{
    /// <summary>
    /// manages saving raw recordings
    /// </summary>
    public class RecordingManager: IRecordingManager // TODO in the future this will also use some databse controller to store the metadata
    {
        private readonly string _outputRootFolderPath;
        private readonly IChannelFileWriter<FrameData> _fileWriter;
        private FightContext _currentFight;
        private FileInfo _currentTempFile;
        private Channel<FrameData> _channel;
        private Task _writingTask;

        public RecordingManager(string outputRootFolderPath, IChannelFileWriter<FrameData> fileWriter)
        {
            _outputRootFolderPath = outputRootFolderPath ?? throw new ArgumentNullException(nameof(outputRootFolderPath));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        }
        public Task OnSessionStartAsync()
        {
            return Task.CompletedTask;
        }
        public Task OnFightStartAsync(FightContext context)
        {
            _currentFight = context;
            return Task.CompletedTask;
        }
        public ChannelWriter<FrameData> BeginStream()
        {
            _channel = Channel.CreateBounded<FrameData>(100);
            _currentTempFile = GetTempOutputFileInfo();

            // Store the task
            _writingTask = _fileWriter.WriteAsync(_channel.Reader, _currentTempFile);

            return _channel.Writer;
        }
        public async Task OnFightEndAsync(FightResult result)
        {
            if (_writingTask != null)
            {
                try
                {
                    await _writingTask;

                    FinalizeRecording();
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Failed to write recording file: {ex}");
                }
                finally
                {
                    _writingTask = null;
                    _currentTempFile = null;
                    _currentFight = null;
                }
            }
        }
        public Task OnSessionEndAsync()
        {
            return Task.CompletedTask;
        }
        private void FinalizeRecording()
        {
            var outputPath = GetOutputPath(_currentFight.Id, _currentFight.Boss.DisplayName);
            Directory.CreateDirectory(outputPath);

            var metadata = new RecordingMetadata(_currentFight);
            RenameTempRecording(Path.Combine(outputPath, $"recording{_fileWriter.FileExtension}"));
            WriteMetadata(Path.Combine(outputPath, "metadata.json"), metadata);
        }
        private void WriteMetadata(string outputFile, RecordingMetadata metadata)
        {
            var json = JsonConvert.SerializeObject(metadata, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            File.WriteAllText(outputFile, json);
        }
        private void RenameTempRecording(string outputFile)
        {
            if (_currentFight == null || _currentTempFile == null || !_currentTempFile.Exists)
                return;
            File.Move(_currentTempFile.FullName, outputFile);
        }
        private FileInfo GetTempOutputFileInfo()
        {
            return new FileInfo(
                Path.Combine(
                    _outputRootFolderPath,
                    $"bossfight_recording_{Guid.NewGuid()}{_fileWriter.FileExtension}.temp"));
        }
        private static string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name;
        }
        private string GetOutputPath(Guid recordingId, string bossName)
        {
            return Path.Combine(
                _outputRootFolderPath,
                SanitizeFileName(bossName),
                recordingId.ToString());
        }
    }
}
