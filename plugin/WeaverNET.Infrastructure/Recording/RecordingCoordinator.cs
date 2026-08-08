using Newtonsoft.Json;
using System;
using System.Diagnostics;
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
    /// manages creation and saving of the recordings
    /// </summary>
    public class RecordingCoordinator<TFrame> : IRecordingCoordinator<TFrame>
    {
        private readonly RecordingFrameTypeDefinition _recordingType;
        private readonly IRecordingRepository _recordingRepository;
        private readonly IRecordingWriter _fileWriter;
        private readonly string _tempDirectory;

        private FileInfo _currentTempFile;
        private FightContext _currentFight;

        private Channel<TFrame> _channel;
        private Task _writingTask;

        public RecordingCoordinator(string tempDirectory, RecordingFrameTypeDefinition recordingType, IRecordingWriter fileWriter, IRecordingRepository recordingRepository)
        {
            _tempDirectory = tempDirectory ?? throw new ArgumentNullException(nameof(tempDirectory));
            _recordingType = recordingType ?? throw new ArgumentNullException(nameof(recordingType));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
            _recordingRepository = recordingRepository ?? throw new ArgumentNullException(nameof(recordingRepository));

            if (_recordingType.FrameType != typeof(TFrame))
            {
                throw new ArgumentException(
                    $"Recording type '{_recordingType.Id}' expects frames of type {_recordingType.FrameType.Name}, " +
                    $"but coordinator uses {typeof(TFrame).Name}.");
            }
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
        public ChannelWriter<TFrame> BeginStream()
        {
            _channel = Channel.CreateBounded<TFrame>(100);
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
                    _recordingType.Id,
                    _recordingType.Version,
                    _currentFight.Boss.Id, 
                    _currentFight.StartTime, 
                    _currentFight.Loadout, 
                    result);

            var recording = new FileReference<BossfightRecordingMetadata>(
                _currentFight.Id, 
                metadata, 
                _currentTempFile, 
                _fileWriter.FileExtension);

            _recordingRepository.Import(recording);
        }
        private FileInfo GetTempOutputFileInfo()
        {
            Directory.CreateDirectory(_tempDirectory);

            return new FileInfo(
                Path.Combine(
                    _tempDirectory,
                    $"bossfight_recording{_fileWriter.FileExtension}.temp"));
        }
    }
}
