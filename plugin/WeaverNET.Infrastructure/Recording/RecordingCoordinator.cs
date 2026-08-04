using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Plugins;
using WeaverNet.Core.Plugins.Recording;

namespace WeaverNET.Infrastructure.Databases
{
    /// <summary>
    /// manages saving raw recordings
    /// </summary>
    public class RecordingCoordinator: IRecordingCoordinator
    {
        private readonly IRecordingRepository _recordingRepository;
        private readonly IChannelFileWriter<RecordingFrame> _fileWriter;
        private readonly string _tempDirectory;

        private FileInfo _currentTempFile;
        private FightContext _currentFight;

        private Channel<RecordingFrame> _channel;
        private Task _writingTask;

        public RecordingCoordinator(string tempDirectory, IChannelFileWriter<RecordingFrame> fileWriter, IRecordingRepository recordingRepository)
        {
            _tempDirectory = tempDirectory ?? throw new ArgumentNullException(nameof(tempDirectory));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _recordingRepository = recordingRepository ?? throw new ArgumentNullException(nameof(recordingRepository));
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
        public ChannelWriter<RecordingFrame> BeginStream()
        {
            _channel = Channel.CreateBounded<RecordingFrame>(100);
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

                    FinalizeRecording(result);
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
        private void FinalizeRecording(FightResult result)
        {
            var metadata = 
                new BossfightRecordingMetadata(
                    _currentFight.Id, 
                    _currentFight.Boss.Id, 
                    _currentFight.StartTime, 
                    _currentFight.Loadout, result, 
                    _fileWriter.FileExtension);
            var recording = new BossfightRecording(metadata, _currentTempFile);
            _recordingRepository.Add(recording);
        }
        private FileInfo GetTempOutputFileInfo()
        {
            Directory.CreateDirectory(_tempDirectory);

            return new FileInfo(
                Path.Combine(
                    _tempDirectory,
                    $"bossfight_recording_{Guid.NewGuid()}{_fileWriter.FileExtension}.temp"));
        }
    }
}
